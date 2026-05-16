using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockSimulator.Infrastructure.Data;
using StockSimulator.Infrastructure.Messaging;

namespace StockSimulator.Infrastructure.BackgroundServices;

public class StockPriceSimulatorWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly NatsPricePublisher _pricePublisher;
    private readonly ILogger<StockPriceSimulatorWorker> _logger;
    private readonly Random _random = new();

    public StockPriceSimulatorWorker(
        IServiceProvider services,
        NatsPricePublisher pricePublisher,
        ILogger<StockPriceSimulatorWorker> logger
        )
    {
        _services = services;
        _pricePublisher = pricePublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Price Simulator Worker is starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var stocks = await context.Stocks.ToListAsync(stoppingToken);

                if (stocks.Any())
                {
                    var stock = stocks[_random.Next(stocks.Count)];

                    decimal changePercent = (decimal)(_random.NextDouble() * 0.03 - 0.015);
                    decimal priceChange = stock.CurrentPrice * changePercent;
                    stock.CurrentPrice = Math.Round(stock.CurrentPrice + priceChange, 2);

                    await context.SaveChangesAsync(stoppingToken);
                    //_logger.LogInformation("Updated DB: {Symbol} is now ${Price}", stock.Symbol, stock.CurrentPrice);

                    await _pricePublisher.PublishPriceUpdateAsync(stock.Symbol, stock.CurrentPrice);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while simulating price movements.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }
}
