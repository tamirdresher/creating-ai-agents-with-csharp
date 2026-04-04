using Microsoft.Extensions.AI;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using SKCodeAssistent.Server.Services;

namespace SKCodeAssistent.Server.Services;

/// <summary>
/// Manages AI coding assistant sessions.
/// Each session is created lazily on first use and cached by session ID.
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
        activity?.SetTag("SessionId", sessionId);

        var sessionTask = _sessions.GetOrAdd(sessionId, _ => CreateAndInitializeSessionAsync());
        return await sessionTask;
    }

    public ValueTask RemoveSessionAsync(Guid sessionId)
    {
        using var activity = ActivitySource.StartActivity(nameof(RemoveSessionAsync));
        activity?.SetTag("SessionId", sessionId);

        if (_sessions.TryRemove(sessionId, out _))
            _logger.LogInformation("Session {SessionId} removed", sessionId);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Processes a user request for the given session and streams back <see cref="AgentMessage"/> chunks.
    /// </summary>
    public async IAsyncEnumerable<AgentMessage> ProcessUserRequestAsync(
        Guid sessionId,
        string userMessage,
        string mode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity(nameof(ProcessUserRequestAsync));
        activity?.SetTag("SessionId", sessionId);

        var session = await GetOrCreateSessionAsync(sessionId);
        await foreach (var msg in session.ProcessUserRequestAsync(userMessage, mode, cancellationToken))
            yield return msg;
    }
}
