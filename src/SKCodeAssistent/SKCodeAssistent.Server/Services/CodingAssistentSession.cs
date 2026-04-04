/*
 * AICodeAssistant.Server - CodingAssistentSession.cs
 *
 * Base template implementation of a coding assistant session using the
 * Microsoft Agent Framework GA (v1.0.0, released April 2026).
 *
 * WORKSHOP TEMPLATE -- this file is intentionally minimal so participants
 * can fill in the TODOs step by step.
 *
 * See SCHOOL_SOLUTIONS/ for complete, runnable reference implementations
 * demonstrating each agent pattern.
 *
 * Key types:
 *   IChatClient          -- Microsoft.Extensions.AI abstraction for chat models
 *   AIAgent              -- Microsoft.Agents.AI high-level agent wrapper
 *   AIFunctionFactory    -- creates tools from C# methods
 *   AsAIAgent()          -- extension on IChatClient to create an AIAgent
 *
 * Quick reference:
 *   AIAgent agent = chatClient.AsAIAgent(
 *       name: "AgentName",
 *       instructions: "...",
 *       tools: [AIFunctionFactory.Create(MyTool)]);
 *
 *   string result = await agent.RunAsync("message");
 *
 *   await foreach (var chunk in agent.RunStreamingAsync("message"))
 *       yield return new AgentMessage(chunk, agent.Name);
 */

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SKCodeAssistent.Server.Configuration;
using SKCodeAssistent.Server.Plugins;
using SKCodeAssistent.Server.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SKCodeAssistent.Server.Services;

/// <summary>
/// Template implementation of <see cref="ICodingAssistentSession"/> using
/// Microsoft Agent Framework v1.0 (GA).
/// </summary>
public class CodingAssistentSession : ICodingAssistentSession
{
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<CodingAssistentSession> _logger;
    private readonly IOptions<AgentConfiguration> _agentConfiguration;

    private IChatClient? _chatClient;
    private AIAgent? _agent;
    private bool _initialized;

    public CodingAssistentSession(
        WorkspaceContextService workspaceContext,
        IOptions<AgentConfiguration> agentConfiguration,
        ILoggerFactory loggerFactory,
        ILogger<CodingAssistentSession> logger)
    {
        ArgumentNullException.ThrowIfNull(workspaceContext);
        ArgumentNullException.ThrowIfNull(agentConfiguration);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _workspaceContext = workspaceContext;
        _agentConfiguration = agentConfiguration;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        if (_initialized) return;

        _chatClient = CreateChatClient();

        // TODO Workshop Step 1 -- Create your agent
        //
        //   _agent = _chatClient.AsAIAgent(
        //       name: "CodingAssistant",
        //       instructions: "You are an expert C# coding assistant. " +
        //                     "The workspace is at: " + _workspaceContext.WorkspacePath,
        //       tools: BuildTools());
        //
        // See SCHOOL_SOLUTIONS/CodingAssistentSessions/ for complete examples.

        _initialized = true;
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<AgentMessage> ProcessUserRequestAsync(
        string userMessage,
        string mode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_initialized)
            throw new InvalidOperationException("Call InitializeAsync() before processing requests.");

        _logger.LogInformation("Processing request | mode={Mode} | workspace={Workspace}",
            mode, _workspaceContext.WorkspacePath);

        // TODO Workshop Step 2 -- Stream your agent response
        //
        //   await foreach (var chunk in _agent!.RunStreamingAsync(userMessage, cancellationToken))
        //       yield return new AgentMessage(chunk, _agent.Name);

        yield return new AgentMessage(
            "TODO: implement ProcessUserRequestAsync -- see SCHOOL_SOLUTIONS for reference.",
            "CodingAssistant");
    }

    // -- Private helpers --

    private IChatClient CreateChatClient()
    {
        var cfg = _agentConfiguration.Value;

        if (!string.IsNullOrWhiteSpace(cfg.AzureOpenAI.Endpoint)
            && !string.IsNullOrWhiteSpace(cfg.AzureOpenAI.ApiKey))
        {
            _logger.LogInformation("Using Azure OpenAI at {Endpoint}", cfg.AzureOpenAI.Endpoint);

            // TODO Workshop (Azure path):
            //   return new AzureOpenAIClient(
            //       new Uri(cfg.AzureOpenAI.Endpoint),
            //       new Azure.AzureKeyCredential(cfg.AzureOpenAI.ApiKey))
            //       .GetChatClient(cfg.AzureOpenAI.DeploymentName)
            //       .AsIChatClient();
        }
        else if (!string.IsNullOrWhiteSpace(cfg.OpenAI.ApiKey)
                 && !string.IsNullOrWhiteSpace(cfg.OpenAI.ModelId))
        {
            _logger.LogInformation("Using OpenAI model {Model}", cfg.OpenAI.ModelId);

            // TODO Workshop (OpenAI / GitHub Models path):
            //   var options = string.IsNullOrWhiteSpace(cfg.OpenAI.Endpoint)
            //       ? null
            //       : new OpenAIClientOptions { Endpoint = new Uri(cfg.OpenAI.Endpoint) };
            //   return new OpenAIClient(new ApiKeyCredential(cfg.OpenAI.ApiKey), options)
            //       .GetChatClient(cfg.OpenAI.ModelId)
            //       .AsIChatClient();
        }
        else
        {
            throw new InvalidOperationException(
                "No AI service configured. Set AzureOpenAI or OpenAI credentials " +
                "in appsettings/environment/Aspire.");
        }

        return null!; // unreachable -- TODO blocks above always return or throw
    }

    private static IList<AIFunction> BuildTools()
    {
        return
        [
            AIFunctionFactory.Create(GetCurrentUtcTime),
            // TODO: add more tools
        ];
    }

    [Description("Returns the current date and time in UTC.")]
    private static string GetCurrentUtcTime() =>
        DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss zzz");
}
