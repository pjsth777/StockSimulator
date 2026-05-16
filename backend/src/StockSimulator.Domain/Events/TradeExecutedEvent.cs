namespace StockSimulator.Domain.Events;

public class TradeExecutedEvent
{
    public Guid TransactionID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsBuy { get; set; }
    public DateTime Timestamp { get; set; }
}
