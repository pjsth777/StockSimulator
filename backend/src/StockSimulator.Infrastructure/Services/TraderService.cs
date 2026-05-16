using Microsoft.EntityFrameworkCore;
using StockSimulator.Domain.DTO;
using StockSimulator.Domain.Entities;
using StockSimulator.Infrastructure.Data;
using StockSimulator.Infrastructure.Services.IServices;
using NATS.Net;
using System.Text.Json;
using StockSimulator.Domain.Events;
using NATS.Client.JetStream.Models;

namespace StockSimulator.Infrastructure.Services;

public class TraderService : ITraderService
{
    private readonly ApplicationDbContext _context;

    public TraderService(ApplicationDbContext context)
    {
        _context = context;        
    }

    public async Task<bool> ExecuteTradeAsync(TraderRequestDTO dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol.ToUpper() == dto.Symbol.ToUpper());
            if (stock == null) throw new ArgumentException("Stock Symbol not found.");

            var portfolio = await _context.UserPortfolios
                .FirstOrDefaultAsync(p => p.Username.ToLower() == dto.Username.ToLower());

            if (portfolio == null) throw new ArgumentException("User portfolio not found.");

            decimal totalCost = stock.CurrentPrice * dto.Quantity;

            var existingItem = await _context.PortfolioItems.FirstOrDefaultAsync(i => i.UserPortfolioId == portfolio.Id && i.StockId == stock.Id);

            if (dto.IsBuy)
            {
                if (portfolio.AvailableCash < totalCost)
                {
                    throw new InvalidOperationException("Insufficient funds to complete this purchase.");
                }

                portfolio.AvailableCash -= totalCost;

                if (existingItem != null)
                {
                    decimal totalSpent = (existingItem.AverageBuyPrice * existingItem.Quantity) + totalCost;
                    existingItem.Quantity += dto.Quantity;
                    existingItem.AverageBuyPrice = Math.Round(totalSpent / existingItem.Quantity, 2);

                    _context.PortfolioItems.Update(existingItem);
                }
                else
                {
                    var newItem = new PortfolioItem
                    {
                        Id = Guid.NewGuid(),
                        UserPortfolioId = portfolio.Id,
                        StockId = stock.Id,
                        Quantity = dto.Quantity,
                        AverageBuyPrice = stock.CurrentPrice
                    };

                    await _context.PortfolioItems.AddAsync(newItem);
                }
            }
            else
            {
                if (existingItem == null || existingItem.Quantity < dto.Quantity)
                {
                    throw new InvalidOperationException("You do not own enough shares to sell this quantity.");
                }

                portfolio.AvailableCash += totalCost;
                existingItem.Quantity -= dto.Quantity;

                if (existingItem.Quantity == 0)
                {
                    _context.PortfolioItems.Remove(existingItem);
                }
                else
                {
                    _context.PortfolioItems.Update(existingItem);
                }
            }

            _context.UserPortfolios.Update(portfolio);

            var transactionReceipt = new Transaction
            {
                Id = Guid.NewGuid(),

                UserId = portfolio.UserId,
                StockId = stock.Id,

                Username = portfolio.Username,
                Symbol = stock.Symbol.ToUpper(),
                Quantity = dto.Quantity,
                PricePerShare = stock.CurrentPrice,
                IsBuy = dto.IsBuy,
                ExecuteAt = DateTime.UtcNow
            };

            await _context.Transactions.AddAsync(transactionReceipt);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            try
            {
                await using var natsClient = new NatsClient("nats://localhost:4222");

                var js = natsClient.CreateJetStreamContext();

                await js.CreateStreamAsync(new StreamConfig(
                    name: "TRADE_EVENTS",
                    subjects: new[] { "trades.executed" }
                ));

                var tradeEvent = new TradeExecutedEvent
                {
                    TransactionID = transactionReceipt.Id,
                    Username = transactionReceipt.Username,
                    Symbol = transactionReceipt.Symbol,
                    Quantity = transactionReceipt.Quantity,
                    PricePerShare = transactionReceipt.PricePerShare,
                    TotalAmount = transactionReceipt.TotalAmount,
                    IsBuy = transactionReceipt.IsBuy,
                    Timestamp = transactionReceipt.ExecuteAt
                };

                string serializedEvent = JsonSerializer.Serialize(tradeEvent);

                await js.PublishAsync(subject: "trades.executed", data:serializedEvent);
            }
            catch (Exception natsEx)
            {
                Console.WriteLine($"[NATS JetStream Warning]: Failed to broadcast trade message: {natsEx.Message}");
            }
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

}
