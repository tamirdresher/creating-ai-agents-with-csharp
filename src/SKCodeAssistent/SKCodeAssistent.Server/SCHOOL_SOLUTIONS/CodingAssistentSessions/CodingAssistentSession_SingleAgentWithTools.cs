/*
 * SKCodeAssistent.Server - CodingAssistentSession_SingleAgentWithTools.cs
 *
 * EDUCATIONAL IMPLEMENTATION - Single Agent with AIFunction Tools
 *
 * Extends the single-agent pattern by wiring up local tool functions
 * (CommandExecutionPlugin, FileOperationsPlugin) via AIFunctionFactory.Create().
 */

#nullable enable

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using Azure.AI.OpenAI;
using System.ClientModel;
using SKCodeAssistent.Server.Configuration;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Plugins;
using System.Runtime.CompilerServices;

namespace SKCodeAssistent.Server.Services;

/// <summary>
/// Single-agent session that augments the agent with AIFunction tools created from
/// <see cref="CommandExecutionPlugin"/> and <see cref="FileOperationsPlugin"/>.
/// </summary>
public class CodingAssistentSession_SingleAgentWithTools : ICodingAssistentSession
{
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILogger<CodingAssistentSession> _logger;
    private readonly IOptions<AgentConfiguration> _agentConfiguration;

    private AIAgent? _architectAgent;
    private AIAgent? _developerAgent;
    private AIAgent? _testerAgent;

    private bool _initialized;

    public CodingAssistentSession_SingleAgentWithTools(
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

    public Task InitializeAsync()
    {
        if (_initialized) return Task.CompletedTask;

        var tools = BuildTools();

        // Developer and Tester get the full tool set; Architect only needs read/list tools.
        var readOnlyTools = tools.Where(t =>
            t.Name is "read_file" or "list_files" or "file_exists" or "get_current_directory").ToList();

        _architectAgent = AgentDefinitions.CreateArchitectAgent(CreateChatClient(), readOnlyTools);
        _developerAgent = AgentDefinitions.CreateDeveloperAgent(CreateChatClient(), tools);
        _testerAgent    = AgentDefinitions.CreateTesterAgent(CreateChatClient(), tools);

        _initialized = true;
        return Task.CompletedTask;
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
                "Team mode is not supported in the single-agent-with-tools implementation. Use Architect, Coder, or Tester mode.",
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

    private List<AIFunction> BuildTools()
    {
        var cmdPlugin  = new CommandExecutionPlugin(_workspaceContext,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<CommandExecutionPlugin>.Instance);
        var filePlugin = new FileOperationsPlugin(_workspaceContext);

        return [.. cmdPlugin.GetFunctions(), .. filePlugin.GetFunctions()];
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
