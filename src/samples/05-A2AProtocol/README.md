# 05-A2AProtocol

Connect to a **remote agent** using the [Agent-to-Agent (A2A) protocol](https://github.com/google-a2a/A2A).
A2A is a vendor-neutral standard for inter-agent communication — your agent can call any A2A-compatible service regardless of what framework built it.

## What you'll learn

- How to fetch a remote agent's **Agent Card** (capability manifest)
- How to create an `A2AClient` and wrap it as an `A2AAgent`
- How the A2A agent implements the same interface as a local `AIAgent`

## Architecture

```
┌─────────────┐      A2A/HTTP     ┌──────────────────────────────┐
│ A2AProtocol │◄─────────────────►│  Any A2A-compatible server   │
│  (client)   │  Agent Card +     │  (e.g. 09-AgentWithAspire)   │
└─────────────┘  streaming msgs   └──────────────────────────────┘
```

## Prerequisites

- .NET 9 SDK
- A running A2A server (start `09-AgentWithAspire` or any A2A endpoint)
- Set `A2A_AGENT_URL` in `.env`

## Setup

```bash
# In one terminal: start a server (e.g. 09-AgentWithAspire.Worker)
cd ../09-AgentWithAspire/09-AgentWithAspire.Worker && dotnet run

# In another terminal: run this sample
cp .env.example .env
# Set A2A_AGENT_URL=http://localhost:5000 in .env
dotnet run
```

## Key code

```csharp
// Discover the remote agent's capabilities
var cardResolver = new A2ACardResolver(agentUrl, httpClient);
var agentCard    = await cardResolver.GetAgentCardAsync();

// Connect and chat — identical to using a local AIAgent
var a2aAgent = new A2AAgent(new A2AClient(agentUrl, httpClient), agentCard);
await foreach (var chunk in a2aAgent.RunStreamingAsync("Hello!"))
    Console.Write(chunk);
```

## Next steps

- [06-HITL](../06-HITL/README.md) — add human approval gates to agent actions
