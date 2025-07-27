#pragma warning disable SKEXP0110 // Experimental APIs
#pragma warning disable SKEXP0001 // Experimental APIs


using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Chat;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Threading.Tasks;
using ModelContextProtocol.Client;

namespace SKCodeAssistent.Server.Services;

/// <summary>
/// Service for managing AI agents sessions
/// </summary>
public class CodingAssistentsSessionService
{
    private static readonly ActivitySource ActivitySource = new("SKCodeAssistent.Server.AgentManagementService");

    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Guid, Task<ICodingAssistentSession>> _sessions = new();
    private readonly ILogger<CodingAssistentsSessionService> _logger;

    public CodingAssistentsSessionService(
        IServiceProvider serviceProvider,
        ILogger<CodingAssistentsSessionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private async Task<ICodingAssistentSession> CreateAndInitializeSessionAsync()
    {
        var session = _serviceProvider.GetRequiredService<ICodingAssistentSession>();
        await session.InitializeAsync();
        return session;
    }



    private async Task<ICodingAssistentSession> GetOrCreateSessionAsync(Guid sessionId)
    {
        using var activity = ActivitySource.StartActivity(nameof(GetOrCreateSessionAsync));
        activity!.SetTag("SessionId", sessionId);

        var sessionTask = _sessions.GetOrAdd(sessionId, _ => CreateAndInitializeSessionAsync());
        return await sessionTask;
    }

    public ValueTask RemoveSessionAsync(Guid sessionId)
    {
        using var activity = ActivitySource.StartActivity(nameof(RemoveSessionAsync));
        activity!.SetTag("SessionId", sessionId);

        using (_logger.BeginScope(new Dictionary<string, object> { { "SessionId", sessionId }, { "Method", nameof(RemoveSessionAsync) } }))
        {
            if (_sessions.TryRemove(sessionId, out var sessionTask))
            {
                _logger.LogInformation($"Session {sessionId} removed and disposed");
            }
        }
        
        return ValueTask.CompletedTask;
    }

    public async IAsyncEnumerable<ChatMessageContent> ProcessUserRequestAsync(
        Guid sessionId,
        string userMessage,
        string mode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity(nameof(ProcessUserRequestAsync));
        activity!.SetTag("SessionId", sessionId);

        using (_logger.BeginScope(new Dictionary<string, object> { { "SessionId", sessionId }, { "Method", nameof(ProcessUserRequestAsync) } }))
        {
            var session = await GetOrCreateSessionAsync(sessionId);
            await foreach (var msg in session.ProcessUserRequestAsync(userMessage, mode, cancellationToken))
            {
                yield return msg;
            }
        }
    }
}