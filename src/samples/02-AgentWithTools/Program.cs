/*
 * 02-AgentWithTools — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates giving an agent access to function tools using
 * AIFunctionFactory.Create(). The agent can call these tools autonomously.
 *
 * Tools shown:
 *   - GetCurrentTime  — returns the current UTC time
 *   - ReadFile        — reads text from a file
 *   - ListDirectory   — lists files in a directory
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
using System.ComponentModel;

// ── Config ───────────────────────────────────────────────────────────────────
Env.TraversePath().Load();

var apiKey   = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
               ?? throw new InvalidOperationException("Set OPENAI_API_KEY");
var modelId  = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") ?? "gpt-4o-mini";
var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");

OpenAIClientOptions? options = endpoint is not null
    ? new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
    : null;

var chatClient = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient(modelId);

// ── Define tools with AIFunctionFactory ──────────────────────────────────────
// AIFunctionFactory.Create() turns any .NET method into an AIFunction (AITool).
// [Description] attributes populate the tool descriptions sent to the model.

var tools = new List<AITool>
{
    AIFunctionFactory.Create(GetCurrentTime),
    AIFunctionFactory.Create(ReadFile),
    AIFunctionFactory.Create(ListDirectory),
};

// ── Create the agent with tools ──────────────────────────────────────────────
AIAgent agent = chatClient.AsAIAgent(
    instructions: """
        You are a helpful assistant that can read files and list directories.
        When the user asks about files or directories, use your tools.
        Always confirm what you found before answering.
        """,
    name: "FileAssistant",
    tools: tools);

// ── Chat loop ────────────────────────────────────────────────────────────────
Console.WriteLine("=== 02-AgentWithTools (type 'exit' to quit) ===");
Console.WriteLine("Try: 'What time is it?' or 'List the files in the current directory'\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    Console.Write("Agent: ");
    await foreach (var chunk in agent.RunStreamingAsync(input))
        Console.Write(chunk);
    Console.WriteLine("\n");
}

Console.WriteLine("Goodbye!");

// ── Tool implementations ─────────────────────────────────────────────────────

[Description("Returns the current UTC date and time.")]
static string GetCurrentTime() =>
    DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss zzz");

[Description("Reads the text content of a file at the given path.")]
static string ReadFile(
    [Description("Absolute or relative path to the file.")] string filePath)
{
    if (!File.Exists(filePath))
        return $"Error: file not found — {filePath}";
    try { return File.ReadAllText(filePath); }
    catch (Exception ex) { return $"Error reading file: {ex.Message}"; }
}

[Description("Lists the files and subdirectories in the given directory.")]
static string ListDirectory(
    [Description("Absolute or relative path to the directory.")] string directoryPath)
{
    if (!Directory.Exists(directoryPath))
        return $"Error: directory not found — {directoryPath}";

    var entries = Directory.GetFileSystemEntries(directoryPath)
        .Select(Path.GetFileName)
        .OrderBy(n => n);

    return string.Join(Environment.NewLine, entries!);
}
