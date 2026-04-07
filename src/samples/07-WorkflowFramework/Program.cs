/*
 * 07-WorkflowFramework — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates the Workflow engine — the MAF replacement for SK's Process Framework.
 * Workflows are directed graphs of executors connected by edges.
 *
 * This sample builds a code-review pipeline:
 *   1. ParseExecutor     — extracts code from the user input
 *   2. Agent (Reviewer)  — reviews the code for issues
 *   3. Agent (Improver)  — rewrites the code addressing the review
 *   4. SummaryExecutor   — formats the final output
 *
 * Key concepts:
 *   - Executor<TIn, TOut>      — custom processing units
 *   - WorkflowBuilder          — imperative graph construction
 *   - InProcessExecution       — runs workflows locally
 *   - StreamingRun             — observe events in real time
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
using OpenAI.Chat;
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

// ── Custom executors ─────────────────────────────────────────────────────────

// ParseExecutor: wraps raw user text into a ChatMessage for agent consumption
Func<string, ChatMessage> parseFunc = input => new ChatMessage(ChatRole.User, $"Review this code:\n\n{input}");
var parseExecutor = parseFunc.BindAsExecutor("Parser");

// FormatExecutor: takes final agent output and formats it
Func<string, string> formatFunc = text => $"""
    ════════════════════════════════════════
    WORKFLOW RESULT
    ════════════════════════════════════════
    {text}
    ════════════════════════════════════════
    """;
var formatExecutor = formatFunc.BindAsExecutor("Formatter");

// ── AI Agent executors ───────────────────────────────────────────────────────

ChatClientAgent reviewerAgent = new(chatClient, """
    You are a code reviewer. Analyze the code and list:
    1. Bugs and logic errors
    2. Performance concerns
    3. Security issues
    4. Suggested improvements
    Be specific and concise.
    """, "Reviewer");

ChatClientAgent improverAgent = new(chatClient, """
    You are a senior developer. You receive code along with review feedback.
    Rewrite the code addressing ALL the review points.
    Output ONLY the improved code with brief inline comments explaining changes.
    """, "Improver");

// ── Build the workflow graph ─────────────────────────────────────────────────
//
// parseExecutor → reviewerAgent → improverAgent → formatExecutor
//
var builder = new WorkflowBuilder(parseExecutor);
builder.AddEdge(parseExecutor, reviewerAgent);
builder.AddEdge(reviewerAgent, improverAgent);
builder.AddEdge(improverAgent, formatExecutor);
builder.WithOutputFrom(formatExecutor);
var workflow = builder.Build();

// ── Run the workflow ─────────────────────────────────────────────────────────
Console.WriteLine("=== 07-WorkflowFramework: Code Review Pipeline ===\n");
Console.Write("Paste some code to review (or press Enter for a default):\n> ");
var userCode = Console.ReadLine();

if (string.IsNullOrWhiteSpace(userCode))
{
    userCode = """
        public class Calculator
        {
            public int Divide(int a, int b) => a / b;
            public string FormatResult(int result) => "Result: " + result.ToString();
        }
        """;
    Console.WriteLine("[Using default code]\n");
}

Console.WriteLine("Running workflow...\n");

await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, userCode);
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

string? lastExecId = null;
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    switch (evt)
    {
        case AgentResponseUpdateEvent agentEvt:
            if (agentEvt.ExecutorId != lastExecId)
            {
                lastExecId = agentEvt.ExecutorId;
                Console.WriteLine($"\n─── [{lastExecId}] ──────────────────────────");
            }
            Console.Write(agentEvt.Update.Text);
            break;

        case ExecutorCompletedEvent completedEvt:
            Console.WriteLine($"\n[{completedEvt.ExecutorId} completed]");
            if (completedEvt.Data is string result)
                Console.WriteLine(result);
            break;

        case WorkflowOutputEvent outputEvt:
            if (outputEvt.Data is string finalResult)
                Console.WriteLine(finalResult);
            break;
    }
}

Console.WriteLine("\nWorkflow complete!");
