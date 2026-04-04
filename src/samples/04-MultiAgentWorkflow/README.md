# 04-MultiAgentWorkflow

Orchestrate multiple specialist agents into a **sequential pipeline** using `WorkflowBuilder`.
Each agent handles one stage; its output becomes the next agent's input.

## What you'll learn

- How to define a directed workflow graph with `WorkflowBuilder`
- How to run a workflow with `InProcessExecution.RunStreamingAsync`
- How to consume `AgentResponseUpdateEvent` to display per-agent output

## The pipeline

```
User Input
    │
    ▼
┌───────────┐    design doc    ┌───────────┐    implementation    ┌────────┐
│ Architect │─────────────────►│ Developer │────────────────────►│ Tester │
└───────────┘                  └───────────┘                      └────────┘
```

## Prerequisites

- .NET 9 SDK
- An OpenAI API key (gpt-4o recommended; multi-agent workflows use more tokens)

## Setup

```bash
cp .env.example .env
# Set OPENAI_API_KEY in .env
```

## Run

```bash
dotnet run
# Enter a feature request, e.g.: "Create a thread-safe in-memory cache with TTL"
```

## Key code

```csharp
// Build the workflow graph
var workflow = new WorkflowBuilder(architectAgent)
    .AddEdge(architectAgent, developerAgent)
    .AddEdge(developerAgent, testerAgent)
    .Build();

// Run it
StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, userInput);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is AgentResponseUpdateEvent e)
        Console.Write(e.Data);
}
```

> **Note:** See `MIGRATION.md` in the repo root for the `WorkflowBuilder` API mapping
> from Semantic Kernel `AgentGroupChat`.

## Next steps

- [05-A2AProtocol](../05-A2AProtocol/README.md) — call a remote agent over A2A
