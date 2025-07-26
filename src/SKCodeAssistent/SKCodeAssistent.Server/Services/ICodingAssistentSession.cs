/*
 * SKCodeAssistent.Server - ICodingAssistentSession.cs
 *
 * This interface defines the contract for all coding assistant session implementations.
 * It provides a unified API for different agent patterns and orchestration strategies.
 */

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Magentic;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Orchestration;
using System.Runtime.CompilerServices;

namespace SKCodeAssistent.Server.Services
{
    /// <summary>
    /// Defines the contract for coding assistant session implementations.
    /// This interface provides a unified API for different agent patterns and orchestration strategies.
    ///
    /// </summary>
    public interface ICodingAssistentSession
    {
        /// <summary>
        /// Initializes the coding assistant session asynchronously.        
        /// </summary>
        /// <returns>A task representing the asynchronous initialization operation</returns>
        Task InitializeAsync();

        /// <summary>
        /// Processes a user request and returns an asynchronous stream of chat responses.
        /// This method handles the core interaction flow between users and AI agents.
        ///
        ///
        /// The streaming approach allows for:
        /// - Immediate response feedback to users
        /// - Cancellation of long-running requests
        /// - Progressive result delivery
        /// - Better user experience in interactive scenarios
        /// </summary>
        /// <param name="userMessage">The user's input message or request</param>
        /// <param name="mode">The assistant mode determining behavior (e.g., "architect", "developer", "tester", "devteam")</param>
        /// <param name="cancellationToken">Token for cancelling the operation if needed</param>
        /// <returns>An asynchronous enumerable of chat message responses from the agent(s)</returns>
        IAsyncEnumerable<ChatMessageContent> ProcessUserRequestAsync(
           string userMessage,
           string mode,
           CancellationToken cancellationToken = default);
    }
}