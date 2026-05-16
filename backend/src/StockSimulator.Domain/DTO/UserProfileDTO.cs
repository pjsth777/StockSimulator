namespace StockSimulator.Domain.DTO;

public class UserProfileDTO
{
    public string Username { get; set; } = string.Empty;
    public decimal AvailableCash { get; set; }
    public decimal TotalPortfolioValue { get; set; }
    public List<HoldingItemDTO> Holdings { get; set; } = new();
}

public class HoldingItemDTO
{
    public string Symbol { get; set; }
    public string CompanyName { get; set; }
    public int Quantity { get; set; }
    public decimal AverageBuyPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal TotalValue => Quantity * CurrentPrice;
    public decimal ProfitLoss => Math.Round((CurrentPrice - AverageBuyPrice) * Quantity, 2);
}
