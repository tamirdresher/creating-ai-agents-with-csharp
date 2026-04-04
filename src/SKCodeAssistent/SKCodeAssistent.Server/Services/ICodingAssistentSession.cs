/*
 * AICodeAssistant.Server - ICodingAssistentSession.cs
 *
 * Defines the contract for all coding assistant session implementations.
 * Uses Microsoft.Extensions.AI abstractions — NO Semantic Kernel dependency.
 *
 * Migrated from Semantic Kernel to Microsoft Agent Framework GA (v1.0.0, April 2026).
 */

using Microsoft.Extensions.AI;

namespace SKCodeAssistent.Server.Services
{
    /// <summary>
    /// Defines the contract for a coding assistant session.
    /// Implementations wire up one or more <see cref="Microsoft.Agents.AI.AIAgent"/>
    /// instances and return their streamed responses as <see cref="AgentMessage"/> chunks.
    /// </summary>
    public interface ICodingAssistentSession
    {
        /// <summary>
        /// Initialises the session — creates the agent(s), wires up tools / MCP servers, etc.
        /// Call once before the first <see cref="ProcessUserRequestAsync"/> call.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Processes a user request and streams back agent responses.
        /// </summary>
        /// <param name="userMessage">Natural-language request from the user.</param>
        /// <param name="mode">
        /// Optional assistant mode that selects which agent to front
        /// (e.g. "architect", "developer", "tester", "devteam").
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        IAsyncEnumerable<AgentMessage> ProcessUserRequestAsync(
            string userMessage,
            string mode,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// A single streamed chunk from an agent.
    /// </summary>
    /// <param name="Content">The text fragment.</param>
    /// <param name="AgentName">Which agent produced this chunk (null for single-agent sessions).</param>
    public record AgentMessage(string Content, string? AgentName = null);
}
