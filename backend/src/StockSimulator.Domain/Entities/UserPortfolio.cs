namespace StockSimulator.Domain.Entities;

public class UserPortfolio
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal AvailableCash { get; set; }

    public ICollection<PortfolioItem> Items { get; set; } = new List<PortfolioItem>();
    public User User { get; set; }
}
