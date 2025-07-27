/*
 * SKCodeAssistent.Server - CodingAssistentSession_SingleAgent.cs
 *
 * EDUCATIONAL IMPLEMENTATION - Single Agent Pattern
 *
 * This implementation demonstrates the foundational single-agent pattern for AI coding assistance.
 * It shows how to work with individual specialized agents (Architect, Developer, Tester) in isolation,
 * each handling specific aspects of software development.
 *
 * Learning Objectives:
 * - Understanding basic agent initialization and configuration
 * - Agent role specialization and mode switching
 * - Workspace context integration
 * - Conversation history management
 * - Error handling and validation patterns
 *
 * Key Concepts Demonstrated:
 * - Agent Definition Pattern: Using predefined agent configurations
 * - Mode-based Agent Selection: Switching between different agent types
 * - Context Injection: Providing workspace information to agents
 * - Thread-based Conversation: Managing conversation state
 *
 * This is the simplest form of agent interaction and serves as the foundation
 * for more complex multi-agent orchestration patterns.
 */

#pragma warning disable SKEXP0110 // Experimental APIs
#pragma warning disable SKEXP0001 // Experimental APIs

#nullable enable

using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SKCodeAssistent.Server.Configuration;
using SKCodeAssistent.Server.Plugins;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS;
using System.Runtime.CompilerServices;

namespace SKCodeAssistent.Server.Services;

/// <summary>
/// Educational implementation demonstrating the single-agent pattern for AI coding assistance.
/// This class shows how to work with individual specialized agents, each handling specific
/// aspects of software development (architecture, coding, testing).
///
/// Educational Value:
/// - Demonstrates basic agent initialization and lifecycle management
/// - Shows mode-based agent selection and role specialization
/// - Illustrates workspace context integration
/// - Provides foundation for understanding more complex orchestration patterns
/// </summary>
public class CodingAssistentSession_SingleAgent : ICodingAssistentSession
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
    
    // Specialized agents for different development roles
    private ChatCompletionAgent? _architectAgent;
    private ChatCompletionAgent? _developerAgent;
    private ChatCompletionAgent? _testerAgent;
    
    private bool _initialized;
    
    #endregion

    /// <summary>
    /// Initializes a new instance of the single-agent coding assistant session.
    /// </summary>
    /// <param name="workspaceContext">Service for managing VSCode workspace context</param>
    /// <param name="agentConfiguration">Configuration for AI service providers</param>
    /// <param name="loggerFactory">Factory for creating loggers</param>
    /// <param name="logger">Logger instance for this session</param>
    /// <param name="plugins">Collection of plugins (not used in basic single-agent pattern)</param>
    public CodingAssistentSession_SingleAgent(
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
    /// Initializes the single-agent session by setting up the kernel and creating specialized agents.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;

        // Initialize the core Semantic Kernel with AI service configuration
        _kernel = await InitializeKernelAsync();

        // Initialize conversation history for maintaining context
        _history = new ChatHistory();
        
        // Create specialized agents using predefined configurations
        // Each agent gets a cloned kernel for isolation and independence
        _architectAgent = AgentDefinitions.CreateArchitectAgent(_kernel.Clone());
        _developerAgent = AgentDefinitions.CreateDeveloperAgent(_kernel.Clone());
        _testerAgent = AgentDefinitions.CreateTesterAgent(_kernel.Clone());

        _initialized = true;
    }

    /// <summary>
    /// Processes user requests using the appropriate single agent based on the selected mode.
    /// </summary>
    /// <param name="userMessage">The user's input message</param>
    /// <param name="mode">The assistant mode determining which agent to use</param>
    /// <param name="cancellationToken">Token for cancelling the operation</param>
    /// <returns>Stream of responses from the selected agent</returns>
    public async IAsyncEnumerable<ChatMessageContent> ProcessUserRequestAsync(
        string userMessage,
        string mode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var workspacePath = _workspaceContext.WorkspacePath;
        var workspaceSource = _workspaceContext.IsWorkspaceSet ? "VSCode" : "default";
        
        _logger.LogInformation("Processing user request with workspace: {WorkspacePath} (source: {Source})",
            workspacePath, workspaceSource);

        // Handle unsupported team mode in single-agent implementation
        if (mode is AssistentModes.DevTeam)
        {
            yield return new ChatMessageContent(AuthorRole.Assistant, $"Team mode not supported in single-agent implementation. Please use architect, developer, or tester mode.") { AuthorName = "System" };
            yield break;
        }

        // Build contextual message with workspace information
        // This ensures the agent understands the current working environment
        var contextualMessage = $@"""
Working in workspace: {workspacePath}

if the workspace is not set, don't attempt to write any file unless explicitly asked for.
You are working alone without a team of agents.

User Request: {userMessage}
""";

        // Select the appropriate agent based on the mode
        // This demonstrates the single-agent pattern where one agent handles the entire request
        ChatCompletionAgent? agent = mode switch
        {
            AssistentModes.Architect => _architectAgent,
            AssistentModes.Coder => _developerAgent,
            AssistentModes.Tester => _testerAgent,
            _ => null
        };

        // Handle invalid mode selection
        if (agent == null)
        {
            yield return new ChatMessageContent(AuthorRole.Assistant, $"Unknown agent mode: {mode}") { AuthorName = "System" };
            yield break;
        }

        // Create agent thread with conversation history and process the request
        var agentThread = new ChatHistoryAgentThread(_history!);
        _history!.AddUserMessage(contextualMessage);
        
        // Stream responses from the selected agent
        await foreach (var response in agent.InvokeAsync(agentThread, cancellationToken: cancellationToken))
        {
            yield return response;
        }
    }

    /// <summary>
    /// Initializes the Semantic Kernel with the configured AI service provider.
    /// </summary>
    /// <returns>Configured Semantic Kernel instance</returns>
    private Task<Kernel> InitializeKernelAsync()
    {
        var builder = Kernel.CreateBuilder();

        // Add logging factory for agent operations
        builder.Services.AddSingleton(_loggerFactory);

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
       
        return Task.FromResult(kernel);
    }
}
