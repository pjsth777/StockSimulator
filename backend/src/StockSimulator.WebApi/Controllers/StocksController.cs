using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NATS.Net;
using StockSimulator.Domain.Entities;
using StockSimulator.Infrastructure.Data;

namespace StockSimulator.WebApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StocksController> _logger;

    public StocksController(
        ApplicationDbContext context,
        ILogger<StocksController> logger
        )
    {
        _context = context;
        _logger = logger;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
    {
        var stocks = await _context.Stocks.ToListAsync();
        return Ok(stocks);
    }

    [HttpGet("{symbol}")]
    public async Task<ActionResult<Stock>> GetStock(string symbol)
    {
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.Symbol.ToUpper() == symbol.ToUpper());

        if (stock == null)
        {
            return NotFound(new { Message = $"Stock with symbol {symbol} not found." });
        }

        return Ok(stock);
    }

    [HttpGet("stream")]
    public async Task GetStockStream(CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var natsClient = new NatsClient("nats://localhost:4222");

        _logger.LogInformation("New frontend SSE Client connected to NATS stream.");

        try
        {
            await foreach (var msg in natsClient.SubscribeAsync<string>("stocks.price.updated").WithCancellation(cancellationToken))
            {
                var jsonData = msg.Data;

                await Response.WriteAsync($"data: {jsonData}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Frontend client disconnected from SSE stream gracefully.");
        }
        catch (Exception ex)
        {
            _logger.LogInformation("An error occurred during the SSE stock streaming context execution.");
        }
        finally
        {
            await natsClient.DisposeAsync();
        }
    }

}
