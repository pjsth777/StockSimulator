using NATS.Net;
using StockSimulator.Domain.DTO;
using System.Text.Json;

namespace StockSimulator.Infrastructure.Messaging;

public class NatsPricePublisher
{
    private readonly NatsClient _natsClient;
    private const string Subject = "stocks.price.updated";

    public NatsPricePublisher()
    {
        _natsClient = new NatsClient("nats://localhost:4222");
    }

    public async Task PublishPriceUpdateAsync(string symbol, decimal newPrice)
    {
        var priceEvent = new StockPriceUpdatedEvent
        {
            Symbol = symbol,
            NewPrice = newPrice,
            TimeStamp = DateTime.UtcNow
        };

        string jsonPayload = JsonSerializer.Serialize(priceEvent);

        await _natsClient.PublishAsync(Subject, jsonPayload);
    }
}
