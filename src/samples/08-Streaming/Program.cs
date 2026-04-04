/*
 * 08-Streaming — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates advanced streaming patterns:
 *   1. Token-by-token streaming with timing and throughput metrics
 *   2. Running multiple prompts in parallel with concurrent streams
 *   3. Cancellation — stopping a stream mid-flight
 *
 * RunStreamingAsync returns IAsyncEnumerable<string> — each item is a
 * text chunk as it arrives from the model (typically a word or sub-word).
 */

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.ComponentModel;
using System.Diagnostics;

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

// ── Create agent with a time tool (forces a tool call + streaming round-trip) ──
var tools = new List<AITool>
{
    AIFunctionFactory.Create(GetCurrentTime),
    AIFunctionFactory.Create(GetRandomFact),
};

AIAgent agent = chatClient.AsAIAgent(
    name: "StreamingDemo",
    instructions: """
        You are a helpful assistant. Answer questions with moderately detailed responses
        (3-5 paragraphs) so streaming is clearly visible. Use your tools when relevant.
        """,
    tools: tools);

// ═════════════════════════════════════════════════════════════════════════════
// Demo 1: Basic streaming with metrics
// ═════════════════════════════════════════════════════════════════════════════
Console.WriteLine("=== 08-Streaming: Demo 1 — Token-by-token with metrics ===\n");

var prompt1 = "Explain what makes C# 13 generics different from earlier versions, "
            + "and give a short code example. Then tell me the current time.";
Console.WriteLine($"Prompt: {prompt1}\n");
Console.WriteLine("Response:");
Console.WriteLine(new string('─', 60));

var sw         = Stopwatch.StartNew();
var firstChunk = TimeSpan.Zero;
int chunkCount = 0;

await foreach (var chunk in agent.RunStreamingAsync(prompt1))
{
    if (chunkCount == 0)
        firstChunk = sw.Elapsed;

    Console.Write(chunk);
    chunkCount++;
}

sw.Stop();
Console.WriteLine();
Console.WriteLine(new string('─', 60));
Console.WriteLine($"Metrics: {chunkCount} chunks | TTFT: {firstChunk.TotalMilliseconds:F0}ms | Total: {sw.ElapsedMilliseconds}ms");

// ═════════════════════════════════════════════════════════════════════════════
// Demo 2: Parallel streaming from two independent prompts
// ═════════════════════════════════════════════════════════════════════════════
Console.WriteLine("\n\n=== Demo 2 — Parallel streaming (two prompts simultaneously) ===\n");

var task1 = StreamAndCollectAsync(agent, "What is LINQ? Give 3 examples.", label: "LINQ");
var task2 = StreamAndCollectAsync(agent, "What is async/await in C#? Give 2 examples.", label: "Async");

// Both prompts stream concurrently; we await both to finish
await Task.WhenAll(task1, task2);

Console.WriteLine("\nBoth streams complete.");

// ═════════════════════════════════════════════════════════════════════════════
// Demo 3: Cancellation — stop streaming after 2 seconds
// ═════════════════════════════════════════════════════════════════════════════
Console.WriteLine("\n\n=== Demo 3 — Cancellation after 2 seconds ===\n");
Console.WriteLine("Starting a long response that will be cancelled after 2s...\n");

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
int cancelled_chunks = 0;
try
{
    await foreach (var chunk in agent.RunStreamingAsync(
        "Write a very long essay about the history of programming languages, "
        + "covering at least 20 different languages in detail.",
        cancellationToken: cts.Token))
    {
        Console.Write(chunk);
        cancelled_chunks++;
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine($"\n\n[Stream cancelled after {cancelled_chunks} chunks — CancellationToken triggered]");
}

Console.WriteLine("\n\nDone!");

// ── Tool implementations ───────────────────────────────────────────────────────

[Description("Returns the current UTC date and time as a formatted string.")]
static string GetCurrentTime() =>
    DateTimeOffset.UtcNow.ToString("dddd, MMMM d yyyy 'at' HH:mm:ss 'UTC'");

[Description("Returns a random interesting fact about C# or .NET.")]
static string GetRandomFact()
{
    string[] facts =
    [
        "C# was designed by Anders Hejlsberg, who also created Turbo Pascal and Delphi.",
        ".NET 9 brings ahead-of-time (AOT) compilation to ASP.NET Core Minimal APIs.",
        "The 'await' keyword in C# compiles into a state machine via Roslyn.",
        "Pattern matching in C# 13 supports list patterns like [first, .., last].",
        "Source generators run at compile time and can generate hundreds of thousands of lines.",
    ];
    return facts[Random.Shared.Next(facts.Length)];
}

// ── Helper: stream a prompt with a label prefix (for parallel demo) ───────────
static async Task StreamAndCollectAsync(AIAgent agent, string prompt, string label)
{
    var sw = Stopwatch.StartNew();
    Console.WriteLine($"[{label}] Starting...");
    var chunks = 0;
    await foreach (var chunk in agent.RunStreamingAsync(prompt))
        chunks++;
    Console.WriteLine($"[{label}] Done — {chunks} chunks in {sw.ElapsedMilliseconds}ms");
}
