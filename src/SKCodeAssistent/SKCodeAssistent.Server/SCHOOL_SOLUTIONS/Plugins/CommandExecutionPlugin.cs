/*
 * SKCodeAssistent.Server - CommandExecutionPlugin.cs
 *
 * Enables AI agents to execute system commands within the workspace context.
 * Migrated from Semantic Kernel (IKernelPlugin / [KernelFunction]) to plain C# methods
 * wrapped with AIFunctionFactory.Create().
 *
 * Call GetFunctions() to obtain the list of AIFunction tools to pass to AsAIAgent().
 */

using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.AI;
using SKCodeAssistent.Server.Services;

namespace SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Plugins;

/// <summary>
/// Provides AIFunction tools for executing system commands in the workspace.
/// </summary>
public class CommandExecutionPlugin
{
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILogger<CommandExecutionPlugin> _logger;

    public CommandExecutionPlugin(WorkspaceContextService workspaceContext, ILogger<CommandExecutionPlugin> logger)
    {
        _workspaceContext = workspaceContext;
        _logger           = logger;
    }

    /// <summary>Returns all tools exposed by this plugin as AIFunctions.</summary>
    public IEnumerable<AIFunction> GetFunctions() =>
    [
        AIFunctionFactory.Create(ExecuteCommandAsync),
        AIFunctionFactory.Create(GetCurrentDirectoryAsync),
        AIFunctionFactory.Create(CheckCommandAvailabilityAsync),
    ];

    [Description("Execute a command in the workspace directory")]
    public async Task<string> ExecuteCommandAsync(
        [Description("The command to execute (e.g., 'dotnet build', 'npm install')")] string command,
        [Description("Optional working directory relative to workspace (empty for workspace root)")] string? workingDirectory = null)
    {
        try
        {
            var baseDir = string.IsNullOrEmpty(workingDirectory)
                ? _workspaceContext.WorkspacePath
                : Path.Combine(_workspaceContext.WorkspacePath, workingDirectory);

            if (!Directory.Exists(baseDir))
                return $"Working directory does not exist: {workingDirectory ?? "workspace root"}";

            _logger.LogInformation("Executing command: {Command} in {Dir}", command, baseDir);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName               = GetShellExecutable(),
                    Arguments              = GetShellArguments(command),
                    WorkingDirectory       = baseDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                }
            };

            var outputLines = new List<string>();
            var errorLines  = new List<string>();

            process.OutputDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) outputLines.Add(e.Data); };
            process.ErrorDataReceived  += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) errorLines.Add(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var completed = await process.WaitForExitAsync(TimeSpan.FromMinutes(5));
            if (!completed)
            {
                process.Kill();
                return $"Command timed out after 5 minutes: {command}";
            }

            var parts = new List<string>
            {
                $"Command: {command}",
                $"Working Directory: {baseDir}",
                $"Exit Code: {process.ExitCode}"
            };
            if (outputLines.Count > 0) parts.Add($"Output:\n{string.Join("\n", outputLines)}");
            if (errorLines.Count  > 0) parts.Add($"Error:\n{string.Join("\n", errorLines)}");
            parts.Add(process.ExitCode == 0 ? "✅ Command completed successfully" : "❌ Command failed");

            return string.Join("\n\n", parts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {Command}", command);
            return $"Error executing command '{command}': {ex.Message}";
        }
    }

    [Description("Get the current workspace directory path")]
    public string GetCurrentDirectoryAsync()
    {
        try
        {
            var source = _workspaceContext.IsWorkspaceSet ? "VSCode workspace" : "current directory";
            return $"Current workspace directory: {_workspaceContext.WorkspacePath} ({source})";
        }
        catch (Exception ex)
        {
            return $"Error getting current directory: {ex.Message}";
        }
    }

    [Description("Check if a command is available in the system")]
    public async Task<string> CheckCommandAvailabilityAsync(
        [Description("The command to check (e.g., 'dotnet', 'npm', 'git')")] string command)
    {
        try
        {
            var check = OperatingSystem.IsWindows() ? $"where {command}" : $"which {command}";
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName               = GetShellExecutable(),
                    Arguments              = GetShellArguments(check),
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                }
            };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output)
                ? $"✅ Command '{command}' is available at: {output.Trim()}"
                : $"❌ Command '{command}' is not available";
        }
        catch (Exception ex)
        {
            return $"Error checking command '{command}': {ex.Message}";
        }
    }

    private static string GetShellExecutable() =>
        OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/bash";

    private static string GetShellArguments(string command) =>
        OperatingSystem.IsWindows() ? $"/c {command}" : $"-c \"{command}\"";
}

/// <summary>Extension to wait for a process exit with a timeout.</summary>
public static class ProcessExtensions
{
    public static async Task<bool> WaitForExitAsync(this Process process, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
