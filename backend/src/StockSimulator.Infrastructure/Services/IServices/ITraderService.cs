using StockSimulator.Domain.DTO;

namespace StockSimulator.Infrastructure.Services.IServices;

public interface ITraderService
{
    Task<bool> ExecuteTradeAsync(TraderRequestDTO dto);
}
