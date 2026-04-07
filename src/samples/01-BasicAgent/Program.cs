/*
 * 01-BasicAgent — Microsoft Agent Framework GA (v1.0.0)
 *
 * The simplest possible agent:
 *  1. Create an OpenAI chat client
 *  2. Wrap it as an AIAgent using .AsAIAgent()
 *  3. Chat in a loop with streaming responses
 *
 * Prerequisites:
 *  - Copy .env.example → .env and set OPENAI_API_KEY (or GITHUB_TOKEN + OPENAI_ENDPOINT)
 *  - dotnet run
 */

using DotNetEnv;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

// ── Load .env configuration ──────────────────────────────────────────────────
Env.TraversePath().Load();

var apiKey   = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
               ?? throw new InvalidOperationException("Set OPENAI_API_KEY in .env or environment.");
var modelId  = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") ?? "gpt-4o-mini";
var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");

// ── Build the chat client ────────────────────────────────────────────────────
OpenAIClientOptions? options = endpoint is not null
    ? new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
    : null;

var chatClient = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient(modelId);

// ── Create an AIAgent ────────────────────────────────────────────────────────
// AsAIAgent() wraps the ChatClient as an AIAgent with instructions and a name.
AIAgent agent = chatClient.AsAIAgent(
    instructions: """
        You are a friendly C# tutor specialising in .NET 9 and the Microsoft Agent Framework.
        Give concise, accurate answers with short code snippets where helpful.
        """,
    name: "CSharpTutor");

// ── Interactive chat loop ────────────────────────────────────────────────────
Console.WriteLine("=== 01-BasicAgent (type 'exit' to quit) ===\n");

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    Console.Write("Agent: ");

    // RunStreamingAsync streams text chunks as they arrive from the model.
    await foreach (var chunk in agent.RunStreamingAsync(userInput))
        Console.Write(chunk);

    Console.WriteLine("\n");
}

Console.WriteLine("Goodbye!");
