# 09-AgentWithAspire

Host an `AIAgent` in an **ASP.NET Core** web service and get full observability via the **Aspire dashboard** — distributed traces, metrics, and structured logs — with zero instrumentation code beyond registering OpenTelemetry.

## What you'll learn

- How to register `IChatClient` and `AIAgent` as DI singletons
- How to wire up OpenTelemetry for Agent Framework + MEAI sources
- How to expose an agent via minimal API endpoints
- How Aspire orchestrates the Worker and injects OTLP config automatically

## Projects

| Project | Purpose |
|---------|---------|
| `09-AgentWithAspire.AppHost` | Aspire host — declares resources and starts the dashboard |
| `09-AgentWithAspire.Worker` | ASP.NET Core app — hosts the agent and exposes `/chat` |

## Prerequisites

- .NET 9 SDK
- Aspire workload: `dotnet workload install aspire`
- An OpenAI API key

## Setup

```bash
# Set env vars or user secrets for the AppHost
cd 09-AgentWithAspire.AppHost
dotnet user-secrets set "OPENAI_API_KEY" "sk-..."
dotnet user-secrets set "OPENAI_MODEL_ID" "gpt-4o"
```

## Run (via Aspire)

```bash
cd 09-AgentWithAspire.AppHost
dotnet run
# Open the Aspire dashboard URL shown in the console
```

## Endpoints (Worker)

```bash
# Single-turn chat
curl -X POST "http://localhost:5000/chat?message=Hello+world"

# Streaming (SSE)
curl -N "http://localhost:5000/chat/stream?message=Explain+LINQ"

# Health check
curl "http://localhost:5000/health"
```

## Telemetry

The Aspire dashboard shows:
- **Traces** — each `/chat` request with agent spans and tool calls
- **Metrics** — request count, latency histograms, token usage
- **Logs** — structured logs from all resources in one view

OTel sources wired up:
```
Microsoft.Agents.*       ← Agent Framework
Microsoft.Extensions.AI  ← MEAI (IChatClient calls)
Microsoft.AspNetCore     ← HTTP request spans
```

## Next steps

This is the last sample! Review [MIGRATION.md](../../../MIGRATION.md) for the full API mapping reference.
