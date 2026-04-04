/*
 * SKCodeAssistent.Server - CodingAssistentSession_MultiAgentWithA2A.cs
 *
 * EDUCATIONAL IMPLEMENTATION - Multi-Agent Workflow with A2A Remote Developer
 *
 * Like MultiAgentWorkflow, but the Developer agent is replaced by a remote agent
 * reachable via the Agent-to-Agent (A2A) protocol.
 *
 * The remote developer URL is configured in AgentConfiguration.RemoteDevAgentUrl.
 */

#nullable enable

using A2A;
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
using LocalAgentMessage = SKCodeAssistent.Server.Services.AgentMessage;
using A2AAgentMessage = A2A.AgentMessage;

namespace SKCodeAssistent.Server.Services;

/// <summary>
/// Multi-agent workflow where the Developer role is fulfilled by a remote A2A agent.
/// The Architect and Tester remain local AIAgents.
/// </summary>
public class CodingAssistentSession_MultiAgentWithA2A : ICodingAssistentSession
{
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILogger<CodingAssistentSession> _logger;
    private readonly IOptions<AgentConfiguration> _agentConfiguration;

    private AIAgent? _architectAgent;
    private AIAgent? _remoteDevAgent;   // backed by A2A call
    private AIAgent? _testerAgent;

    private bool _initialized;

    public CodingAssistentSession_MultiAgentWithA2A(
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

        var tools = BuildTools();

        _architectAgent = AgentDefinitions.CreateArchitectAgent(CreateChatClient());
        _testerAgent    = AgentDefinitions.CreateTesterAgent(CreateChatClient(), tools);

        // Create an AI agent that proxies calls to the remote A2A developer.
        _remoteDevAgent = await CreateA2AAgentAsync();

        _initialized = true;
    }

    public async IAsyncEnumerable<LocalAgentMessage> ProcessUserRequestAsync(
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

            // Architect → remote Developer → Tester
            var workflow = AgentWorkflowBuilder.BuildSequential(
                [_architectAgent!, _remoteDevAgent!, _testerAgent!]);

            StreamingRun run = await InProcessExecution.Default.RunStreamingAsync(
                workflow, contextualMessage, null, cancellationToken);
            await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

            await foreach (var evt in run.WatchStreamAsync().WithCancellation(cancellationToken))
            {
                if (evt is AgentResponseUpdateEvent updateEvt)
                    yield return new LocalAgentMessage(
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
                AssistentModes.Coder     => _remoteDevAgent,
                AssistentModes.Tester    => _testerAgent,
                _                        => null
            };

            if (agent is null)
            {
                yield return new LocalAgentMessage($"Unknown agent mode: {mode}", "System");
                yield break;
            }

            await foreach (var chunk in agent.RunStreamingAsync(contextualMessage).WithCancellation(cancellationToken))
                yield return new LocalAgentMessage(chunk.Text ?? string.Empty, agent.Name);
        }
    }

    // -- Helpers --

    /// <summary>
    /// Creates an AIAgent that wraps a remote A2A endpoint as a tool.
    /// The agent uses the local LLM only to forward/translate messages to the A2A service.
    /// </summary>
    private async Task<AIAgent> CreateA2AAgentAsync()
    {
        var a2aUrl    = _agentConfiguration.Value.RemoteDevAgentUrl;
        var httpClient = new HttpClient();
        var url       = new Uri(a2aUrl);

        var a2aClient     = new A2AClient(url, httpClient);
        var cardResolver  = new A2ACardResolver(url, httpClient);
        var agentCard     = await cardResolver.GetAgentCardAsync();

        // Wrap the A2A call as an AIFunction so the local agent can invoke it.
        var a2aTool = AIFunctionFactory.Create(
            async (string message) =>
            {
                var userMsg = new A2AAgentMessage
                {
                    Role  = MessageRole.User,
                    Parts = [new TextPart { Text = message }],
                };
                var response = await a2aClient.SendMessageAsync(
                    new MessageSendParams { Message = userMsg });

                A2AAgentMessage? reply = response switch
                {
                    AgentTask task          => task.Status.Message,
                    A2AAgentMessage msg     => msg,
                    _                       => null
                };

                return reply?.Parts?
                    .Select(p => p.AsTextPart()?.Text)
                    .Where(t => t is not null)
                    .FirstOrDefault() ?? string.Empty;
            },
            name: "call_remote_developer",
            description: "Delegates a software development task to the remote developer agent via A2A.");

        return CreateChatClient().AsAIAgent(
            name: agentCard.Name ?? "RemoteDeveloper",
            description: agentCard.Description ?? "Remote developer agent accessed via A2A.",
            instructions:
            """
            You are a proxy for a remote software developer agent.
            For every development task you receive, call the call_remote_developer tool with the full task description
            and return its response verbatim.
            """,
            tools: [a2aTool]);
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
