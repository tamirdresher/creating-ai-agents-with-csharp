/*
 * SKCodeAssistent.Server - WorkspaceController.cs
 *
 * This controller manages workspace context information from the VSCode extension.
 * It provides endpoints for setting workspace paths and active document information,
 * enabling AI agents to understand their working environment.
 *
 * The controller integrates with the WorkspaceContextService to maintain state
 * about the current workspace and active document, which is essential for
 * context-aware AI assistance.
 */

using Microsoft.AspNetCore.Mvc;
using SKCodeAssistent.Server.Services;

namespace SKCodeAssistent.Server.Controllers;

/// <summary>
/// REST API controller for managing workspace context information.
/// This controller receives workspace and document context from the VSCode extension
/// and updates the WorkspaceContextService accordingly.
///
/// Educational Purpose: This controller demonstrates:
/// - RESTful API design for context management
/// - Integration between VSCode extension and server components
/// - Activity enrichment for observability
/// - Simple request/response patterns for state management
///
/// The workspace context is crucial for AI agents to understand:
/// - Where they are operating (workspace path)
/// - What file the user is currently working on (active document)
/// - How to scope their operations appropriately
/// </summary>
[ApiController]
[Route("api/workspace")]
public class WorkspaceController : ControllerBase
{
    private readonly WorkspaceContextService _workspaceContextService;

    /// <summary>
    /// Initializes a new instance of the WorkspaceController.
    /// </summary>
    /// <param name="workspaceContextService">Service for managing workspace context state</param>
    public WorkspaceController(WorkspaceContextService workspaceContextService)
    {
        _workspaceContextService = workspaceContextService;
    }

    /// <summary>
    /// Sets the workspace path from the VSCode extension.
    /// This endpoint is called when the extension detects a workspace change
    /// and needs to update the server with the new workspace location.
    ///
    /// </summary>
    /// <param name="request">Request containing the workspace path</param>
    /// <returns>HTTP 200 OK response indicating successful update</returns>
    [HttpPost("path")]
    public IActionResult SetWorkspacePath([FromBody] SetPathRequest request)
    {
        // Enrich the current activity with path information for tracing
        EnrichActivityInfo(request);

        // Update the workspace context service with the new path
        _workspaceContextService.WorkspacePath = request.Path;
        return Ok();
    }

    /// <summary>
    /// Sets the active document path from the VSCode extension.
    /// This endpoint is called when the user switches to a different file
    /// or when the extension needs to inform the server about the currently active document.
    ///
    /// </summary>
    /// <param name="request">Request containing the active document path</param>
    /// <returns>HTTP 200 OK response indicating successful update</returns>
    [HttpPost("activedocument")]
    public IActionResult SetWorkspaceActiveDocument([FromBody] SetPathRequest request)
    {
        // Enrich the current activity with document path information for tracing
        EnrichActivityInfo(request);

        // Update the workspace context service with the active document path
        _workspaceContextService.ActiveDocumentPath = request.Path;
        return Ok();
    }

    /// <summary>
    /// Enriches the current activity with request information for observability.
    /// This method adds telemetry data to the current activity for tracing and monitoring.
    ///
    /// </summary>
    /// <param name="request">The request containing path information to add to telemetry</param>
    private static void EnrichActivityInfo(SetPathRequest request)
    {
        var activity = System.Diagnostics.Activity.Current;
        if (activity != null)
        {
            // Add the path as a tag to the current activity for tracing
            activity.SetTag(nameof(request.Path), request.Path);
        }
    }

    /// <summary>
    /// Request model for setting workspace or document paths.
    /// This simple model is used by both workspace and active document endpoints.
    ///
    /// </summary>
    public class SetPathRequest
    {
        /// <summary>
        /// The file system path to set (workspace path or document path).
        /// </summary>
        public string Path { get; set; } = string.Empty;
    }
}
