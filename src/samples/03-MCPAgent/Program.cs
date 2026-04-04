/*
 * 03-MCPAgent — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates connecting an AIAgent to Model Context Protocol (MCP) servers.
 * MCP lets agents talk to external tool servers over a standard protocol.
 *
 * This sample connects to two MCP servers:
 *   1. GitHub MCP server   — via stdio (runs npx under the hood)
 *   2. Microsoft Docs MCP  — via HTTP/SSE
 *
 * Prerequisites:
 *   - Node.js installed (for npx to run the GitHub MCP server)
 *   - GITHUB_PERSONAL_ACCESS_TOKEN set in .env (for GitHub server auth)
 *   - dotnet run
 */

using DotNetEnv;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;
using System.ClientModel;

// ── Load environment variables ────────────────────────────────────────────────
Env.TraversePath().Load();

var apiKey   = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
               ?? throw new InvalidOperationException("Set OPENAI_API_KEY in .env");
var modelId  = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") ?? "gpt-4o";
var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");

// ── Build IChatClient ─────────────────────────────────────────────────────────
OpenAIClientOptions? options = endpoint is not null
    ? new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
    : null;

IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient(modelId)
    .AsIChatClient();

// ── Connect to GitHub MCP server (stdio transport) ────────────────────────────
//
// The GitHub MCP server runs as a child process via npx.
// It exposes tools like: search_repositories, create_issue, get_file_contents, etc.
// GITHUB_PERSONAL_ACCESS_TOKEN is read by the server process from its environment.
Console.WriteLine("Connecting to GitHub MCP server...");
var githubTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name      = "GitHub",
    Command   = "npx",
    Arguments = ["-y", "@modelcontextprotocol/server-github"],
    // Pass the GitHub PAT to the MCP server process
    EnvironmentVariables = new Dictionary<string, string?>
    {
        ["GITHUB_PERSONAL_ACCESS_TOKEN"] =
            Environment.GetEnvironmentVariable("GITHUB_PERSONAL_ACCESS_TOKEN")
    }
});
var githubClient = await McpClient.CreateAsync(githubTransport);
var githubTools  = await githubClient.ListToolsAsync();
Console.WriteLine($"  GitHub tools: {githubTools.Count()}");

// ── Connect to Microsoft Docs MCP server (SSE/HTTP transport) ─────────────────
//
// The MS Docs MCP server is hosted at learn.microsoft.com and provides
// tools for searching and reading .NET / Azure documentation.
Console.WriteLine("Connecting to Microsoft Docs MCP server...");
var msDocsTransport = new HttpClientTransport(new HttpClientTransportOptions
{
    Name     = "MsDocs",
    Endpoint = new Uri("https://learn.microsoft.com/api/mcp")
});
var msDocsClient = await McpClient.CreateAsync(msDocsTransport);
var msDocsTools  = await msDocsClient.ListToolsAsync();
Console.WriteLine($"  MsDocs tools: {msDocsTools.Count()}");

// ── Build agent with all MCP tools ────────────────────────────────────────────
var allTools = githubTools.Concat(msDocsTools).Cast<AITool>().ToList();
Console.WriteLine($"\nTotal tools available to agent: {allTools.Count}\n");

AIAgent agent = chatClient.AsAIAgent(
    name: "MCPAgent",
    instructions: """
        You are a helpful assistant with access to GitHub and Microsoft Docs.
        Use GitHub tools to search repos, create issues, and read code.
        Use MsDocs tools to search and read .NET / Azure documentation.
        Always cite which tool you used to find information.
        """,
    tools: allTools);

// ── Chat loop ──────────────────────────────────────────────────────────────────
Console.WriteLine("=== 03-MCPAgent  (type 'exit' to quit) ===");
Console.WriteLine("Try: 'Search for dotnet/runtime repos' or 'What is IChatClient?'\n");

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
