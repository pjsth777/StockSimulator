using Microsoft.EntityFrameworkCore;
using StockSimulator.Domain.Entities;

namespace StockSimulator.Infrastructure.Data;

public class DbSeeder
{
    private readonly ApplicationDbContext _context;

    public DbSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Ensure the database matches our migrations
        await _context.Database.MigrateAsync();

        // Only seed if the Stocks table is completely empty
        if (!await _context.Stocks.AnyAsync())
        {
            var defaultStocks = new List<Stock>
            {
                new() { Symbol = "AAPL", Name = "Apple Inc.", CurrentPrice = 175.50m },
                new() { Symbol = "MSFT", Name = "Microsoft Corporation", CurrentPrice = 420.25m },
                new() { Symbol = "TSLA", Name = "Tesla Inc.", CurrentPrice = 180.10m },
                new() { Symbol = "GOOGL", Name = "Alphabet Inc.", CurrentPrice = 150.75m },
                new() { Symbol = "NVDA", Name = "NVIDIA Corporation", CurrentPrice = 875.00m },
            };

            await _context.Stocks.AddRangeAsync(defaultStocks);
            await _context.SaveChangesAsync();
        }
    }
}
