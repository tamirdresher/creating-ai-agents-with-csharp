/*
 * 04-MultiAgentWorkflow — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates a sequential multi-agent workflow using WorkflowBuilder.
 * Three specialists work together in a pipeline:
 *
 *   Architect  →  Developer  →  Tester
 *
 * The Architect designs the solution, the Developer implements it,
 * and the Tester writes tests — each agent sees the previous agent's output.
 *
 * Key APIs:
 *   WorkflowBuilder   — defines the graph of agents and edges
 *   InProcessExecution.RunStreamingAsync — runs the workflow in-process
 *   WorkflowEvent / AgentResponseUpdateEvent — stream events as they arrive
 */

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

// ── Config ─────────────────────────────────────────────────────────────────────
Env.TraversePath().Load();

var apiKey   = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
               ?? throw new InvalidOperationException("Set OPENAI_API_KEY");
var modelId  = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") ?? "gpt-4o";
var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");

OpenAIClientOptions? options = endpoint is not null
    ? new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
    : null;

IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient(modelId)
    .AsIChatClient();

// ── Define the three specialist agents ────────────────────────────────────────
//
// Each agent gets a focused system prompt for its role.
// In a workflow the agents execute sequentially; each sees prior output.

AIAgent architectAgent = chatClient.AsAIAgent(
    name: "Architect",
    instructions: """
        You are a software architect. When given a feature request:
        1. Identify the key components and their responsibilities.
        2. Define the public API (method signatures, data models).
        3. Describe data flow in 2-3 sentences.
        Be concise — the Developer will implement what you design.
        """);

AIAgent developerAgent = chatClient.AsAIAgent(
    name: "Developer",
    instructions: """
        You are a C# developer. You receive an architecture design and implement it.
        - Write clean, idiomatic C# code targeting .NET 9.
        - Include XML doc comments on public members.
        - Do not write tests — that is the Tester's job.
        """);

AIAgent testerAgent = chatClient.AsAIAgent(
    name: "Tester",
    instructions: """
        You are a QA engineer. You receive implemented C# code and write xUnit tests for it.
        - Cover happy paths and edge cases.
        - Use Arrange-Act-Assert structure.
        - Mock external dependencies with NSubstitute.
        """);

// ── Build the workflow graph ───────────────────────────────────────────────────
//
// AgentWorkflowBuilder.BuildSequential creates a linear pipeline where each
// agent's output becomes the next agent's input.
var workflow = AgentWorkflowBuilder.BuildSequential(
    [architectAgent, developerAgent, testerAgent]);

// ── Run the workflow ───────────────────────────────────────────────────────────
Console.WriteLine("=== 04-MultiAgentWorkflow ===");
Console.Write("Feature request: ");
var userInput = Console.ReadLine()
    ?? "Create a simple in-memory cache with TTL support";

Console.WriteLine();

// InProcessExecution.Default runs everything in the current process (no network hops).
// RunStreamingAsync<T> takes the workflow, the initial user message, optional sessionId,
// and a CancellationToken. Returns a StreamingRun you can watch for events.
StreamingRun run = await InProcessExecution.Default.RunStreamingAsync(
    workflow, userInput, null, default);

// TrySendMessageAsync kicks off the first turn with event emission enabled.
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

// WatchStreamAsync yields WorkflowEvent instances as each agent produces output.
string? lastAgentId = null;
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    if (evt is AgentResponseUpdateEvent updateEvt)
    {
        // AgentResponseUpdate.AgentId identifies the responding agent
        var agentId = updateEvt.Update.AgentId ?? updateEvt.Update.AuthorName ?? "Agent";

        // Print a header when a new agent starts speaking
        if (agentId != lastAgentId)
        {
            Console.WriteLine($"\n─── [{agentId}] ───────────────────────────────");
            lastAgentId = agentId;
        }

        Console.Write(updateEvt.Update.Text);
    }
}

Console.WriteLine("\n\nWorkflow complete!");
