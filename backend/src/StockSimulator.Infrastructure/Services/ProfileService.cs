using Microsoft.EntityFrameworkCore;
using StockSimulator.Domain.DTO;
using StockSimulator.Infrastructure.Data;
using StockSimulator.Infrastructure.Services.IServices;

namespace StockSimulator.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;

    public ProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDTO?> GetProfileByUsernameAsync(string username)
    {
        var portfolio = await _context.UserPortfolios
            .Include(p => p.Items)
            .ThenInclude(i => i.Stock)
            .FirstOrDefaultAsync(p => p.Username.ToLower() == username.ToLower());

        if (portfolio == null) return null;

        var holdings = portfolio.Items.Select(item => new HoldingItemDTO
        {
            Symbol = item.Stock?.Symbol ?? "UNKNOWN",
            CompanyName = item.Stock?.Name ?? "Unknown Company",
            Quantity = item.Quantity,
            AverageBuyPrice = item.AverageBuyPrice,
            CurrentPrice = item.Stock?.CurrentPrice ?? 0.00m
        }).ToList();

        decimal totalHoldingsValue = holdings.Sum(h => h.TotalValue);

        return new UserProfileDTO
        {
            Username = portfolio.Username,
            AvailableCash = portfolio.AvailableCash,
            TotalPortfolioValue = Math.Round(portfolio.AvailableCash + totalHoldingsValue, 2),
            Holdings = holdings
        };
    }
}
