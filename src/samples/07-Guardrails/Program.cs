/*
 * 07-Guardrails — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates output validation using a dedicated "guard" agent.
 *
 * Architecture:
 *   1. Main agent (CodingAgent) processes the user's request.
 *   2. Guard agent (GuardAgent) independently evaluates the response.
 *      - It checks for harmful code, sensitive data, or off-topic content.
 *      - It replies with "SAFE: <reason>" or "BLOCKED: <reason>".
 *   3. Only SAFE responses reach the user.
 *
 * This pattern is framework-agnostic — the guard is just another AIAgent.
 * For production you would also want an input guardrail (before the main agent).
 */

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Text;

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

// ── Main agent: answers coding questions ──────────────────────────────────────
AIAgent mainAgent = chatClient.AsAIAgent(
    name: "CodingAgent",
    instructions: "You are a C# expert. Answer coding questions with code examples.");

// ── Guard agent: validates the main agent's output ────────────────────────────
AIAgent guardAgent = chatClient.AsAIAgent(
    name: "GuardAgent",
    instructions: """
        You are a content safety reviewer for an AI assistant.
        You will receive a user request and a draft response from another AI.

        Evaluate the draft response for:
        1. Harmful code (system commands that delete data, malware patterns, exploits)
        2. Exposure of sensitive data (passwords, API keys, PII)
        3. Relevance — the response should actually answer the user's question

        Reply with EXACTLY one of:
          SAFE: <one-sentence reason why it is safe>
          BLOCKED: <one-sentence reason why it is blocked>

        Do not include anything else in your reply.
        """);

// ── Chat loop ──────────────────────────────────────────────────────────────────
Console.WriteLine("=== 07-Guardrails: Two-Agent Safety Pipeline ===");
Console.WriteLine("The guard agent validates every response before it reaches you.\n");
Console.WriteLine("Try: 'How do I read a file in C#?' or try to get it to do something unsafe.\n");

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    // Step 1: collect main agent's response
    Console.Write("[CodingAgent thinking...] ");
    var mainResponse = await CollectResponseAsync(mainAgent, userInput);
    Console.WriteLine("done.");

    // Step 2: run guard agent — pass both the original request and the draft
    var guardPrompt = $"""
        User request: {userInput}

        Draft response:
        {mainResponse}
        """;

    var verdict = (await CollectResponseAsync(guardAgent, guardPrompt)).Trim();

    // Step 3: display result based on verdict
    Console.WriteLine();
    if (verdict.StartsWith("SAFE", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Agent: " + mainResponse);
        Console.WriteLine($"\n[Guard: ✅ {verdict}]");
    }
    else
    {
        Console.WriteLine($"🚫 Response blocked by guardrail:");
        Console.WriteLine($"   {verdict}");
    }

    Console.WriteLine();
}

Console.WriteLine("Goodbye!");

// ── Helper: collect full streaming response into a string ─────────────────────
static async Task<string> CollectResponseAsync(AIAgent agent, string message)
{
    var sb = new StringBuilder();
    await foreach (var chunk in agent.RunStreamingAsync(message))
        sb.Append(chunk);
    return sb.ToString();
}
