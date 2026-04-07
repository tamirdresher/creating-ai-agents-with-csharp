# 04 — Multi-Agent Workflow

Orchestrate multiple agents in a sequential pipeline using the Workflow engine.

## What You'll Learn

- Defining specialist agents with focused instructions
- Building a sequential workflow with `AgentWorkflowBuilder.BuildSequential()`
- Watching workflow events with `StreamingRun.WatchStreamAsync()`

## Architecture

```
User Input → [Architect] → [Developer] → [Tester] → Final Output
```

Each agent sees the previous agent's output and adds its own contribution.

## Key APIs

| API | Purpose |
|-----|---------|
| `ChatClientAgent` | Agent class wrapping an `IChatClient` |
| `AgentWorkflowBuilder.BuildSequential()` | Creates a linear agent pipeline |
| `InProcessExecution.RunStreamingAsync()` | Runs the workflow locally |
| `TurnToken` | Triggers agent processing |
| `AgentResponseUpdateEvent` | Streaming event from agents |

## Run

```bash
cp .env.example .env
dotnet run
```

## Other Patterns

The `AgentWorkflowBuilder` also supports:
- `BuildConcurrent()` — run agents in parallel
- `CreateHandoffBuilderWith()` — route to specialist agents
- `CreateGroupChatBuilderWith()` — round-robin or managed group chat

## Corresponding Notebook

📓 [4-MultiAgent-Orchestration.ipynb](../../../notebooks/4-MultiAgent-Orchestration.ipynb)
