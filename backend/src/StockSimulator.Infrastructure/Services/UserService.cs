using Microsoft.EntityFrameworkCore;
using StockSimulator.Domain.DTO;
using StockSimulator.Domain.Entities;
using StockSimulator.Infrastructure.Data;
using StockSimulator.Infrastructure.Services.IServices;

namespace StockSimulator.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IList<User>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return users;
    }

    public async Task<User> GetUser(string email)
    {
        var users = await _context.Users.Where(t => t.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
        return users;
    }

    public async Task<User?> RegisterUserAsync(RegisterUserDTO dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() || u.Username.ToLower() == dto.Username.ToLower()))
        {
            throw new InvalidOperationException("Username or Emai is alreday taken.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = dto.Password,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var newPortfolio = new UserPortfolio
            {
                Username = newUser.Username,
                UserId = newUser.Id,
                AvailableCash = 10000.00m
            };

            await _context.UserPortfolios.AddAsync(newPortfolio);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return newUser;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
