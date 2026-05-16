using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.JetStream.Models;
using NATS.Net;
using StockSimulator.Domain.Events;
using System.Text.Json;

namespace StockSimulator.Infrastructure.Workers;

public class TradeAuditWorker : BackgroundService
{
    // DEMO ONLY: Toggle to true via the debugger to simulate service downtime
    public static bool IsCrashed { get; set; } = false;

    private readonly ILogger<TradeAuditWorker> _logger;

    public TradeAuditWorker(ILogger<TradeAuditWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Trade Audit Worker successfully initialized with NATS JetStream...");

        await using var natsClient = new NatsClient("nats://localhost:4222");

        var js = natsClient.CreateJetStreamContext();

        await js.CreateStreamAsync(new StreamConfig(
            name: "TRADE_EVENTS",
            subjects: new[] { "trades.executed" }
        ), cancellationToken: stoppingToken);

        var consumer = await js.CreateOrUpdateConsumerAsync(
            stream: "TRADE_EVENTS",
            config: new ConsumerConfig
            {
                Name = "AuditWorkerConsumer",
                DurableName = "AuditWorkerConsumer",
                AckPolicy = ConsumerConfigAckPolicy.Explicit
            },
            cancellationToken: stoppingToken
        );

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            await foreach (var msg in consumer.ConsumeAsync<string>().WithCancellation(stoppingToken))
            {
                // DEMO INTERCEPTOR: Simulates consumer lag/downtime safely
                while (IsCrashed)
                {
                    await Task.Delay(1000, stoppingToken);
                }

                var jsonPayload = msg.Data;
                var tradeEvent = JsonSerializer.Deserialize<TradeExecutedEvent>(jsonPayload, jsonOptions);

                if (tradeEvent != null)
                {
                    string action = tradeEvent.IsBuy ? "BUY" : "SELL";

                    _logger.LogWarning(
                        "[AUDIT ALERT] Transaction Verified! ID: {TxId} | User: {User} | Action: {Action} | Symbol: {Symbol} | Total: ${Amount}",
                        tradeEvent.TransactionID,
                        tradeEvent.Username,
                        action,
                        tradeEvent.Symbol,
                        tradeEvent.TotalAmount.ToString("F2")
                    );

                    await msg.AckAsync(cancellationToken: stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Trade Audit Worker is shutting down gracefully.");
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "An error occurred during JetStream message consumption.");
        }
    }
}
