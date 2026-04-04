using System.Runtime.CompilerServices;
using System.Threading.Channels;
using SKCodeAssistent.Server.Services;

namespace SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Orchestration;

/// <summary>
/// Observes agent responses from an orchestration run and makes them available
/// as an <see cref="IAsyncEnumerable{AgentMessage}"/> stream via an in-memory channel.
/// </summary>
internal class OrchestrationMonitor
{
    private Channel<AgentMessage> _channel = Channel.CreateUnbounded<AgentMessage>();

    /// <summary>Resets the channel so the monitor can be re-used for a new orchestration session.</summary>
    public void StartOrchestrationSession() =>
        _channel = Channel.CreateUnbounded<AgentMessage>();

    /// <summary>Called by the workflow to push each streamed agent message into the channel.</summary>
    public async ValueTask ObserveResponseAsync(AgentMessage message) =>
        await _channel.Writer.WriteAsync(message);

    /// <summary>Signals that the orchestration is finished (or faulted).</summary>
    public void CompleteOrchestrationSession(Exception? error = null) =>
        _channel.Writer.Complete(error);

    /// <summary>Reads messages from the channel until the orchestration completes.</summary>
    public async IAsyncEnumerable<AgentMessage> ReadChannelAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var reader = _channel.Reader;
        while (await reader.WaitToReadAsync(cancellationToken))
        {
            while (reader.TryRead(out var item))
                yield return item;
        }
    }
}
