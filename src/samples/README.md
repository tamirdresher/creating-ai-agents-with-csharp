# Console Samples — Microsoft Agent Framework GA (v1.0.0)

Standalone console applications demonstrating Microsoft Agent Framework concepts.
Each sample is a self-contained .NET 9 project you can run independently.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- An OpenAI API key (or [GitHub Models](https://github.com/marketplace/models) token)
- Node.js (for MCP sample only)

## Quick Start

```bash
cd src/samples/01-BasicAgent
cp .env.example .env       # fill in your API key
dotnet run
```

## Samples

| # | Sample | Topic | Notebook Equivalent |
|---|--------|-------|-------------------|
| 01 | [BasicAgent](01-BasicAgent/) | Create an AIAgent, stream responses | Notebook 1 |
| 02 | [AgentWithTools](02-AgentWithTools/) | Function tools with `AIFunctionFactory` | Notebook 3 |
| 03 | [MCPAgent](03-MCPAgent/) | Model Context Protocol integration | Notebook 3.2 |
| 04 | [MultiAgentWorkflow](04-MultiAgentWorkflow/) | Sequential agent pipeline | Notebook 4 |
| 05 | [A2AProtocol](05-A2AProtocol/) | Agent-to-Agent communication | Notebook 6 |
| 06 | [HITL](06-HITL/) | Human-in-the-Loop approval gates | Notebook 7 |
| 07 | [WorkflowFramework](07-WorkflowFramework/) | Custom executors and workflow graphs | Notebook 7 |
| 08 | [Guardrails](08-Guardrails/) | Input/output safety validation | Notebook 8 |

## NuGet Packages Used

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Agents.AI` | 1.0.0 | Core agent framework |
| `Microsoft.Agents.AI.OpenAI` | 1.0.0 | OpenAI integration |
| `Microsoft.Agents.AI.Workflows` | 1.0.0 | Workflow engine (samples 04, 06, 07) |
| `Microsoft.Extensions.AI` | 10.4.0 | `IChatClient` abstraction |
| `ModelContextProtocol` | 0.4.0-preview | MCP client (sample 03) |
| `OpenAI` | 2.9.1 | OpenAI SDK |

## Using GitHub Models

Set these in your `.env` file to use [GitHub Models](https://github.com/marketplace/models):

```
OPENAI_API_KEY=your-github-token
OPENAI_ENDPOINT=https://models.inference.ai.azure.com
OPENAI_MODEL_ID=gpt-4o-mini
```
