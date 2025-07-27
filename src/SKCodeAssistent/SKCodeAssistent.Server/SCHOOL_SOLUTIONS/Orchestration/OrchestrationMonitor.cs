using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Orchestration
{
    class OrchestrationMonitor
    {
        Channel<ChatMessageContent> _channel = Channel.CreateUnbounded<ChatMessageContent>();
        private ChatHistory _history;

        public OrchestrationMonitor(ChatHistory history)
        {
            _history = history;
        }

      
        public async ValueTask ObserveResponseAsync(ChatMessageContent response)
        {
            await _channel.Writer.WriteAsync(response);
            _history.Add(response);
        }

        public void CompleteOrchestrationSession(Exception? error = null)
        {
            _channel.Writer.Complete(error);
        }

        public async IAsyncEnumerable<ChatMessageContent> ReadChannelAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var reader = _channel.Reader;
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                while (reader.TryRead(out var item))
                {
                    yield return item;
                }
            }
        }

        internal void StartOrchestrationSession()
        {
           _channel = Channel.CreateUnbounded<ChatMessageContent>();
        }
    }
}
