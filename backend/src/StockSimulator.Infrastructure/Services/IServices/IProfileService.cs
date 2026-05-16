using StockSimulator.Domain.DTO;

namespace StockSimulator.Infrastructure.Services.IServices;

public interface IProfileService
{
    Task<UserProfileDTO?> GetProfileByUsernameAsync(string username);
}
