/*
 * SKCodeAssistent.Server - CodingAssistentSession_MultiAgentWorkflow.cs
 *
 * EDUCATIONAL IMPLEMENTATION - Multi-Agent Workflow (WorkflowBuilder)
 *
 * In DevTeam mode three agents are orchestrated sequentially:
 *   Architect → Developer → Tester
 * using the Agent Framework WorkflowBuilder + InProcessExecution.
 *
 * In single-agent modes the request is routed to the appropriate agent directly.
 */

#nullable enable

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
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
/// Multi-agent workflow session.  In DevTeam mode an Architect → Developer → Tester
/// pipeline is executed; in other modes a single agent handles the request.
/// </summary>
public class CodingAssistentSession_MultiAgentWorkflow : ICodingAssistentSession
{
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILogger<CodingAssistentSession> _logger;
    private readonly IOptions<AgentConfiguration> _agentConfiguration;

    private AIAgent? _architectAgent;
    private AIAgent? _developerAgent;
    private AIAgent? _testerAgent;

    private bool _initialized;

    public CodingAssistentSession_MultiAgentWorkflow(
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

        _architectAgent = AgentDefinitions.CreateArchitectAgent(CreateChatClient());
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
        var activeDoc     = _workspaceContext.ActiveDocumentPath;
        _logger.LogInformation("Processing request | mode={Mode} | workspace={Workspace}", mode, workspacePath);

        if (mode is AssistentModes.DevTeam)
        {
            var contextualMessage =
                $"""
                Working in workspace: {workspacePath}
                Active Document: {activeDoc}

                If the workspace is not set, don't attempt to write any file unless asked for.
                You are part of a collaborative team of agents — pass requests to teammates when they are better suited.
                Keep the discussion as short as possible; finish as early as you can.

                User Request: {userMessage}
                """;

            // Build a sequential Architect → Developer → Tester workflow.
            var workflow = AgentWorkflowBuilder.BuildSequential(
                [_architectAgent!, _developerAgent!, _testerAgent!]);

            StreamingRun run = await InProcessExecution.Default.RunStreamingAsync(
                workflow, contextualMessage, null, cancellationToken);
            await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

            await foreach (var evt in run.WatchStreamAsync().WithCancellation(cancellationToken))
            {
                if (evt is AgentResponseUpdateEvent updateEvt)
                    yield return new AgentMessage(
                        updateEvt.Update.Text ?? string.Empty,
                        updateEvt.Update.AgentId ?? updateEvt.Update.AuthorName ?? "Agent");
            }
        }
        else
        {
            var contextualMessage =
                $"""
                Working in workspace: {workspacePath}
                Active Document: {activeDoc}

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
