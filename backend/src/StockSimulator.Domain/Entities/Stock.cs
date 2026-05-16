namespace StockSimulator.Domain.Entities;

public class Stock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
