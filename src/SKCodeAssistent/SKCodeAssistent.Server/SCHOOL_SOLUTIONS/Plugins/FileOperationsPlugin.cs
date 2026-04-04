/*
 * SKCodeAssistent.Server - FileOperationsPlugin.cs
 *
 * Migrated from IKernelPlugin / [KernelFunction] to plain C# methods
 * wrapped via AIFunctionFactory.Create().
 */

using System.ComponentModel;
using Microsoft.Extensions.AI;
using SKCodeAssistent.Server.Services;

namespace SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Plugins;

/// <summary>
/// Provides AIFunction tools for file-system operations inside the workspace.
/// </summary>
public class FileOperationsPlugin
{
    private readonly WorkspaceContextService _workspaceContext;

    public FileOperationsPlugin(WorkspaceContextService workspaceContext)
    {
        _workspaceContext = workspaceContext;
    }

    /// <summary>Returns all tools exposed by this plugin as AIFunctions.</summary>
    public IEnumerable<AIFunction> GetFunctions() =>
    [
        AIFunctionFactory.Create(ReadFileAsync),
        AIFunctionFactory.Create(WriteFileAsync),
        AIFunctionFactory.Create(ListFiles),
        AIFunctionFactory.Create(FileExists),
        AIFunctionFactory.Create(CreateDirectory),
    ];

    [Description("Read the contents of a file")]
    public async Task<string> ReadFileAsync(
        [Description("The file path to read (relative to workspace)")] string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_workspaceContext.WorkspacePath, filePath);
            if (!File.Exists(fullPath))
                return $"File not found: {filePath}";

            var content = await File.ReadAllTextAsync(fullPath);
            return $"File: {filePath}\n\n{content}";
        }
        catch (Exception ex)
        {
            return $"Error reading file {filePath}: {ex.Message}";
        }
    }

    [Description("Write content to a file")]
    public async Task<string> WriteFileAsync(
        [Description("The file path to write to (relative to workspace)")] string filePath,
        [Description("The content to write")] string content)
    {
        if (!_workspaceContext.IsWorkspaceSet)
            return "Error: Workspace must be set before writing files.";

        try
        {
            var fullPath  = Path.Combine(_workspaceContext.WorkspacePath, filePath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(fullPath, content);
            return $"Successfully wrote to file: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error writing to file {filePath}: {ex.Message}";
        }
    }

    [Description("List files and directories in a directory")]
    public string ListFiles(
        [Description("The directory path (relative to workspace, empty for root)")] string directoryPath = "")
    {
        try
        {
            var fullPath = string.IsNullOrEmpty(directoryPath)
                ? _workspaceContext.WorkspacePath
                : Path.Combine(_workspaceContext.WorkspacePath, directoryPath);

            if (!Directory.Exists(fullPath))
                return $"Directory not found: {directoryPath}";

            var files       = Directory.GetFiles(fullPath, "*", SearchOption.TopDirectoryOnly);
            var directories = Directory.GetDirectories(fullPath, "*", SearchOption.TopDirectoryOnly);

            var result = new List<string>();

            if (directories.Length > 0)
            {
                result.Add("Directories:");
                result.AddRange(directories.Select(d => $"  {Path.GetFileName(d)}/"));
            }

            if (files.Length > 0)
            {
                result.Add("Files:");
                result.AddRange(files.Select(f => $"  {Path.GetFileName(f)}"));
            }

            if (result.Count == 0)
                result.Add("Directory is empty");

            return string.Join("\n", result);
        }
        catch (Exception ex)
        {
            return $"Error listing directory {directoryPath}: {ex.Message}";
        }
    }

    [Description("Check if a file exists")]
    public string FileExists(
        [Description("The file path to check (relative to workspace)")] string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_workspaceContext.WorkspacePath, filePath);
            return File.Exists(fullPath)
                ? $"File exists: {filePath}"
                : $"File does not exist: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error checking file {filePath}: {ex.Message}";
        }
    }

    [Description("Create a directory")]
    public string CreateDirectory(
        [Description("The directory path to create (relative to workspace)")] string directoryPath)
    {
        if (!_workspaceContext.IsWorkspaceSet)
            return "Error: Workspace must be set before creating directories.";

        try
        {
            var fullPath = Path.Combine(_workspaceContext.WorkspacePath, directoryPath);

            if (Directory.Exists(fullPath))
                return $"Directory already exists: {directoryPath}";

            Directory.CreateDirectory(fullPath);
            return $"Successfully created directory: {directoryPath}";
        }
        catch (Exception ex)
        {
            return $"Error creating directory {directoryPath}: {ex.Message}";
        }
    }
}
