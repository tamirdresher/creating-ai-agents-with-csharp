/*
 * 03-MCPAgent — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates connecting an AIAgent to Model Context Protocol (MCP) servers.
 * MCP is a standard protocol for exposing tools to AI agents.
 *
 * This sample:
 *   1. Connects to a local "everything" MCP server via stdio
 *   2. Lists the available tools
 *   3. Passes them to an AIAgent so it can call them
 *
 * Prerequisites:
 *   - Node.js installed (for npx to run MCP servers)
 *   - dotnet run
 *
 * Note: To use the GitHub MCP server instead, set GITHUB_PERSONAL_ACCESS_TOKEN
 * and change the command to: npx -y @modelcontextprotocol/server-github
 */

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

// ── Load .env ────────────────────────────────────────────────────────────────
Env.TraversePath().Load();

var apiKey   = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
               ?? throw new InvalidOperationException("Set OPENAI_API_KEY in .env");
var modelId  = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") ?? "gpt-4o-mini";
var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");

// ── Build IChatClient ────────────────────────────────────────────────────────
OpenAIClientOptions? options = endpoint is not null
    ? new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
    : null;

IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient(modelId)
    .AsIChatClient();

// ── Connect to an MCP server (stdio transport) ──────────────────────────────
// The "everything" MCP test server exposes sample tools for demonstration.
// Replace with any MCP server (GitHub, filesystem, etc.).
Console.WriteLine("Connecting to MCP server...");

await using var mcpClient = await McpClientFactory.CreateAsync(
    new McpServerConfig
    {
        Id = "everything",
        Name = "Everything",
        TransportType = "stdio",
        TransportOptions = new Dictionary<string, string>
        {
            ["command"] = "npx",
            ["arguments"] = "-y @modelcontextprotocol/server-everything"
        }
    });

// ── List MCP tools ──────────────────────────────────────────────────────────
var mcpTools = await mcpClient.ListToolsAsync();
Console.WriteLine($"MCP tools discovered: {mcpTools.Count}");
foreach (var tool in mcpTools.Take(10))
    Console.WriteLine($"  - {tool.Name}");

if (mcpTools.Count > 10)
    Console.WriteLine($"  ... and {mcpTools.Count - 10} more");

Console.WriteLine();

// ── Build agent with MCP tools ──────────────────────────────────────────────
// MCP tools implement AITool, so they can be passed directly to the agent.
var agentChatClient = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient(modelId);

AIAgent agent = agentChatClient.AsAIAgent(
    name: "MCPAgent",
    instructions: """
        You are a helpful assistant with access to MCP tools.
        Use the available tools when they can help answer the user's question.
        Always cite which tool you used and explain the result.
        """,
    tools: mcpTools.Cast<AITool>().ToList());

// ── Chat loop ───────────────────────────────────────────────────────────────
Console.WriteLine("=== 03-MCPAgent (type 'exit' to quit) ===");
Console.WriteLine("Try: 'echo hello world' or 'add 5 and 3'\n");

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
