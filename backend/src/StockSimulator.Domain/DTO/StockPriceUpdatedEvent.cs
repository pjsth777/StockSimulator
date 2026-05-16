namespace StockSimulator.Domain.DTO;

public class StockPriceUpdatedEvent
{
    public string Symbol { get; set; } = string.Empty;
    public decimal NewPrice { get; set; }
    public DateTime TimeStamp { get; set; }
}
