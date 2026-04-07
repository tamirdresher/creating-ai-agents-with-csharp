# 03 — MCP Agent (Model Context Protocol)

Connect an agent to external tools via the Model Context Protocol standard.

## What You'll Learn

- Connecting to MCP servers using `McpClientFactory.CreateAsync()`
- Listing and using tools from MCP servers
- Passing MCP tools to an `AIAgent`

## Key APIs

| API | Purpose |
|-----|---------|
| `McpClientFactory.CreateAsync()` | Connects to an MCP server |
| `mcpClient.ListToolsAsync()` | Discovers available MCP tools |
| MCP tools as `AITool` | MCP tools integrate directly with agents |

## Prerequisites

- **Node.js** installed (for `npx` to run MCP servers)

## Run

```bash
cp .env.example .env
dotnet run
```

The sample connects to the MCP "everything" test server. To use the GitHub MCP server instead:

1. Set `GITHUB_PERSONAL_ACCESS_TOKEN` in `.env`
2. Change the command in `Program.cs` to `@modelcontextprotocol/server-github`

## Corresponding Notebook

📓 [3.2-MCP.ipynb](../../../notebooks/3.2-MCP.ipynb)
