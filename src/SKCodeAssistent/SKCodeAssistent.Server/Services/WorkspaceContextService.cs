using Microsoft.Extensions.Logging;

namespace SKCodeAssistent.Server.Services;

/// <summary>
/// Service for managing workspace context passed from the VSCode extension
/// </summary>
public class WorkspaceContextService
{
    public string WorkspacePath { get; set; } = "";
    public string ActiveDocumentPath { get; set; } = "";

    /// <summary>
    /// Indicates whether the workspace path has been set
    /// </summary>
    public bool IsWorkspaceSet => !string.IsNullOrEmpty(WorkspacePath);

    /// <summary>
    /// Clears the workspace context
    /// </summary>
    public void ClearWorkspaceContext()
    {
        WorkspacePath = string.Empty;
        ActiveDocumentPath = string.Empty;
    }
}