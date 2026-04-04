# Notebooks — Archived

> **These notebooks have been archived.** They were written for Semantic Kernel and
> the Polyglot Notebooks VS Code extension, both of which are no longer maintained.

## Why

| Component | Status |
|---|---|
| Polyglot Notebooks VS Code extension | Deprecated March 27, 2026 |
| .NET Interactive (the kernel) | Archived April 24, 2026 |
| Semantic Kernel | Maintenance mode (superseded by Agent Framework) |

## What Replaces Them

Workshop content has been migrated to **regular C# console projects** and an
**ASP.NET Core server** that you can run and debug like any other .NET application:

| Old notebook | New location |
|---|---|
| `0-AI-settings.ipynb` | `src/SKCodeAssistent/SKCodeAssistent.AppHost/` (Aspire config) |
| `1-SemanticKernel-Intro.ipynb` | `src/samples/01-BasicAgent/` |
| `2-SemanticKernel-Agents.ipynb` | `src/samples/02-SingleAgent/` |
| `3-Functions-Plugins.ipynb` | `src/samples/03-AgentWithTools/` |
| `3.1-OpenAPIPlugin.ipynb` | `src/samples/04-AgentWithOpenAPI/` |
| `3.2-MCP.ipynb` | `src/samples/05-AgentWithMCP/` |
| `4-MultiAgent-Orchestration.ipynb` | `src/samples/06-MultiAgentWorkflow/` |
| `5-ChatHistoryReducers.ipynb` | (history managed by Agent Framework internally) |
| `6-Agent-to-Agent-Protocol*.ipynb` | `src/samples/07-A2ARemoteAgent/` |
| `7-Process-Framework-and-HITL.ipynb` | `src/samples/08-HumanInTheLoop/` |
| `8-Guardrails-and-AI-Safety.ipynb` | `src/samples/09-Guardrails/` |

> **Note:** The `src/samples/` projects are being added in follow-up PRs.
> See [MIGRATION.md](../MIGRATION.md) for the full roadmap.

## Running the Workshop Without Notebooks

```bash
# Start the Aspire dashboard + server
cd src/SKCodeAssistent
dotnet run --project SKCodeAssistent.AppHost

# Or run individual samples directly
cd src/samples/01-BasicAgent
dotnet run
```
