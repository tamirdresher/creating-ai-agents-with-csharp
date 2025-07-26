/*
 * SKCodeAssistent.Server - CodingAssistentSession_MagenticOrchestration.cs
 *
 * EDUCATIONAL IMPLEMENTATION - Advanced Multi-Agent Orchestration Pattern
 *
 * This implementation demonstrates advanced multi-agent orchestration using Semantic Kernel's
 * Magentic orchestration framework. It shows how to coordinate multiple specialized agents
 * in a collaborative environment while maintaining conversation context and history.
 *
 * Learning Objectives:
 * - Understanding multi-agent orchestration patterns
 * - Implementing conversation history management and summarization
 * - Integrating Model Context Protocol (MCP) for external service integration
 * - Managing agent runtime and lifecycle
 * - Implementing real-time orchestration monitoring
 * - Plugin system integration with multiple agents
 *
 * Key Concepts Demonstrated:
 * - Magentic Orchestration Framework: Advanced agent coordination
 * - Conversation Summarization: Managing long conversation context
 * - Runtime Management: InProcess runtime for agent execution
 * - Orchestration Monitoring: Real-time observation of agent interactions
 * - MCP Integration: External service integration through protocols
 * - Fallback Patterns: Single-agent mode when team mode isn't needed
 *
 * This implementation represents a sophisticated approach to AI agent collaboration,
 * suitable for complex software development tasks requiring multiple specialized perspectives.
 */

#pragma warning disable SKEXP0110 // Experimental APIs
#pragma warning disable SKEXP0001 // Experimental APIs

using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Magentic;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using SKCodeAssistent.Server.Configuration;
using SKCodeAssistent.Server.Plugins;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Orchestration;
using System.Runtime.CompilerServices;

namespace SKCodeAssistent.Server.Services;

/// <summary>
/// Educational implementation demonstrating advanced multi-agent orchestration using Semantic Kernel's
/// Magentic framework. This class shows how to coordinate multiple specialized agents in a collaborative
/// environment with sophisticated conversation management and external service integration.
///
/// Educational Value:
/// - Demonstrates complex multi-agent orchestration patterns
/// - Shows conversation history management and summarization techniques
/// - Illustrates Model Context Protocol (MCP) integration for external services
/// - Provides examples of runtime management and agent lifecycle control
/// - Shows real-time orchestration monitoring and observation patterns
///
/// Architecture Features:
/// - Magentic orchestration for intelligent agent coordination
/// - Chat history summarization to manage conversation context
/// - InProcess runtime for efficient agent execution
/// - Orchestration monitoring for real-time observation
/// - MCP integration for GitHub and Microsoft Docs services
/// - Fallback to single-agent mode when appropriate
///
/// This implementation is suitable for complex software development scenarios requiring
/// multiple specialized perspectives and collaborative problem-solving.
/// </summary>
public class CodingAssistentSession_MagenticOrchestration : ICodingAssistentSession
{
    #region Private Fields
    
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<CodingAssistentSession> _logger;
    private readonly List<IKernelPlugin> _plugins;
    private readonly IOptions<AgentConfiguration> _agentConfiguration;
    
    // Core AI components
    private Kernel? _kernel;
    private ChatHistory? _history;
    private AgentGroupChat? _groupChat;
    
    // Specialized agents for different development roles
    private ChatCompletionAgent? _architectAgent;
    private ChatCompletionAgent? _developerAgent;
    private ChatCompletionAgent? _testerAgent;
    
    private bool _initialized;
    
    #endregion

    /// <summary>
    /// Initializes a new instance of the Magentic orchestration session.
    /// </summary>
    /// <param name="workspaceContext">Service for managing VSCode workspace context</param>
    /// <param name="agentConfiguration">Configuration for AI service providers</param>
    /// <param name="loggerFactory">Factory for creating loggers</param>
    /// <param name="logger">Logger instance for this session</param>
    /// <param name="plugins">Collection of plugins to extend agent capabilities</param>
    public CodingAssistentSession_MagenticOrchestration(
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

    /// <summary>
    /// Initializes the Magentic orchestration session by setting up the kernel, agents, and group chat.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;

        // Initialize the core Semantic Kernel with AI service configuration and plugins
        _kernel = await InitializeKernelAsync();
        
        // Initialize conversation history for maintaining context across interactions
        _history = new ChatHistory();
        
        // Create specialized agents using predefined configurations
        // Each agent gets a cloned kernel for isolation and independent operation
        _architectAgent = AgentDefinitions.CreateArchitectAgent(_kernel.Clone());
        _developerAgent = AgentDefinitions.CreateDeveloperAgent(_kernel.Clone());
        _testerAgent = AgentDefinitions.CreateTesterAgent(_kernel.Clone());
        
        // Create an agent group chat for coordinated multi-agent interactions
        _groupChat = new AgentGroupChat(_architectAgent, _developerAgent, _testerAgent);
        
        // Configure termination strategy to prevent runaway conversations
        _groupChat.ExecutionSettings.TerminationStrategy.MaximumIterations = 15;
        _groupChat.ExecutionSettings.TerminationStrategy.AutomaticReset = true;
        
        _initialized = true;
    }

