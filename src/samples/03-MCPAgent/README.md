# 03-MCPAgent

Connect an `AIAgent` to external tool servers using the **Model Context Protocol (MCP)**.
MCP is an open standard that lets any AI agent discover and call tools hosted in separate processes or remote services.

## What you'll learn

- How to create MCP clients for stdio (local process) and SSE (HTTP) transports
- How to list MCP tools and pass them to an `AIAgent`
- How multiple tool servers can be combined into one agent

## Architecture

```
┌─────────────┐    stdio     ┌─────────────────────────────┐
│  MCPAgent   │◄────────────►│  GitHub MCP server (npx)    │
│  (AIAgent)  │              └─────────────────────────────┘
│             │    HTTP/SSE  ┌─────────────────────────────┐
│             │◄────────────►│  MsDocs MCP server (remote) │
└─────────────┘              └─────────────────────────────┘
```

## Prerequisites

- .NET 9 SDK
- Node.js + npm (for the GitHub MCP server via `npx`)
- An OpenAI API key
- A GitHub Personal Access Token with `repo` and `read:org` scopes

## Setup

```bash
cp .env.example .env
# Set OPENAI_API_KEY and GITHUB_PERSONAL_ACCESS_TOKEN in .env
```

## Run

```bash
dotnet run
```

## Key code

```csharp
// Connect to GitHub MCP server (runs as a child process)
var transport = new StdioClientTransport(new StdioClientTransportOptions {
    Command = "npx", Arguments = ["-y", "@modelcontextprotocol/server-github"]
});
var client = await McpClientFactory.CreateAsync(transport);
var tools  = await client.GetMcpClientToolsAsync(); // IEnumerable<AIFunction>

// Pass tools to the agent — no other changes needed
AIAgent agent = chatClient.AsAIAgent("MCPAgent", "...", tools: tools);
```

## Next steps

- [04-MultiAgentWorkflow](../04-MultiAgentWorkflow/README.md) — chain agents into a pipeline
