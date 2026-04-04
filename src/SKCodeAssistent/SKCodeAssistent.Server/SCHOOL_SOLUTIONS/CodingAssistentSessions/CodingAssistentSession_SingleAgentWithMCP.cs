/*
 * SKCodeAssistent.Server - CodingAssistentSession_SingleAgentWithMCP.cs
 *
 * EDUCATIONAL IMPLEMENTATION - Single Agent with AIFunction Tools + MCP
 *
 * Extends the tools pattern by also loading tools from two MCP servers:
 * - GitHub MCP server (stdio transport via npx)
 * - Microsoft Docs MCP server (HTTP/SSE transport)
 *
 * Key concept: McpClient.ListToolsAsync() returns IEnumerable<AIFunction> directly,
 * so the tools can be passed straight to AsAIAgent().
 */

#nullable enable

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using OpenAI;
using Azure.AI.OpenAI;
using System.ClientModel;
using SKCodeAssistent.Server.Configuration;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Plugins;
using System.Runtime.CompilerServices;

namespace SKCodeAssistent.Server.Services;

/// <summary>
/// Single-agent session that combines local AIFunction tools with tools discovered
/// from MCP servers (GitHub and Microsoft Docs).
/// </summary>
public class CodingAssistentSession_SingleAgentWithMCP : ICodingAssistentSession
{
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILogger<CodingAssistentSession> _logger;
    private readonly IOptions<AgentConfiguration> _agentConfiguration;

    private AIAgent? _architectAgent;
    private AIAgent? _developerAgent;
    private AIAgent? _testerAgent;

    private bool _initialized;

    public CodingAssistentSession_SingleAgentWithMCP(
        WorkspaceContextService workspaceContext,
        IOptions<AgentConfiguration> agentConfiguration,
        ILoggerFactory loggerFactory,
        ILogger<CodingAssistentSession> logger)
    {
        ArgumentNullException.ThrowIfNull(workspaceContext);
        ArgumentNullException.ThrowIfNull(agentConfiguration);
        ArgumentNullException.ThrowIfNull(logger);

        _workspaceContext = workspaceContext;
        _agentConfiguration = agentConfiguration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        var localTools = BuildLocalTools();
        var mcpTools   = await LoadMcpToolsAsync();
        var allTools   = localTools.Concat(mcpTools).ToList();

        var readOnlyTools = allTools.Where(t =>
            t.Name is "read_file" or "list_files" or "file_exists" or "get_current_directory").ToList();

        _architectAgent = AgentDefinitions.CreateArchitectAgent(CreateChatClient(), readOnlyTools);
        _developerAgent = AgentDefinitions.CreateDeveloperAgent(CreateChatClient(), allTools);
        _testerAgent    = AgentDefinitions.CreateTesterAgent(CreateChatClient(), allTools);

        _initialized = true;
    }

    public async IAsyncEnumerable<AgentMessage> ProcessUserRequestAsync(
        string userMessage,
        string mode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var workspacePath = _workspaceContext.WorkspacePath;
        _logger.LogInformation("Processing request | mode={Mode} | workspace={Workspace}", mode, workspacePath);

        if (mode is AssistentModes.DevTeam)
        {
            yield return new AgentMessage(
                "Team mode is not supported in the single-agent-with-MCP implementation. Use Architect, Coder, or Tester mode.",
                "System");
            yield break;
        }

        var contextualMessage =
            $"""
            Working in workspace: {workspacePath}

            If the workspace is not set, don't attempt to write any file unless explicitly asked for.
            You are working alone without a team of agents.

            User Request: {userMessage}
            """;

        AIAgent? agent = mode switch
        {
            AssistentModes.Architect => _architectAgent,
            AssistentModes.Coder     => _developerAgent,
            AssistentModes.Tester    => _testerAgent,
            _                        => null
        };

        if (agent is null)
        {
            yield return new AgentMessage($"Unknown agent mode: {mode}", "System");
            yield break;
        }

        await foreach (var chunk in agent.RunStreamingAsync(contextualMessage).WithCancellation(cancellationToken))
            yield return new AgentMessage(chunk.Text ?? string.Empty, agent.Name);
    }

    // -- Helpers --

    private List<AIFunction> BuildLocalTools()
    {
        var cmdPlugin  = new CommandExecutionPlugin(_workspaceContext,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<CommandExecutionPlugin>.Instance);
        var filePlugin = new FileOperationsPlugin(_workspaceContext);
        return [.. cmdPlugin.GetFunctions(), .. filePlugin.GetFunctions()];
    }

    private async Task<IEnumerable<AIFunction>> LoadMcpToolsAsync()
    {
        var tools = new List<AIFunction>();

        // GitHub MCP server — tools like search_repositories, create_issue, etc.
        try
        {
            var githubClient = await McpClient.CreateAsync(
                new StdioClientTransport(new StdioClientTransportOptions
                {
                    Name    = "GitHubMCP",
                    Command  = "npx",
                    Arguments = ["-y", "@modelcontextprotocol/server-github"],
                }));
            tools.AddRange(await githubClient.ListToolsAsync());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load GitHub MCP tools — continuing without them.");
        }

        // Microsoft Docs MCP server — tools for searching learn.microsoft.com
        try
        {
            var msDocsClient = await McpClient.CreateAsync(
                new HttpClientTransport(new HttpClientTransportOptions
                {
                    Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
                }));
            tools.AddRange(await msDocsClient.ListToolsAsync());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load MSDocs MCP tools — continuing without them.");
        }

        return tools;
    }

    private IChatClient CreateChatClient()
    {
        var cfg = _agentConfiguration.Value;

        if (!string.IsNullOrWhiteSpace(cfg.AzureOpenAI.Endpoint) && !string.IsNullOrWhiteSpace(cfg.AzureOpenAI.ApiKey))
        {
            return new AzureOpenAIClient(
                    new Uri(cfg.AzureOpenAI.Endpoint),
                    new Azure.AzureKeyCredential(cfg.AzureOpenAI.ApiKey))
                .GetChatClient(cfg.AzureOpenAI.ModelId)
                .AsIChatClient();
        }

        var options = string.IsNullOrWhiteSpace(cfg.OpenAI.Endpoint)
            ? null
            : new OpenAIClientOptions { Endpoint = new Uri(cfg.OpenAI.Endpoint) };

        return new OpenAIClient(new ApiKeyCredential(cfg.OpenAI.ApiKey), options)
            .GetChatClient(cfg.OpenAI.ModelId)
            .AsIChatClient();
    }
}
