namespace StockSimulator.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public Guid StockId { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal TotalAmount => Quantity * PricePerShare;
    public bool IsBuy { get; set; }
    public DateTime ExecuteAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; }
    public Stock Stock { get; set; }
}
