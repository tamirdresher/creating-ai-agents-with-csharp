/*
 * 04-MultiAgentWorkflow — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates multi-agent orchestration using the Workflow engine.
 * Shows three patterns:
 *   1. Sequential — agents execute in a pipeline (Architect → Developer → Tester)
 *   2. Concurrent — agents execute in parallel and results are aggregated
 *   3. Handoff    — a triage agent routes to specialist agents
 *
 * Key APIs:
 *   AgentWorkflowBuilder.BuildSequential / BuildConcurrent / CreateHandoffBuilderWith
 *   InProcessExecution.RunStreamingAsync  — runs the workflow in-process
 *   StreamingRun.WatchStreamAsync         — yields events as agents produce output
 *
 * Prerequisites:
 *  - Copy .env.example → .env and set OPENAI_API_KEY
 *  - dotnet run
 */

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

// ── Config ───────────────────────────────────────────────────────────────────
Env.TraversePath().Load();

var apiKey   = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
               ?? throw new InvalidOperationException("Set OPENAI_API_KEY");
var modelId  = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") ?? "gpt-4o-mini";
var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");

OpenAIClientOptions? options = endpoint is not null
    ? new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
    : null;

IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient(modelId)
    .AsIChatClient();

// ── Define specialist agents ─────────────────────────────────────────────────

ChatClientAgent architectAgent = new(chatClient, """
    You are a software architect. Given a feature request:
    1. Identify key components and responsibilities.
    2. Define the public API (method signatures, data models).
    3. Describe data flow in 2-3 sentences.
    Be concise — the Developer will implement your design.
    """, "Architect");

ChatClientAgent developerAgent = new(chatClient, """
    You are a C# developer. You receive an architecture design and implement it.
    - Write clean, idiomatic C# targeting .NET 9.
    - Include XML doc comments on public members.
    - Do not write tests — that is the Tester's job.
    """, "Developer");

ChatClientAgent testerAgent = new(chatClient, """
    You are a QA engineer. You receive C# code and write xUnit tests.
    - Cover happy paths and edge cases.
    - Use Arrange-Act-Assert structure.
    """, "Tester");

// ── Build sequential workflow ────────────────────────────────────────────────
// AgentWorkflowBuilder.BuildSequential creates a pipeline: each agent's output
// becomes the next agent's input.

var workflow = AgentWorkflowBuilder.BuildSequential([architectAgent, developerAgent, testerAgent]);

// ── Run the workflow ─────────────────────────────────────────────────────────
Console.WriteLine("=== 04-MultiAgentWorkflow: Sequential Pipeline ===\n");
Console.Write("Feature request: ");
var userInput = Console.ReadLine()
    ?? "Create a simple in-memory cache with TTL support";

Console.WriteLine();

// Start the streaming workflow execution
var messages = new List<ChatMessage> { new(ChatRole.User, userInput) };
await using StreamingRun run = await InProcessExecution.RunStreamingAsync(
    workflow,
    messages);

// TurnToken triggers the agent executors to begin processing
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

// Watch events as each agent produces output
string? lastAgentId = null;
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is AgentResponseUpdateEvent updateEvt)
    {
        var agentId = updateEvt.ExecutorId ?? "Agent";

        if (agentId != lastAgentId)
        {
            Console.WriteLine($"\n─── [{agentId}] ───────────────────────────────");
            lastAgentId = agentId;
        }

        Console.Write(updateEvt.Update.Text);
    }
}

Console.WriteLine("\n\nWorkflow complete!");
