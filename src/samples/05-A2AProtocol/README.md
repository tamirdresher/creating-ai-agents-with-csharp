# 05 — A2A Protocol (Agent-to-Agent)

Demonstrates the Agent-to-Agent protocol concept for inter-agent communication.

## What You'll Learn

- The A2A protocol model: Agent Cards, message exchange, task lifecycle
- Dispatching work to multiple specialist agents concurrently
- How agents can operate as independent services

## Architecture

```
User Code → [CodeReviewer] ──┬──→ Review Results
            [DocWriter]    ──┘──→ Documentation
```

Both agents process the same input concurrently, simulating remote A2A dispatch.

## A2A in Production

In a real A2A deployment:
1. Agents are hosted as HTTP services
2. Each agent publishes an **Agent Card** at `/.well-known/agent.json`
3. Clients discover capabilities and send messages via the A2A protocol
4. Responses can be synchronous or task-based (long-running)

## Run

```bash
cp .env.example .env
dotnet run
```

## Corresponding Notebook

📓 [6-Agent-to-Agent-Protocol.ipynb](../../../notebooks/6-Agent-to-Agent-Protocol.ipynb)
