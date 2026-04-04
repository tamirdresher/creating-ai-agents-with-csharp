# Migration Guide: Semantic Kernel → Microsoft Agent Framework GA

This document records the decisions and API mappings made when migrating this workshop
from Semantic Kernel (v1.68.0) to **Microsoft Agent Framework GA (v1.0.0, April 2026)**.

---

## Why migrate?

| | Semantic Kernel | Microsoft Agent Framework |
|---|---|---|
| Status | Maintenance mode | Active development (GA) |
| AutoGen | Separate library | Unified |
| API stability | Multiple breaking preview APIs | Stable v1.0 |
| MEAI alignment | Partial | First-class |
| NuGet | `Microsoft.SemanticKernel` | `Microsoft.Agents.AI` |

The Agent Framework **unifies Semantic Kernel + AutoGen** into a single library with
a stable API built on top of the `Microsoft.Extensions.AI` (MEAI) abstractions.
Both SK and AutoGen are in maintenance mode as of April 2026.

---

## Package mapping

| Old (Semantic Kernel) | New (Agent Framework) |
|---|---|
| `Microsoft.SemanticKernel` | `Microsoft.Agents.AI` |
| `Microsoft.SemanticKernel.Connectors.OpenAI` | `Microsoft.Agents.AI.OpenAI` |
| `Microsoft.SemanticKernel.Connectors.AzureOpenAI` | `Azure.AI.OpenAI` + `Microsoft.Agents.AI.OpenAI` |
| `Microsoft.SemanticKernel.Agents.Core` | `Microsoft.Agents.AI` (agents are built in) |
| `Microsoft.SemanticKernel.Agents.Orchestration` | `Microsoft.Agents.AI.Workflows` |
| `Microsoft.SemanticKernel.Agents.Magentic` | `Microsoft.Agents.AI.Workflows` (MagenticGroupStrategy) |
| `Microsoft.SemanticKernel.Agents.A2A` | `A2A` + `A2A.AspNetCore` (unchanged) |
| `Microsoft.SemanticKernel.Agents.Runtime.InProcess` | `Microsoft.Agents.AI.Workflows` (`InProcessExecution`) |

---

## API mapping

### Creating a chat client

```csharp
// ❌ Old (Semantic Kernel)
IKernelBuilder builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion("gpt-4o", apiKey);
Kernel kernel = builder.Build();

// ✅ New (Agent Framework / MEAI)
IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey))
    .GetChatClient("gpt-4o")
    .AsIChatClient();
```

### Creating an agent

```csharp
// ❌ Old
var agent = new ChatCompletionAgent
{
    Name = "Coder",
    Instructions = "You are a C# expert.",
    Kernel = kernel,
};

// ✅ New
AIAgent agent = chatClient.AsAIAgent(
    name: "Coder",
    instructions: "You are a C# expert.");
```

### Running / streaming

```csharp
// ❌ Old (SK AgentThread)
AgentThread thread = new ChatHistoryAgentThread();
await foreach (AgentResponseItem<StreamingChatMessageContent> msg
    in agent.InvokeStreamingAsync("Fix my bug", thread))
    Console.Write(msg.Message.Content);

// ✅ New
await foreach (string chunk in agent.RunStreamingAsync("Fix my bug"))
    Console.Write(chunk);
```

### Tools (KernelPlugin → AIFunction)

```csharp
// ❌ Old
[KernelFunction, Description("Returns the time.")]
public string GetTime() => DateTime.UtcNow.ToString();

var plugin = KernelPluginFactory.CreateFromObject(new TimePlugin());
kernel.Plugins.Add(plugin);

// ✅ New
[Description("Returns the current UTC time.")]
static string GetTime() => DateTime.UtcNow.ToString();

var tool = AIFunctionFactory.Create(GetTime);
AIAgent agent = chatClient.AsAIAgent(..., tools: [tool]);
```

### Multi-agent orchestration

```csharp
// ❌ Old (AgentGroupChat)
var groupChat = new AgentGroupChat(architectAgent, developerAgent, testerAgent)
{
    ExecutionSettings = new AgentGroupChatSettings
    {
        SelectionStrategy = new SequentialSelectionStrategy(),
        TerminationStrategy = new MaxIterationTerminationStrategy(maxIterations: 5)
    }
};
await foreach (var msg in groupChat.InvokeAsync()) { ... }

// ✅ New (WorkflowBuilder)
var workflow = AgentWorkflowBuilder.BuildSequential([architectAgent, developerAgent, testerAgent]);

StreamingRun run = await InProcessExecution.Default.RunStreamingAsync(
    workflow, userMessage, null, cancellationToken);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is AgentResponseUpdateEvent e)
        Console.WriteLine($"{e.Update.AgentId}: {e.Update.Text}");
}
```

### OpenTelemetry source names

```csharp
// ❌ Old
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnostics", true);
builder.AddOpenTelemetry().WithTracing(b => b.AddSource("Microsoft.SemanticKernel*"));

// ✅ New — no AppContext switches needed
builder.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("Microsoft.Agents.*").AddSource("Microsoft.Extensions.AI"))
    .WithMetrics(b => b.AddMeter("Microsoft.Agents.*").AddMeter("Microsoft.Extensions.AI"));
```

---

## Phase 2 complete

The `SCHOOL_SOLUTIONS/` directory contains reference implementations that have been
**fully migrated** to Agent Framework GA. All 12 files compile cleanly with 0 errors.
Console samples 03–09 are also available in `src/samples/`.

See [SCHOOL_SOLUTIONS/MIGRATION_PENDING.md](src/SKCodeAssistent/SKCodeAssistent.Server/SCHOOL_SOLUTIONS/MIGRATION_PENDING.md)
for the complete migration map.

---

## Resources

- [Microsoft Agent Framework docs](https://learn.microsoft.com/agent-framework/)
- [MEAI (Microsoft.Extensions.AI) docs](https://learn.microsoft.com/dotnet/ai/meai/)
- [Agent Framework samples repo](https://github.com/microsoft/Agent-Framework-Samples)
- [NuGet: Microsoft.Agents.AI](https://www.nuget.org/packages/Microsoft.Agents.AI)
