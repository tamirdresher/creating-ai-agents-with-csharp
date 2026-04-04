/*
 * 01-BasicAgent — Microsoft Agent Framework GA (v1.0.0)
 *
 * This sample demonstrates the simplest possible agent:
 *  1. Get an OpenAI chat client
 *  2. Wrap it as an AIAgent
 *  3. Ask it a question and stream the response
 *
 * Prerequisites:
 *  - Copy .env.example → .env and fill in your API key / model
 *  - dotnet run
 */

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

// ── Load environment variables from .env (if present) ──────────────────────
Env.TraversePath().Load();

var apiKey   = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
               ?? throw new InvalidOperationException("Set OPENAI_API_KEY in .env or environment");
var modelId  = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") ?? "gpt-4o";
var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");  // optional (GitHub Models etc.)

// ── Build an IChatClient ───────────────────────────────────────────────────
OpenAIClientOptions? options = endpoint is not null
    ? new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
    : null;

IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient(modelId)
    .AsIChatClient();

// ── Create an AIAgent ──────────────────────────────────────────────────────
AIAgent agent = chatClient.AsAIAgent(
    name: "CSharpTutor",
    instructions: """
        You are a friendly C# tutor specialising in .NET 9 and the Microsoft Agent Framework.
        Give concise, accurate answers with short code snippets where helpful.
        """);

// ── Chat loop ──────────────────────────────────────────────────────────────
Console.WriteLine("=== 01-BasicAgent  (type 'exit' to quit) ===\n");

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    Console.Write("Agent: ");

    // RunStreamingAsync streams text chunks as they arrive from the model
    await foreach (var chunk in agent.RunStreamingAsync(userInput))
        Console.Write(chunk);

    Console.WriteLine("\n");
}

Console.WriteLine("Goodbye!");
