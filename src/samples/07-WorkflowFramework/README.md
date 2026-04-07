# 07 — Workflow Framework

Build directed-graph workflows using the MAF Workflow engine — the successor to SK's Process Framework.

## What You'll Learn

- Creating custom `Executor<TIn, TOut>` processing units
- Building workflow graphs with `WorkflowBuilder`
- Mixing custom executors with AI agents in a single pipeline
- Observing execution with `WorkflowEvent` streaming

## Architecture

```
User Code → [Parser] → [Reviewer Agent] → [Improver Agent] → [Formatter] → Output
              (func)       (AI)               (AI)              (func)
```

## Key APIs

| API | Purpose |
|-----|---------|
| `Executor<TIn, TOut>` | Custom processing unit |
| `Func.BindAsExecutor()` | Turn a function into an executor |
| `WorkflowBuilder` | Build the directed graph |
| `InProcessExecution.RunStreamingAsync()` | Execute the workflow |
| `ExecutorCompletedEvent` | Executor finished processing |
| `AgentResponseUpdateEvent` | Agent streaming output |
| `WorkflowOutputEvent` | Final workflow output |

## Run

```bash
cp .env.example .env
dotnet run
```

Paste code when prompted, or press Enter for a default sample.

## Corresponding Notebook

📓 [7-Process-Framework-and-HITL.ipynb](../../../notebooks/7-Process-Framework-and-HITL.ipynb)