    /// <summary>
    /// Processes user requests using either multi-agent orchestration or single-agent fallback.
    /// </summary>
    /// <param name="userMessage">The user's input message</param>
    /// <param name="mode">The assistant mode determining execution strategy</param>
    /// <param name="cancellationToken">Token for cancelling the operation</param>
    /// <returns>Stream of responses from the orchestrated agents</returns>
    public async IAsyncEnumerable<ChatMessageContent> ProcessUserRequestAsync(
        string userMessage,
        string mode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var workspacePath = _workspaceContext.WorkspacePath;
        var workspaceSource = _workspaceContext.IsWorkspaceSet ? "VSCode" : "default";
        _logger.LogInformation("Processing user request with workspace: {WorkspacePath} (source: {Source})",
            workspacePath, workspaceSource);

        // Handle team mode with multi-agent orchestration
        if (mode is AssistentModes.DevTeam)
        {
            // Get chat completion service for history summarization
            var chatCompletionService = _kernel!.GetRequiredService<IChatCompletionService>();
            
            // Create history reducer to manage conversation context
            // This prevents context window overflow in long conversations
            var historyReducer = new ChatHistorySummarizationReducer(chatCompletionService, 1) { UseSingleSummary = true };
            var historySummary = "";
            
            // Summarize existing conversation history if present
            if (_history!.Any())
            {
                var reducedHistory = await historyReducer.ReduceAsync(_history!, cancellationToken);
                historySummary = $"Summary of chat so far: {reducedHistory?.FirstOrDefault()?.Content}";
            }
            
            // Build contextual message with workspace information and conversation summary
            var contextualMessage =
                $@"""
                Working in workspace: {workspacePath}

                if the workspace is not set, don't attempt to write any file unless asked for.
                You are part of a collaborative team of agents, so pass requests to them if needed and if they are better suited for a task.
                Try to make the discussion short as possible and finish as early as you can to avoid many interactions and round of discussion.
            
                {historySummary}

                User Request: {userMessage}
                """;

            // Create orchestration monitor for real-time observation
            var orchestrationMonitor = new OrchestrationMonitor(_history!);
            
            // Configure Magentic orchestration with all three agents
            var agentsOrchestration =
               new MagenticOrchestration(
                   new StandardMagenticManager(chatCompletionService, new OpenAIPromptExecutionSettings())
                   {
                       MaximumInvocationCount = 15  // Limit function calls per agent
                   },
                   _architectAgent!,
                   _developerAgent!,
                   _testerAgent!)
               {
                   ResponseCallback = orchestrationMonitor.ObserveResponseAsync,  // Monitor responses
                   LoggerFactory = _loggerFactory,
               };
            
            // Initialize InProcess runtime for agent execution
            await using InProcessRuntime runtime = new();
            orchestrationMonitor.StartOrchestrationSession();
            await runtime.StartAsync();
            
            // Execute orchestration in background task
            var agentsDiscussionTask = Task.Run(async () =>
            {
                try
                {
                    // Start the orchestrated discussion
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
            
            // Stream responses from orchestration monitor in real-time
            await foreach (var response in orchestrationMonitor.ReadChannelAsync(cancellationToken))
            {
                yield return response;
            }
            
            // Wait for orchestration completion
            await agentsDiscussionTask;
        }
        else
        {
            // Handle single-agent modes (fallback pattern)
            var contextualMessage =
                $@"""
                Working in workspace: {workspacePath}
            
                if the workspace is not set, don't attempt to write any file unless explicitly asked for.
                You are working alone without a team of agents.
            
                User Request: {userMessage}
                """;

            // Select appropriate agent based on mode
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

            // Execute single-agent conversation
            var agentThread = new ChatHistoryAgentThread(_history!);
            _history!.AddUserMessage(contextualMessage);
            
            await foreach (var response in agent.InvokeAsync(agentThread, cancellationToken: cancellationToken))
            {
                yield return response;
            }
        }
    }

    /// <summary>
    /// Retrieves the chat history as an asynchronous stream.
    /// This method provides access to the conversation history for review or analysis.
    /// </summary>
    /// <param name="cancellationToken">Token for cancelling the operation</param>
    /// <returns>Async enumerable of chat messages from the conversation history</returns>
    public async IAsyncEnumerable<ChatMessageContent> GetChatHistoryAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var message in _groupChat!.GetChatMessagesAsync(cancellationToken))
        {
            yield return message;
        }
    }

    /// <summary>
    /// Clears the chat history and resets the group chat state.
    /// This method is useful for starting fresh conversations or managing memory usage.
    /// </summary>
    public void ClearChatHistory()
    {
        _groupChat!.IsComplete = false;
        _history!.Clear();
        _logger.LogInformation($"Chat history cleared");
    }

    /// <summary>
    /// Initializes the Semantic Kernel with AI service configuration and plugins.
    /// This method sets up the complete kernel environment including AI services,
    /// local plugins, and external MCP integrations.
    /// </summary>
    /// <returns>Configured Semantic Kernel instance</returns>
    private async Task<Kernel> InitializeKernelAsync()
    {
        var builder = Kernel.CreateBuilder();
        var agentConfig = _agentConfiguration.Value;
        
        // Configure AI service based on available configuration
        if (!string.IsNullOrEmpty(agentConfig.AzureOpenAI.Endpoint) && !string.IsNullOrEmpty(agentConfig.AzureOpenAI.ApiKey))
        {
            // Azure OpenAI configuration
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: agentConfig.AzureOpenAI.ModelId,
                endpoint: agentConfig.AzureOpenAI.Endpoint,
                apiKey: agentConfig.AzureOpenAI.ApiKey);
        }
        else if (!string.IsNullOrEmpty(agentConfig.OpenAI.Endpoint) && !string.IsNullOrEmpty(agentConfig.OpenAI.ModelId))
        {
            // OpenAI (or compatible) configuration
            builder.AddOpenAIChatCompletion(
                modelId: agentConfig.OpenAI.ModelId,
                endpoint: new Uri(agentConfig.OpenAI.Endpoint),
                apiKey: agentConfig.OpenAI.ApiKey);
        }
        else
        {
            throw new InvalidOperationException("No valid AI service configuration found. Please check your application configuration.");
        }
                
        var kernel = builder.Build();
        
        // Add local plugins to extend agent capabilities
        foreach (var plugin in _plugins)
        {
            kernel.Plugins.AddFromObject(plugin);
        }

        // Add Model Context Protocol (MCP) plugins for external service integration
        await AddMCPPluginsAsync(kernel);
        return kernel;
    }

