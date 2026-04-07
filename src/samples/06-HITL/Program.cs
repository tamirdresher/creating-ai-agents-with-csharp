/*
 * 06-HITL (Human-in-the-Loop) — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates two HITL patterns:
 *
 * Pattern 1 — Tool-level approval:
 *   The agent calls tools, but destructive tools (delete, write) ask the
 *   user for confirmation before executing. Pure C# — no framework changes.
 *
 * Pattern 2 — Workflow-level approval (commented out; see WorkflowHITL below):
 *   Uses RequestPort / ExternalRequest to pause a workflow and wait for
 *   human input before proceeding.
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

IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient(modelId)
    .AsIChatClient();

// ── Build tools with approval gates ──────────────────────────────────────────
var tools = new List<AITool>
{
    AIFunctionFactory.Create(ListFiles),
    AIFunctionFactory.Create(ReadFile),
    AIFunctionFactory.Create(DeleteFile),   // requires human approval
    AIFunctionFactory.Create(WriteFile),    // requires human approval
};

// ── Create agent ─────────────────────────────────────────────────────────────
AIAgent agent = new ChatClientAgent(chatClient, """
        You are a file management assistant. You can list, read, write, and delete files.
        IMPORTANT: Always list or read files first before modifying or deleting.
        Explain what you are about to do before calling a destructive tool.
    """,
    "FileManagerAgent",
    tools: tools);

// ── Chat loop ────────────────────────────────────────────────────────────────
Console.WriteLine("=== 06-HITL: Human-in-the-Loop File Manager ===");
Console.WriteLine("Destructive operations (write, delete) require your approval.");
Console.WriteLine("Try: 'List the files here' or 'Create a file called test.txt'\n");

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

// ── Tool implementations with HITL approval gates ────────────────────────────

[Description("Lists files and subdirectories at the given path.")]
static string ListFiles(
    [Description("Directory path (defaults to current directory).")] string path = ".")
{
    if (!Directory.Exists(path))
        return $"Error: directory not found — {path}";
    return string.Join("\n", Directory.GetFileSystemEntries(path)
        .Select(Path.GetFileName).OrderBy(n => n)!);
}

[Description("Reads the text content of a file.")]
static string ReadFile(
    [Description("Path to the file.")] string filePath)
{
    if (!File.Exists(filePath)) return $"Error: file not found — {filePath}";
    try { return File.ReadAllText(filePath); }
    catch (Exception ex) { return $"Error: {ex.Message}"; }
}

[Description("Deletes a file. REQUIRES human approval before executing.")]
static string DeleteFile(
    [Description("Path of the file to delete.")] string filePath)
{
    // ── HITL approval gate ───────────────────────────────────────────────
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"  ⚠ Agent wants to DELETE: {filePath}");
    Console.ResetColor();
    Console.Write("  Approve? (yes/no): ");
    var answer = Console.ReadLine()?.Trim().ToLowerInvariant();

    if (answer is "yes" or "y")
    {
        try { File.Delete(filePath); return $"Deleted: {filePath}"; }
        catch (Exception ex) { return $"Error: {ex.Message}"; }
    }

    return $"Deletion of '{filePath}' was cancelled by the user.";
}

[Description("Writes text content to a file. REQUIRES human approval before executing.")]
static string WriteFile(
    [Description("Path of the file to write.")] string filePath,
    [Description("Text content to write.")] string content)
{
    // ── HITL approval gate ───────────────────────────────────────────────
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"  ⚠ Agent wants to WRITE: {filePath}");
    Console.WriteLine($"  Preview: {content[..Math.Min(120, content.Length)]}...");
    Console.ResetColor();
    Console.Write("  Approve? (yes/no): ");
    var answer = Console.ReadLine()?.Trim().ToLowerInvariant();

    if (answer is "yes" or "y")
    {
        try { File.WriteAllText(filePath, content); return $"Written: {filePath}"; }
        catch (Exception ex) { return $"Error: {ex.Message}"; }
    }

    return $"Write to '{filePath}' was cancelled by the user.";
}
