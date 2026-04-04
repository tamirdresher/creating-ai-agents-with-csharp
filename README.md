# Creating AI Agents with C# — Microsoft Agent Framework Workshop

> **Updated for Microsoft Agent Framework GA (v1.0.0 — April 2026)**
>
> This workshop was previously built on Semantic Kernel. It has been migrated to the
> **Microsoft Agent Framework**, which unifies Semantic Kernel and AutoGen into a single,
> stable API. See [MIGRATION.md](MIGRATION.md) for the full context and API mapping.

A hands-on workshop for building AI agents in C# using the Microsoft Agent Framework,
Microsoft.Extensions.AI, and .NET Aspire.

---

## 📚 Workshop Overview

You will build an AI Coding Assistant step-by-step, starting with a single agent
and progressing to a collaborative multi-agent system (Architect + Developer + Tester)
with tool use, MCP integration, and Agent-to-Agent (A2A) communication.

---

## 🎯 Learning Path

### Console Samples (start here)

Self-contained console projects — no server required.

| Sample | What you learn |
|---|---|
| [`src/samples/01-BasicAgent`](src/samples/01-BasicAgent/) | Create an `AIAgent`, stream responses |
| [`src/samples/02-AgentWithTools`](src/samples/02-AgentWithTools/) | Add `AIFunction` tools to an agent |
| `src/samples/03-AgentWithMCP` *(coming soon)* | Connect to an MCP server |
| `src/samples/04-MultiAgentWorkflow` *(coming soon)* | Wire agents with `WorkflowBuilder` |
| `src/samples/05-A2ARemoteAgent` *(coming soon)* | Agent-to-Agent protocol |

### Server Project (SKCodeAssistent)

A production-style ASP.NET Core + Aspire server that hosts the coding assistant.
Participants fill in the TODOs in `Services/CodingAssistentSession.cs`, then unlock
progressively richer implementations from `SCHOOL_SOLUTIONS/`.

```
src/SKCodeAssistent/
├── SKCodeAssistent.AppHost/     ← .NET Aspire orchestrator
├── SKCodeAssistent.Server/      ← ASP.NET Core API + agent logic
│   ├── Services/                ← ICodingAssistentSession + template
│   └── SCHOOL_SOLUTIONS/        ← Complete reference implementations
└── SKCodeAssistent.ServiceDefaults/
```

### Assignments

| # | Topic |
|---|---|
| 1 | [Three Agents](docs/assignments/Assignment-1-Three-Agents.md) |
| 2 | [Tools and MCP](docs/assignments/Assignment-2-Plugins-and-MCP.md) |
| 3 | [Team Orchestration](docs/assignments/Assignment-3-Team-Orchestration.md) |
| 4 | [A2A Protocol](docs/assignments/Assignment-4-A2A-Protocol.md) |
| 5 | [Process Framework & HITL](docs/assignments/Assignment-5-Process-Framework.md) |

---

## 🚀 Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- An OpenAI API key **or** a [GitHub Models](docs/Using_GitHub_Models.md) token
- Node.js 18+ (for the VS Code extension)

### Option A — Console sample (fastest)

```bash
cd src/samples/01-BasicAgent
cp .env.example .env
# edit .env → set OPENAI_API_KEY
dotnet run
```

### Option B — Full server via Aspire

```bash
cd src/SKCodeAssistent
dotnet run --project SKCodeAssistent.AppHost
# Opens Aspire dashboard at https://localhost:15888
```

Configure credentials in `SKCodeAssistent.AppHost/appsettings.json`
or via environment variables — see [docs/README.md](docs/README.md#configuration).

---

## 🔑 Agent Framework Quick Reference

```csharp
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

// 1. Chat client (works with OpenAI, Azure OpenAI, GitHub Models, Ollama…)
IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey))
    .GetChatClient("gpt-4o")
    .AsIChatClient();

// 2. Create an agent with tools
AIAgent agent = chatClient.AsAIAgent(
    name: "Coder",
    instructions: "You are an expert C# developer.",
    tools: [AIFunctionFactory.Create(MyTool)]);

// 3. Stream a response
await foreach (string chunk in agent.RunStreamingAsync("Explain LINQ"))
    Console.Write(chunk);

// 4. Multi-agent workflow
var workflow = new WorkflowBuilder(architectAgent)
    .AddEdge(architectAgent, developerAgent)
    .AddEdge(developerAgent, testerAgent)
    .Build();
```

---

## 📖 Resources

- [Microsoft Agent Framework docs](https://learn.microsoft.com/agent-framework/)
- [NuGet: Microsoft.Agents.AI](https://www.nuget.org/packages/Microsoft.Agents.AI)
- [GitHub Models setup](docs/Using_GitHub_Models.md)
- [Migration guide (SK → Agent Framework)](MIGRATION.md)
- [Polyglot Notebooks archive](notebooks/DEPRECATED.md)
