#pragma warning disable SKEXP0110 // Experimental APIs
#pragma warning disable SKEXP0001 // Experimental APIs

using A2A;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.A2A;
using Microsoft.SemanticKernel.Agents.Magentic;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI; // <-- Add this for OpenAIPromptExecutionSettings
using ModelContextProtocol.Client;
using SKCodeAssistent.Server.Configuration;
using SKCodeAssistent.Server.Plugins;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Orchestration;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace SKCodeAssistent.Server.Services;

public class CodingAssistentSession_CustomOrchestrationWithA2A : ICodingAssistentSession
{
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<CodingAssistentSession> _logger;
    private readonly List<IKernelPlugin> _plugins;
    private IOptions<AgentConfiguration> _agentConfiguration;
    private Guid _id;
    private Kernel? _kernel;
    private ChatHistory? _history;
    private Agent _remoteDevAgent;
    private AgentGroupChat? _groupChat;
    private ChatCompletionAgent? _architectAgent;
    private ChatCompletionAgent? _developerAgent;
    private ChatCompletionAgent? _testerAgent;
    private bool _initialized;
    private A2AClient _a2aClient;

    public CodingAssistentSession_CustomOrchestrationWithA2A(
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
        _id = Guid.NewGuid();
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        _kernel = await InitializeKernelAsync();

        _history = new ChatHistory();
        _architectAgent = AgentDefinitions.CreateArchitectAgent(_kernel.Clone());
        _developerAgent = AgentDefinitions.CreateDeveloperAgent(_kernel.Clone());
        _testerAgent = AgentDefinitions.CreateTesterAgent(_kernel.Clone());

        var a2aAgentUrl = _agentConfiguration.Value.RemoteDevAgentUrl;

        var httpClient = new HttpClient();
        var url = new Uri(a2aAgentUrl);
        _a2aClient = new A2AClient(url, httpClient);
        var cardResolver = new A2ACardResolver(url, httpClient);
        var agentCard = await cardResolver.GetAgentCardAsync();
        _remoteDevAgent = new A2ARemoteAgent(new A2AAgent(_a2aClient, agentCard));

        _groupChat = new AgentGroupChat(_architectAgent, _developerAgent, _testerAgent);
        _groupChat.ExecutionSettings.TerminationStrategy.MaximumIterations = 15;
        _groupChat.ExecutionSettings.TerminationStrategy.AutomaticReset = true;
        _initialized = true;
    }

    public async IAsyncEnumerable<ChatMessageContent> ProcessUserRequestAsync(
        string userMessage,
        string mode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var workspacePath = _workspaceContext.WorkspacePath;
        var activeDoc = _workspaceContext.ActiveDocumentPath;
        _logger.LogInformation("Processing user request with workspace: {WorkspacePath} ", workspacePath);

        if (mode is AssistentModes.DevTeam)
        {
            var chatCompletionService = _kernel!.GetRequiredService<IChatCompletionService>();
            var historyReducer = new ChatHistorySummarizationReducer(chatCompletionService, 1) { UseSingleSummary = true };
            var historySummary = "";
            if (_history!.Any())
            {
                var reducedHistory = await historyReducer.ReduceAsync(_history!, cancellationToken);
                historySummary = $"Summary of chat so far: {reducedHistory?.FirstOrDefault()?.Content}";
            }
            var contextualMessage =
                $@"""
                Working in workspace: {workspacePath}
                Active Document: {activeDoc}

                if the workspace is not set, don't attempt to write any file unless asked for.
                You are part of a collaborative team of agents, so pass requests to them if needed and if they are better suited for a task.
                Try to make the discussion short as possible and finish as early as you can to avoid many interactions and round of discussion.
            
                {historySummary}

                User Request: {userMessage}
                """;

            var orchestrationMonitor = new OrchestrationMonitor(_history!);
            var agentsOrchestration =
               new GroupChatOrchestration(
                   new SoftwareTeamLeader(userMessage, chatCompletionService)
                   {
                       MaximumInvocationCount = 15
                   },
                   _architectAgent!,
                   //_developerAgent!,
                   _remoteDevAgent,
                   _testerAgent!)
               {
                   ResponseCallback = orchestrationMonitor.ObserveResponseAsync,
                   LoggerFactory = _loggerFactory,
               };
            await using InProcessRuntime runtime = new();
            orchestrationMonitor.StartOrchestrationSession();
            await runtime.StartAsync();
            var agentsDiscussionTask = Task.Run(async () =>
            {
                try
                {
                    var result = await agentsOrchestration.InvokeAsync(contextualMessage, runtime, cancellationToken);
                    var responseValue = await result.GetValueAsync();
                    await runtime.RunUntilIdleAsync();
                    orchestrationMonitor.CompleteOrchestrationSession();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during agent orchestration: {Message}", ex.Message);
                    await runtime.RunUntilIdleAsync();
                    orchestrationMonitor.CompleteOrchestrationSession(ex);
                }
            });
            await foreach (var response in orchestrationMonitor.ReadChannelAsync(cancellationToken))
            {
                yield return response;
            }
            await agentsDiscussionTask;
        }
        else
        {
            var contextualMessage =
                $@"""
                Working in workspace: {workspacePath}
                Active Document: {activeDoc}
            
                if the workspace is not set, don't attempt to write any file unless explictly asked for.
                You are working alone without a team of agents.
            
                User Request: {userMessage}
                """;

            Agent? agent = mode switch
            {
                AssistentModes.Architect => _architectAgent,
                AssistentModes.Coder => _remoteDevAgent,
                AssistentModes.Tester => _testerAgent,
                _ => null
            };
            if (agent == null)
            {
                yield return new ChatMessageContent(AuthorRole.Assistant, $"Unknown agent: {mode}") { AuthorName = "System" };
                yield break;
            }

            if ((agent is A2AAgent) || (agent is A2ARemoteAgent))
            {
                var agentThread = new A2AAgentThread(_a2aClient!, _id.ToString());
                _history!.AddUserMessage(contextualMessage);

                await foreach (var response in agent.InvokeAsync(_history.ToList(), cancellationToken: cancellationToken))
                {
                    yield return response;
                }
            }
            else
            {
                var agentThread = new ChatHistoryAgentThread(_history!);
                _history!.AddUserMessage(contextualMessage);

                await foreach (var response in agent.InvokeAsync(agentThread, cancellationToken: cancellationToken))
                {
                    yield return response;
                }
            }

        }
    }

