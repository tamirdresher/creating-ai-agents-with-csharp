using Microsoft.Extensions.AI;

namespace SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Orchestration;

/// <summary>
/// Summarises a conversation history into a single compact message using an
/// <see cref="IChatClient"/>. Replaces the SK ChatHistoryReducer pattern.
/// </summary>
public class ChatHistorySummarizationReducer
{
    private readonly IChatClient _chatClient;
    private readonly int _targetMessageCount;

    /// <summary>
    /// When true a single summary message is produced regardless of
    /// <paramref name="targetMessageCount"/>.
    /// </summary>
    public bool UseSingleSummary { get; set; }

    public ChatHistorySummarizationReducer(IChatClient chatClient, int targetMessageCount = 1)
    {
        _chatClient         = chatClient;
        _targetMessageCount = targetMessageCount;
    }

    /// <summary>
    /// Reduces the history to at most <c>targetMessageCount</c> messages (or one
    /// summary if <see cref="UseSingleSummary"/> is true).
    /// Returns <c>null</c> if there is nothing to summarise.
    /// </summary>
    public async Task<IEnumerable<ChatMessage>?> ReduceAsync(
        IList<ChatMessage> history,
        CancellationToken cancellationToken = default)
    {
        if (history.Count == 0) return null;

        var formatted = string.Join("\n", history.Select(m =>
            $"{m.Role}: {string.Concat(m.Contents.OfType<TextContent>().Select(t => t.Text))}"));

        var prompt =
            $"""
            Summarise the following conversation in a few sentences, preserving the key decisions and code artifacts produced:

            {formatted}
            """;

        var result = await _chatClient.GetResponseAsync(prompt, cancellationToken: cancellationToken);
        var summaryText = result.Text ?? string.Empty;

        return [new ChatMessage(ChatRole.Assistant, summaryText)];
    }
}