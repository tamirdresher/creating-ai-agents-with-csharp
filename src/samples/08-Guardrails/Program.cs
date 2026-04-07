/*
 * 08-Guardrails — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates output validation using a guard agent pipeline.
 *
 * Architecture:
 *   1. Main agent (CodingAgent) processes the user's request
 *   2. Guard agent (SafetyGuard) evaluates the response for safety
 *   3. Only SAFE responses reach the user
 *
 * This is a framework-agnostic pattern — the guard is just another AIAgent.
 * For production, add an input guardrail too (before the main agent).
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

// ── Main agent: answers coding questions ─────────────────────────────────────
AIAgent mainAgent = new ChatClientAgent(chatClient,
    "You are a C# expert. Answer coding questions with clear code examples.",
    "CodingAgent");

// ── Guard agent: validates the main agent's output ───────────────────────────
AIAgent guardAgent = new ChatClientAgent(chatClient, """
    You are a content safety reviewer for an AI coding assistant.
    You receive a user request and a draft response from another AI.

    Evaluate the draft for:
    1. Harmful code (system commands that delete data, malware, exploits)
    2. Exposure of sensitive data (hardcoded passwords, API keys, PII)
    3. Relevance — the response should actually answer the user's question

    Reply with EXACTLY one of:
      SAFE: <one-sentence reason>
      BLOCKED: <one-sentence reason>

    Nothing else.
    """, "SafetyGuard");

// ── Input guard: checks user prompt before processing ────────────────────────
AIAgent inputGuard = new ChatClientAgent(chatClient, """
    You are an input safety filter. Evaluate the user's request.
    If the request asks for malware, exploits, attacks, or harmful content, block it.
    Normal coding questions are safe.

    Reply with EXACTLY one of:
      SAFE: <one-sentence reason>
      BLOCKED: <one-sentence reason>
    """, "InputGuard");

// ── Chat loop with safety pipeline ───────────────────────────────────────────
Console.WriteLine("=== 08-Guardrails: Two-Layer Safety Pipeline ===");
Console.WriteLine("Both input and output are validated by guard agents.\n");
Console.WriteLine("Try: 'How do I read a file in C#?' or ask something unsafe.\n");

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    // Step 1: Check input safety
    Console.Write("[InputGuard checking...] ");
    var inputVerdict = (await CollectResponseAsync(inputGuard, $"User request: {userInput}")).Trim();

    if (inputVerdict.StartsWith("BLOCKED", StringComparison.OrdinalIgnoreCase))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n🚫 Input blocked: {inputVerdict}");
        Console.ResetColor();
        Console.WriteLine();
        continue;
    }

    Console.WriteLine("passed.");

    // Step 2: Get main agent's response
    Console.Write("[CodingAgent thinking...] ");
    var mainResponse = await CollectResponseAsync(mainAgent, userInput);
    Console.WriteLine("done.");

    // Step 3: Validate output safety
    Console.Write("[SafetyGuard validating...] ");
    var guardPrompt = $"User request: {userInput}\n\nDraft response:\n{mainResponse}";
    var outputVerdict = (await CollectResponseAsync(guardAgent, guardPrompt)).Trim();

    if (outputVerdict.StartsWith("SAFE", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("passed.");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Guard: ✅ {outputVerdict}]");
        Console.ResetColor();
        Console.WriteLine($"\nAgent: {mainResponse}");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n🚫 Output blocked: {outputVerdict}");
        Console.ResetColor();
    }

    Console.WriteLine();
}

Console.WriteLine("Goodbye!");

// ── Helper ───────────────────────────────────────────────────────────────────
static async Task<string> CollectResponseAsync(AIAgent agent, string message)
{
    var sb = new StringBuilder();
    await foreach (var chunk in agent.RunStreamingAsync(message))
        sb.Append(chunk);
    return sb.ToString();
}
