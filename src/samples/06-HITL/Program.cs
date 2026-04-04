/*
 * 06-HITL (Human-in-the-Loop) — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates requiring human approval before an agent executes a
 * potentially dangerous action (file deletion in this example).
 *
 * The pattern works by implementing the approval gate *inside* the tool
 * function itself — the agent calls the tool, the tool pauses and asks the
 * user before proceeding.  No framework changes needed: it is plain C#.
 *
 * Key insight: AIFunctionFactory.Create() wraps any async method, including
 * ones that read from Console. The agent is unaware of the approval step.
 */

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.ComponentModel;

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

// ── Build tools with approval gates ───────────────────────────────────────────
var tools = new List<AITool>
{
    AIFunctionFactory.Create(ListFiles),
    AIFunctionFactory.Create(ReadFile),
    AIFunctionFactory.Create(DeleteFile),  // ← requires human approval
    AIFunctionFactory.Create(WriteFile),   // ← requires human approval
};

// ── Create agent ───────────────────────────────────────────────────────────────
AIAgent agent = chatClient.AsAIAgent(
    name: "FileManagerAgent",
    instructions: """
        You are a file management assistant. You can list, read, write, and delete files.
        IMPORTANT: Always list files or read them first before modifying or deleting.
        Explain what you are about to do before calling a destructive tool.
        """,
    tools: tools);

// ── Chat loop ──────────────────────────────────────────────────────────────────
Console.WriteLine("=== 06-HITL: Human-in-the-Loop File Manager ===");
Console.WriteLine("Try: 'List the files in the current directory' or 'Delete test.txt'\n");

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

// ── Tool implementations ───────────────────────────────────────────────────────

[Description("Lists files and subdirectories at the given path (defaults to current directory).")]
static string ListFiles(
    [Description("Directory path. Leave empty for the current directory.")] string path = ".")
{
    if (!Directory.Exists(path))
        return $"Error: directory not found — {path}";
    var entries = Directory.GetFileSystemEntries(path)
        .Select(Path.GetFileName)
        .OrderBy(n => n);
    return string.Join("\n", entries!);
}

[Description("Reads and returns the text content of a file.")]
static string ReadFile(
    [Description("Absolute or relative path to the file.")] string filePath)
{
    if (!File.Exists(filePath)) return $"Error: file not found — {filePath}";
    try { return File.ReadAllText(filePath); }
    catch (Exception ex) { return $"Error: {ex.Message}"; }
}

[Description("Deletes a file. REQUIRES explicit human approval before executing.")]
static string DeleteFile(
    [Description("Absolute or relative path of the file to delete.")] string filePath)
{
    // ── HITL approval gate ────────────────────────────────────────────────────
    Console.WriteLine();
    Console.WriteLine($"  ⚠️  Agent wants to DELETE: {filePath}");
    Console.Write("  Approve? (yes / no): ");
    var answer = Console.ReadLine()?.Trim().ToLowerInvariant();

    if (answer is "yes" or "y")
    {
        try
        {
            File.Delete(filePath);
            return $"✅ Deleted: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error deleting file: {ex.Message}";
        }
    }

    return $"❌ Deletion of '{filePath}' was cancelled by the user.";
}

[Description("Writes text content to a file. REQUIRES explicit human approval before executing.")]
static string WriteFile(
    [Description("Absolute or relative path of the file to write.")] string filePath,
    [Description("Text content to write into the file.")]             string content)
{
    // ── HITL approval gate ────────────────────────────────────────────────────
    Console.WriteLine();
    Console.WriteLine($"  ⚠️  Agent wants to WRITE to: {filePath}");
    Console.WriteLine($"  Content preview: {content[..Math.Min(80, content.Length)]}...");
    Console.Write("  Approve? (yes / no): ");
    var answer = Console.ReadLine()?.Trim().ToLowerInvariant();

    if (answer is "yes" or "y")
    {
        try
        {
            File.WriteAllText(filePath, content);
            return $"✅ Written: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error writing file: {ex.Message}";
        }
    }

    return $"❌ Write to '{filePath}' was cancelled by the user.";
}
