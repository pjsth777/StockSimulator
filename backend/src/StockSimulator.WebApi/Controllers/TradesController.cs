using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSimulator.Domain.DTO;
using StockSimulator.Infrastructure.Data;
using StockSimulator.Infrastructure.Services.IServices;

namespace StockSimulator.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradesController : ControllerBase
{
    private readonly ITraderService _traderService;
    private readonly ApplicationDbContext _context;

    public TradesController(
        ITraderService traderService,
        ApplicationDbContext context
        )
    {
        _traderService = traderService;
        _context = context;
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteTrade([FromBody] TraderRequestDTO dto)
    {
        try
        {
            await _traderService.ExecuteTradeAsync(dto);
            return Ok(new { Message = $"Trade executed successfully: {(dto.IsBuy ? "Bought" : "Sold")} {dto.Quantity} shares of {dto.Symbol.ToUpper()}" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "An error occurred while executing the transaction." });
        }
    }

    [HttpGet("history/{username}")]
    public async Task<IActionResult> GetTransactionHistory(string username)
    {
        var history = await _context.Transactions
            .Where(t => t.Username.ToLower() == username.ToLower())
            .OrderByDescending(t => t.ExecuteAt)
            .ToListAsync();

        return Ok(history);
    }
}
