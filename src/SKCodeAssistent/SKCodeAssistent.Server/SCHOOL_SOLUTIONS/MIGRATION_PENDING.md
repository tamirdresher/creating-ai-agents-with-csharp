# SCHOOL_SOLUTIONS — Migration Pending

All files in this directory are **excluded from compilation** while the migration from
Semantic Kernel → Microsoft Agent Framework GA progresses.

They are preserved as-is so workshop participants can use them as reference material
when rewriting the implementations.

---

## Migration Map

| Old SK class | New Agent Framework equivalent | Status |
|---|---|---|
| `ChatCompletionAgent` | `AIAgent` via `IChatClient.AsAIAgent()` | ⬜ pending |
| `Kernel` / `KernelBuilder` | `IChatClient` (MEAI abstraction) | ⬜ pending |
| `KernelPlugin` / `KernelFunction` | `AIFunction` via `AIFunctionFactory.Create()` | ⬜ pending |
| `AgentGroupChat` | `WorkflowBuilder` + `InProcessExecution` | ⬜ pending |
| `MagenticOrchestration` | `WorkflowBuilder` with MagenticGroupStrategy | ⬜ pending |
| `IMcpClient` (SK) | `ModelContextProtocol.Client.IMcpClient` (unchanged) | ✅ same |
| `AgentChat` streaming | `RunStreamingAsync` on `AIAgent` | ⬜ pending |
| `ChatHistory` | Managed internally by `AIAgent` thread history | ⬜ pending |

---

## Agent Framework Quick Reference

```csharp
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

// 1. Get an IChatClient
IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey))
    .GetChatClient("gpt-4o")
    .AsIChatClient();

// 2. Create an agent
AIAgent agent = chatClient.AsAIAgent(
    name: "SoftwareArchitect",
    instructions: "You are a senior software architect...",
    tools: [AIFunctionFactory.Create(MyTool)]);

// 3. Stream a response
await foreach (string chunk in agent.RunStreamingAsync("Design a microservice"))
    Console.Write(chunk);

// 4. Multi-agent workflow
var workflow = new WorkflowBuilder(agentA)
    .AddEdge(agentA, agentB)
    .Build();

StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, userMessage);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is AgentResponseUpdateEvent e)
        Console.WriteLine($"{e.ExecutorId}: {e.Data}");
}
```

---

## Files to migrate (in order)

1. `AgentDefinitions.cs` — define architect / developer / tester agents
2. `CodingAssistentSessions/CodingAssistentSession_SingleAgent.cs`
3. `CodingAssistentSessions/CodingAssistentSession_SingleAgentWithPlugins.cs` → `_SingleAgentWithTools`
4. `CodingAssistentSessions/CodingAssistentSession_SingleAgentWithPluginsAndMCP.cs` → `_SingleAgentWithMCP`
5. `Orchestration/SoftwareTeamLeader.cs`
6. `CodingAssistentSessions/CodingAssistentSession_CustomOrchestration.cs` → `_MultiAgentWorkflow`
7. `CodingAssistentSessions/CodingAssistentSession_CustomOrchestrationWithA2A.cs` → `_MultiAgentWithA2A`
8. `CodingAssistentSessions/CodingAssistentSession_MagenticOrchestration.cs`
9. `Orchestration/ChatHistorySummarizationReducer.cs`
10. `Orchestration/OrchestrationMonitor.cs`
11. `Plugins/CommandExecutionPlugin.cs` — port to `AIFunctionFactory`
12. `Plugins/FileOperationsPlugin.cs` — port to `AIFunctionFactory`

Once a file is migrated, remove the `<Compile Remove="SCHOOL_SOLUTIONS\**\*.cs" />` exclusion in the `.csproj`
and add individual `<Compile Remove="..."/>` entries for files not yet done.
