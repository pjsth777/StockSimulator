using StockSimulator.Domain.DTO;
using StockSimulator.Domain.Entities;

namespace StockSimulator.Infrastructure.Services.IServices;

public interface IUserService
{
    Task<IList<User>> GetUsers();
    Task<User> GetUser(string email);
    Task<User?> RegisterUserAsync(RegisterUserDTO dto);
}
