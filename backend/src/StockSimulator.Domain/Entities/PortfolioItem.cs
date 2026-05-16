namespace StockSimulator.Domain.Entities;

public class PortfolioItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserPortfolioId { get; set; }
    public Guid StockId { get; set; }
    public int Quantity { get; set; }
    public decimal AverageBuyPrice { get; set; }

    public UserPortfolio? UserPortfolio { get; set; }
    public Stock? Stock { get; set; }
}
