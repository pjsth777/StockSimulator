# Stock Simulator - Event-Driven Auditing System

An enterprise-grade microservice architecture demonstrating real-time stock transaction auditing using an asynchronous event-driven pattern powered by **NATS JetStream** and **.NET BackgroundServices**.

## Core Architectural Value
This system utilizes asynchronous decoupling to separate core business transactions from secondary processes (auditing, metrics compilation, notifications). 
* **High Availability:** The Web API process accepts user transactions, records them to the primary database, and immediately fires an event to the message broker before returning a `200 OK` response. 
* **Fault Isolation:** If the auditing worker experiences high lag, system upgrades, or an unexpected crash, the main user-facing application continues running seamlessly without processing degradation.

## Reliability & Resilience Guarantees

### 1. At-Least-Once Delivery
Using NATS JetStream, we enforce explicit acknowledgment logic (`ConsumerConfigAckPolicy.Explicit`). NATS delivers a message to the worker but **retains it** in safe buffer storage until the worker processes 
it and explicitly returns `await msg.AckAsync()`. If the worker drops offline mid-execution, the unacknowledged message is automatically safely re-queued.

### 2. Persistent Durable Consumers
The background worker attaches using a configured `DurableName` (`AuditWorkerConsumer`). This prompts NATS to track the state of this specific subscriber. If the consumer goes down, NATS keeps state tracking 
active, accumulating traffic and preparing a backlog queue to deliver immediately upon worker reactivation.

### 3. Stream Persistence
Unlike ephemeral, memory-only pub/sub systems, NATS JetStream stores messages safely on physical disk. In the event of an infrastructure-wide power loss or server cluster restart, no state or historical 
transaction data is lost.

---

## 🛠️ Simulating Downtime (Demo Guide)

To easily prove the architectural resilience to team leads or stakeholders without shutting down core database instances, use the integrated `IsCrashed` execution block:

1. **Verify Live Baseline:** Run the application stack and execute a transaction via the UI. Notice the immediate output confirmation log:
   ```text
   warn: StockSimulator.Infrastructure.Workers.TradeAuditWorker[0]
         [AUDIT ALERT] Transaction Verified! ID: 77023a25... | User: User2 | Action: BUY
   ```

2. **Simulate a Crash:** In Visual Studio, click Pause (Break All), navigate to the Immediate Window (Debug > Windows > Immediate), type the following command, and press Enter:
  ```bash
  StockSimulator.Infrastructure.Workers.TradeAuditWorker.IsCrashed = true
  ```

3. **Queue Messages Offline:** Perform 3 separate transactions on your UI frontend. The UI works instantly, but no logs appear on the worker side.
   
4. **Trigger Recovery:** Pause execution again, set the flag back to normal using the Immediate Window:
   ```csharp
   StockSimulator.Infrastructure.Workers.TradeAuditWorker.IsCrashed = false
   ```
   Click Continue.

5. **Observe the Resiliency:** The worker boots back instantly, connects to the existing durable consumer, pulls down the unacknowledged backlog queue, and processes all missed transactions immediately.