    public async IAsyncEnumerable<ChatMessageContent> GetChatHistoryAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var message in _groupChat!.GetChatMessagesAsync(cancellationToken))
        {
            yield return message;
        }
    }

    public void ClearChatHistory()
    {
        _groupChat!.IsComplete = false;
        _history!.Clear();
        _logger.LogInformation($"Chat history cleared");
    }

    private async Task<Kernel> InitializeKernelAsync()
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddSingleton(_loggerFactory);

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
            new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri("https://learn.microsoft.com/api/mcp")
                }));


    }

    async Task AddMCPPluginAsync(Kernel kernel, string name, IClientTransport clientTransport)
    {
        var mcpClient = await McpClient.CreateAsync(clientTransport);

        // Retrieve the list of tools available on the MCP server
        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

        kernel.Plugins.AddFromFunctions(name, tools.Select(aiFunction => aiFunction.AsKernelFunction()));
    }


    public sealed class A2ARemoteAgent : Agent
    {
        
        private readonly A2AAgent _a2AAgent;


        public A2ARemoteAgent(A2AAgent a2AAgent)
        {
            
            _a2AAgent = a2AAgent;
            this.Name = a2AAgent.Name;
            this.Description = a2AAgent.Description;
        }

       

        public override IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InvokeAsync(ICollection<ChatMessageContent> messages, Microsoft.SemanticKernel.Agents.AgentThread? thread = null, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
        {
            StringBuilder fullMessage = new StringBuilder();
            foreach(var msg in messages)
            {
                fullMessage.AppendLine($"Role: {msg.Role} Content:{msg.Content}");
            }

            try
            {
                return _a2AAgent.InvokeAsync(new ChatMessageContent(AuthorRole.User, fullMessage.ToString()), thread, options).Select(m => { m.Message.AuthorName = Name; return m; });
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public override IAsyncEnumerable<AgentResponseItem<StreamingChatMessageContent>> InvokeStreamingAsync(ICollection<ChatMessageContent> messages, Microsoft.SemanticKernel.Agents.AgentThread? thread = null, AgentInvokeOptions? options = null, CancellationToken cancellationToken = default)
        {
            StringBuilder fullMessage = new StringBuilder();
            foreach (var msg in messages)
            {
                fullMessage.AppendLine($"Role: {msg.Role} Content:{msg.Content}");
            }
            try
            {
                return _a2AAgent.InvokeStreamingAsync(new ChatMessageContent(AuthorRole.User, fullMessage.ToString()), thread, options).Select(m => { m.Message.AuthorName = Name; return m; });
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        protected override Task<AgentChannel> CreateChannelAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException("A2AAgent is not for use with AgentChat.");
        }

        protected override IEnumerable<string> GetChannelKeys()
        {
            throw new NotSupportedException("A2AAgent is not for use with AgentChat.");
        }

        protected override Task<AgentChannel> RestoreChannelAsync(string channelState, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("A2AAgent is not for use with AgentChat.");
        }
    }

}


