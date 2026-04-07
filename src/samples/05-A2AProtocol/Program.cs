/*
 * 05-A2AProtocol — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates the Agent-to-Agent (A2A) protocol concept.
 * A2A is a standard for connecting to remote agents regardless of framework.
 *
 * Since A2A requires a running remote agent server, this sample demonstrates
 * the pattern by:
 *   1. Creating a local "remote" agent
 *   2. Showing how the A2A flow works conceptually
 *   3. Demonstrating agent discovery, message exchange, and response handling
 *
 * For a real A2A deployment, you would:
 *   - Host agents as HTTP services with /.well-known/agent.json
 *   - Use an A2A client library to communicate
 *
 * Prerequisites:
 *  - Copy .env.example → .env and set OPENAI_API_KEY
 *  - dotnet run
 */

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;

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

// ── Define "remote" agents (in a real scenario these run in separate services) ──

ChatClientAgent codeReviewAgent = new(chatClient, """
    You are a code review specialist. Review the provided code for:
    - Bugs and logic errors
    - Performance issues
    - Security vulnerabilities
    - Code style and best practices
    Provide specific, actionable feedback.
    """, "CodeReviewer", "Reviews code for quality issues");

ChatClientAgent documentationAgent = new(chatClient, """
    You are a documentation specialist. Generate documentation for the provided code:
    - XML doc comments for public APIs
    - A brief README section explaining usage
    - Example code showing how to use the API
    """, "DocumentationWriter", "Generates docs for code");

// ── Simulate Agent Card discovery ────────────────────────────────────────────
// In A2A, agents publish their capabilities via an Agent Card at
// /.well-known/agent.json. Here we simulate that discovery.

var agents = new Dictionary<string, (ChatClientAgent Agent, string Description)>
{
    ["review"] = (codeReviewAgent, "Reviews code for bugs, performance, and style issues"),
    ["docs"]   = (documentationAgent, "Generates XML docs, README sections, and examples"),
};

Console.WriteLine("=== 05-A2A Protocol Demo ===\n");
Console.WriteLine("Available remote agents:");
foreach (var (key, (_, desc)) in agents)
    Console.WriteLine($"  [{key}] — {desc}");

Console.WriteLine();

// ── Agent-to-Agent message exchange ──────────────────────────────────────────
// First, get a code snippet to send to the agents.
Console.Write("Paste some C# code (or press Enter for a default): ");
var codeInput = Console.ReadLine();

if (string.IsNullOrWhiteSpace(codeInput))
{
    codeInput = """
        public class UserService
        {
            private readonly string _connectionString;
            public UserService(string connStr) { _connectionString = connStr; }
            public string GetUser(int id)
            {
                var query = "SELECT * FROM Users WHERE Id = " + id;
                // execute query and return result
                return query;
            }
        }
        """;
    Console.WriteLine("[Using default code sample]\n");
}

// Send the code to both agents concurrently (simulating A2A message dispatch)
Console.WriteLine("Dispatching to remote agents via A2A...\n");

var reviewTask = CollectResponseAsync(codeReviewAgent, $"Review this code:\n```csharp\n{codeInput}\n```");
var docsTask   = CollectResponseAsync(documentationAgent, $"Generate documentation for:\n```csharp\n{codeInput}\n```");

await Task.WhenAll(reviewTask, docsTask);

Console.WriteLine("─── [CodeReviewer response] ─────────────────────────");
Console.WriteLine(await reviewTask);
Console.WriteLine("\n─── [DocumentationWriter response] ──────────────────");
Console.WriteLine(await docsTask);

Console.WriteLine("\n\nA2A demo complete!");

// ── Helper ───────────────────────────────────────────────────────────────────
static async Task<string> CollectResponseAsync(ChatClientAgent agent, string message)
{
    var sb = new StringBuilder();
    await foreach (var chunk in agent.RunStreamingAsync(message))
        sb.Append(chunk);
    return sb.ToString();
}
