#pragma warning disable SKEXP0110 // Experimental APIs
#pragma warning disable SKEXP0001 // Experimental APIs

#nullable enable

using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;
using SKCodeAssistent.Server.Configuration;
using SKCodeAssistent.Server.Plugins;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS;
using System.Runtime.CompilerServices;

namespace SKCodeAssistent.Server.Services;

public class CodingAssistentSession_SingleAgentWithPluginsAndMCP : ICodingAssistentSession
{
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<CodingAssistentSession> _logger;
    private readonly List<IKernelPlugin> _plugins;
    private IOptions<AgentConfiguration> _agentConfiguration;
    private Kernel? _kernel;
    private ChatHistory? _history;
    private ChatCompletionAgent? _architectAgent;
    private ChatCompletionAgent? _developerAgent;
    private ChatCompletionAgent? _testerAgent;
    private bool _initialized;

    public CodingAssistentSession_SingleAgentWithPluginsAndMCP(
        WorkspaceContextService workspaceContext,
        IOptions<AgentConfiguration> agentConfiguration,
        ILoggerFactory loggerFactory,
        ILogger<CodingAssistentSession> logger,
        IEnumerable<IKernelPlugin> plugins)
    {
        ArgumentNullException.ThrowIfNull(workspaceContext);
        ArgumentNullException.ThrowIfNull(agentConfiguration);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(plugins);

        _workspaceContext = workspaceContext;
        _loggerFactory = loggerFactory;
        _logger = logger;
        _plugins = plugins.ToList();
        _agentConfiguration = agentConfiguration;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        _kernel = await InitializeKernelAsync();

        _history = new ChatHistory();
        _architectAgent = AgentDefinitions.CreateArchitectAgent(_kernel.Clone());
        _developerAgent = AgentDefinitions.CreateDeveloperAgent(_kernel.Clone());
        _testerAgent = AgentDefinitions.CreateTesterAgent(_kernel.Clone());

        _initialized = true;
    }

    public async IAsyncEnumerable<ChatMessageContent> ProcessUserRequestAsync(
        string userMessage,
        string mode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var workspacePath = _workspaceContext.WorkspacePath;
        var workspaceSource = _workspaceContext.IsWorkspaceSet ? "VSCode" : "default";
        _logger.LogInformation("Processing user request with workspace: {WorkspacePath} (source: {Source})",
            workspacePath, workspaceSource);

        if (mode is AssistentModes.DevTeam)
        {
            yield return new ChatMessageContent(AuthorRole.Assistant, $"Unknown agent: {mode}") { AuthorName = "System" };
        }
        else
        {
            var contextualMessage =
                $@"""
                Working in workspace: {workspacePath}
            
                if the workspace is not set, don't attempt to write any file unless explictly asked for.
                You are working alone without a team of agents.
            
                User Request: {userMessage}
                """;

            ChatCompletionAgent? agent = mode switch
            {
                AssistentModes.Architect => _architectAgent,
                AssistentModes.Coder => _developerAgent,
                AssistentModes.Tester => _testerAgent,
                _ => null
            };
            if (agent == null)
            {
                yield return new ChatMessageContent(AuthorRole.Assistant, $"Unknown agent: {mode}") { AuthorName = "System" };
                yield break;
            }

            var agentThread = new ChatHistoryAgentThread(_history!);
            _history!.AddUserMessage(contextualMessage);
            
            await foreach (var response in agent.InvokeAsync(agentThread, cancellationToken: cancellationToken))
            {
                yield return response;
            }
        }
    }

    private async Task<Kernel> InitializeKernelAsync()
    {
        var builder = Kernel.CreateBuilder();
        var agentConfig = _agentConfiguration.Value;

        if (!string.IsNullOrEmpty(agentConfig.AzureOpenAI.Endpoint) && !string.IsNullOrEmpty(agentConfig.AzureOpenAI.ApiKey))
        {
            builder.AddAzureOpenAIChatCompletion(deploymentName: agentConfig.AzureOpenAI.ModelId, endpoint: agentConfig.AzureOpenAI.Endpoint, apiKey: agentConfig.AzureOpenAI.ApiKey);
        }
        else if (!string.IsNullOrEmpty(agentConfig.OpenAI.Endpoint) && !string.IsNullOrEmpty(agentConfig.OpenAI.ModelId))
        {
            builder.AddOpenAIChatCompletion(modelId: agentConfig.OpenAI.ModelId, endpoint: new Uri(agentConfig.OpenAI.Endpoint), apiKey: agentConfig.OpenAI.ApiKey);
        }
        else
        {
            throw new InvalidOperationException("No valid AI service configuration found. Please check your application configuration.");
        }

        var kernel = builder.Build();

        foreach (var plugin in _plugins)
        {
            kernel.Plugins.AddFromObject(plugin);
        }

        await AddMCPPluginsAsync(kernel);
        return kernel;
    }

    async Task AddMCPPluginsAsync(Kernel kernel)
    {
        await AddMCPPluginAsync(kernel,
            "GitHub",
            new StdioClientTransport(new()
            {
                Name = "MCPServer",
                Command = "npx",
                Arguments = ["-y", "@modelcontextprotocol/server-github"],
            }));

        await AddMCPPluginAsync(kernel,
            "MSDocs",
            new SseClientTransport(
                new SseClientTransportOptions
                {
                    Endpoint = new Uri("https://learn.microsoft.com/api/mcp")
                }));


    }

    async Task AddMCPPluginAsync(Kernel kernel, string name, IClientTransport clientTransport)
    {
        var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

        // Retrieve the list of tools available on the MCP server
        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

        kernel.Plugins.AddFromFunctions(name, tools.Select(aiFunction => aiFunction.AsKernelFunction()));
    }

}
