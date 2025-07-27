/*
 * SKCodeAssistent.Server - AgentController.cs
 *
 * This controller provides REST API endpoints for interacting with AI coding assistant agents.
 * It handles streaming responses from agents and session management for the VSCode extension.
 *
 * The controller uses Server-Sent Events (SSE) to stream real-time agent responses back to clients,
 * enabling a responsive chat-like experience in the VSCode extension.
 */

using Microsoft.AspNetCore.Mvc;
using SKCodeAssistent.Server.Services;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;

namespace SKCodeAssistent.Server.Controllers;

/// <summary>
/// REST API controller for AI coding assistant agent interactions.
/// Provides endpoints for streaming agent conversations and session management.
/// </summary>
[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("SKCodeAssistent.Server.Controllers.AgentController");
    private readonly CodingAssistentsSessionService _codingAssistentSessionService;

    /// <summary>
    /// Initializes a new instance of the AgentController.
    /// </summary>
    /// <param name="codingAssistentSessionService">Service for managing coding assistant sessions</param>
    public AgentController(CodingAssistentsSessionService codingAssistentSessionService)
    {
        _codingAssistentSessionService = codingAssistentSessionService;
    }

    /// <summary>
    /// Streams agent chat responses using Server-Sent Events (SSE).
    /// This endpoint provides real-time streaming of agent responses for interactive chat experiences.
    /// </summary>
    /// <param name="sessionId">Unique identifier for the coding assistant session</param>
    /// <param name="message">User message to send to the agent</param>
    /// <param name="mode">Agent mode (e.g., "Architect", "Coder", "Tester", "DevTeam")</param>
    /// <param name="cancellationToken">Cancellation token for request cancellation</param>
    /// <returns>Server-Sent Event stream of agent responses</returns>
    [HttpGet("stream")]
#pragma warning disable SKEXP0001 // Suppress SKEXP0001 warning for experimental Semantic Kernel APIs
    public async Task StreamAgentChatAaync([FromQuery] Guid sessionId, [FromQuery] string message, [FromQuery] string mode, CancellationToken cancellationToken)
    {
        // Create activity for distributed tracing and monitoring
        using var activity = ActivitySource.StartActivity(nameof(StreamAgentChatAaync));
        activity!.SetTag("SessionId", sessionId);
        activity!.SetTag("Mode", mode);

        // Configure response headers for Server-Sent Events
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no"; // Disable nginx buffering

        // Stream agent responses as they become available
        await foreach (var response in _codingAssistentSessionService.ProcessUserRequestAsync(sessionId, message, mode, cancellationToken))
        {
            // Only send non-empty responses to avoid unnecessary network traffic
            if (!string.IsNullOrWhiteSpace(response.Content))
            {
                // Serialize response as JSON with author and content information
                var data = JsonSerializer.Serialize(new { author = response.AuthorName, content = response.Content });
                await Response.WriteAsync($"data: {data}\n\n");
                await Response.Body.FlushAsync(); // Ensure immediate delivery
            }
        }

        // Send completion event to signal end of stream
        await Response.WriteAsync("event: done\ndata: {}\n\n");
        await Response.Body.FlushAsync();
    }
#pragma warning restore SKEXP0001

    /// <summary>
    /// Removes a coding assistant session and cleans up associated resources.
    /// This endpoint is called when a session is no longer needed (e.g., when VSCode workspace is closed).
    /// </summary>
    /// <param name="sessionId">Unique identifier of the session to remove</param>
    /// <returns>HTTP 204 No Content response indicating successful deletion</returns>
    [HttpDelete("session")]
    public IActionResult RemoveSession([FromQuery] Guid sessionId)
    {
        _codingAssistentSessionService.RemoveSessionAsync(sessionId);
        return NoContent();
    }
}