    /// <summary>
    /// Adds Model Context Protocol (MCP) plugins to the kernel for external service integration.
    /// This method demonstrates how to integrate external services like GitHub and Microsoft Docs
    /// through the MCP standard protocol.
    /// </summary>
    /// <param name="kernel">The kernel to add MCP plugins to</param>
    async Task AddMCPPluginsAsync(Kernel kernel)
    {
        // Add GitHub integration through MCP stdio transport
        // This enables agents to interact with GitHub repositories, issues, and pull requests
        await AddMCPPluginAsync(kernel,
            "GitHub",
            new StdioClientTransport(new()
            {
                Name = "MCPServer",
                Command = "npx",  // Use npx to run the GitHub MCP server
                Arguments = ["-y", "@modelcontextprotocol/server-github"],
            }));

        // Add Microsoft Docs integration through MCP SSE transport
        // This enables agents to access Microsoft documentation and learning resources
        await AddMCPPluginAsync(kernel,
            "MSDocs",
            new SseClientTransport(
                new SseClientTransportOptions
                {
                    Endpoint = new Uri("https://learn.microsoft.com/api/mcp")
                }));
    }

    /// <summary>
    /// Adds a specific MCP plugin to the kernel using the provided transport.
    /// This method handles the connection setup, tool discovery, and plugin registration.
    /// </summary>
    /// <param name="kernel">The kernel to add the plugin to</param>
    /// <param name="name">Name of the plugin for identification</param>
    /// <param name="clientTransport">Transport mechanism for connecting to the MCP server</param>
    async Task AddMCPPluginAsync(Kernel kernel, string name, IClientTransport clientTransport)
    {
        // Create MCP client using the provided transport
        var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

        // Discover available tools from the MCP server
        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

        // Register discovered tools as kernel functions
        kernel.Plugins.AddFromFunctions(name, tools.Select(aiFunction => aiFunction.AsKernelFunction()));
    }



}
