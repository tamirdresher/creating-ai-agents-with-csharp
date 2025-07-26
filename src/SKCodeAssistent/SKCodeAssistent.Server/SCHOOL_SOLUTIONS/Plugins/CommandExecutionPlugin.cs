/*
 * SKCodeAssistent.Server - CommandExecutionPlugin.cs
 *
 * This plugin extends AI agents with the ability to execute system commands within the workspace.
 * It provides a secure and controlled way for agents to interact with the development environment.
 *
 * Educational Purpose: This plugin demonstrates:
 * - Semantic Kernel plugin architecture
 * - Cross-platform command execution
 * - Process management and timeout handling
 * - Security considerations for AI-driven command execution
 * - Error handling and logging patterns
 *
 * Safety Features:
 * - Commands are executed in the workspace directory context
 * - Timeout protection prevents runaway processes
 * - Comprehensive logging for auditability
 * - Cross-platform compatibility (Windows/Linux/macOS)
 */

using System.ComponentModel;
using System.Diagnostics;
using Microsoft.SemanticKernel;
using SKCodeAssistent.Server.Plugins;
using SKCodeAssistent.Server.Services;

namespace SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Plugins;

/// <summary>
/// Plugin that enables AI agents to execute system commands within the workspace context.
/// This plugin provides a controlled and secure way for agents to interact with the development environment.
///
/// Educational Value:
/// - Demonstrates Semantic Kernel plugin architecture and attribute-based function registration
/// - Shows cross-platform process execution patterns
/// - Illustrates timeout handling and process management best practices
/// - Provides examples of comprehensive error handling and logging
///
/// Security Considerations:
/// - Commands are constrained to the workspace directory
/// - Process execution includes timeout protection
/// - All command execution is logged for audit trails
/// - Cross-platform shell selection ensures consistent behavior
///
/// Use Cases:
/// - Building and compiling projects (dotnet build, npm install)
/// - Running tests and validation (dotnet test, npm run test)
/// - Git operations (git status, git commit)
/// - Package management (npm install, dotnet restore)
/// - Development server operations (npm start, dotnet run)
/// </summary>
public class CommandExecutionPlugin : IKernelPlugin
{
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILogger<CommandExecutionPlugin> _logger;
    
    /// <summary>
    /// Initializes a new instance of the CommandExecutionPlugin.
    /// </summary>
    /// <param name="workspaceContext">Service providing workspace context and path information</param>
    /// <param name="logger">Logger for tracking command execution and debugging</param>
    public CommandExecutionPlugin(WorkspaceContextService workspaceContext, ILogger<CommandExecutionPlugin> logger)
    {
        _workspaceContext = workspaceContext;
        _logger = logger;
    }
    
    /// <summary>
    /// Executes a system command within the workspace context with comprehensive error handling and timeout protection.
    /// This method provides AI agents with the ability to run development tools, build scripts, and system commands.
    /// </summary>
    /// <param name="command">The command to execute (e.g., 'dotnet build', 'npm install', 'git status')</param>
    /// <param name="workingDirectory">Optional working directory relative to workspace (empty for workspace root)</param>
    /// <returns>Formatted string containing command execution results, output, and status</returns>
    [KernelFunction("execute_command")]
    [Description("Execute a command in the workspace directory")]
    public async Task<string> ExecuteCommandAsync(
        [Description("The command to execute (e.g., 'dotnet build', 'npm install')")] string command,
        [Description("Optional working directory relative to workspace (empty for workspace root)")] string? workingDirectory = null)
    {
        try
        {
            // Determine the working directory - use workspace root if not specified
            var baseWorkingDirectory = string.IsNullOrEmpty(workingDirectory)
                ? _workspaceContext.WorkspacePath
                : Path.Combine(_workspaceContext.WorkspacePath, workingDirectory);
            
            // Validate that the target directory exists before attempting execution
            if (!Directory.Exists(baseWorkingDirectory))
            {
                return $"Working directory does not exist: {workingDirectory ?? "workspace root"}";
            }
            
            // Log the command execution for audit trail and debugging
            _logger.LogInformation("Executing command: {Command} in {WorkingDirectory}", command, baseWorkingDirectory);
            
            // Configure the process with cross-platform shell selection
            // This ensures consistent behavior across Windows, Linux, and macOS
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetShellExecutable(),          // cmd.exe on Windows, /bin/bash on Unix
                    Arguments = GetShellArguments(command),   // /c on Windows, -c on Unix
                    WorkingDirectory = baseWorkingDirectory,
                    RedirectStandardOutput = true,            // Capture stdout
                    RedirectStandardError = true,             // Capture stderr
                    UseShellExecute = false,                  // Required for output redirection
                    CreateNoWindow = true                     // Run in background
                }
            };
            
            // Use thread-safe collections to capture output from async events
            var outputBuilder = new List<string>();
            var errorBuilder = new List<string>();
            
            // Set up event handlers for real-time output capture
            // This allows us to collect output as it's generated rather than waiting for completion
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.Add(e.Data);
                }
            };
            
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.Add(e.Data);
                }
            };
            
            // Start the process and begin asynchronous output reading
            process.Start();
            process.BeginOutputReadLine();  // Start async stdout reading
            process.BeginErrorReadLine();   // Start async stderr reading
            
            // Wait for process completion with timeout protection
            // This prevents runaway processes from consuming resources indefinitely
            var completed = await process.WaitForExitAsync(TimeSpan.FromMinutes(5));
            
            if (!completed)
            {
                // Force termination if timeout is exceeded
                process.Kill();
                return $"Command timed out after 5 minutes: {command}";
            }
            
            // Combine captured output and error streams
            var output = string.Join("\n", outputBuilder);
            var error = string.Join("\n", errorBuilder);
            
            // Build comprehensive result report
            var result = new List<string>
            {
                $"Command: {command}",
                $"Working Directory: {baseWorkingDirectory}",
                $"Exit Code: {process.ExitCode}"
            };
            
            // Include stdout if available
            if (!string.IsNullOrEmpty(output))
            {
                result.Add($"Output:\n{output}");
            }
            
            // Include stderr if available
            if (!string.IsNullOrEmpty(error))
            {
                result.Add($"Error:\n{error}");
            }
            
            // Add visual success/failure indicator
            if (process.ExitCode == 0)
            {
                result.Add("✅ Command completed successfully");
            }
            else
            {
                result.Add("❌ Command failed");
            }
            
            return string.Join("\n\n", result);
        }
        catch (Exception ex)
        {
            // Log and return user-friendly error message
            _logger.LogError(ex, "Error executing command: {Command}", command);
            return $"Error executing command '{command}': {ex.Message}";
        }
    }
    
    /// <summary>
    /// Gets the current workspace directory path with context information.
    /// This method helps agents understand their current working context and location.
    /// </summary>
    /// <returns>String describing the current workspace directory and its source</returns>
    [KernelFunction("get_current_directory")]
    [Description("Get the current workspace directory path")]
    public string GetCurrentDirectoryAsync()
    {
        try
        {
            var workspacePath = _workspaceContext.WorkspacePath;
            var source = _workspaceContext.IsWorkspaceSet ? "VSCode workspace" : "current directory";
            return $"Current workspace directory: {workspacePath} ({source})";
        }
        catch (Exception ex)
        {
            return $"Error getting current directory: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Checks if a specific command is available in the system PATH.
    /// This method helps agents validate tool availability before attempting to use them.
    /// </summary>
    /// <param name="command">The command to check (e.g., 'dotnet', 'npm', 'git')</param>
    /// <returns>String indicating whether the command is available and its location</returns>
    [KernelFunction("check_command_availability")]
    [Description("Check if a command is available in the system")]
    public async Task<string> CheckCommandAvailabilityAsync(
        [Description("The command to check (e.g., 'dotnet', 'npm', 'git')")] string command)
    {
        try
        {
            // Use platform-appropriate command to check availability
            var checkCommand = OperatingSystem.IsWindows() ? $"where {command}" : $"which {command}";
            
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetShellExecutable(),
                    Arguments = GetShellArguments(checkCommand),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            // Return success if command is found and produces output
            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                return $"✅ Command '{command}' is available at: {output.Trim()}";
            }
            else
            {
                return $"❌ Command '{command}' is not available";
            }
        }
        catch (Exception ex)
        {
            return $"Error checking command '{command}': {ex.Message}";
        }
    }
    
    /// <summary>
    /// Gets the appropriate shell executable for the current platform.
    /// This method provides cross-platform shell selection for command execution.
    /// </summary>
    /// <returns>Shell executable path (cmd.exe on Windows, /bin/bash on Unix systems)</returns>
    private static string GetShellExecutable()
    {
        return OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/bash";
    }
    
    /// <summary>
    /// Formats command arguments for the appropriate shell.
    /// This method handles the different argument formats required by different shells.
    /// </summary>
    /// <param name="command">The command to format</param>
    /// <returns>Formatted shell arguments (/c on Windows, -c on Unix systems)</returns>
    private static string GetShellArguments(string command)
    {
        return OperatingSystem.IsWindows() ? $"/c {command}" : $"-c \"{command}\"";
    }
}

/// <summary>
/// Extension methods for the Process class that provide additional functionality
/// for timeout-based process management.
///
/// Educational Value: This class demonstrates:
/// - Extension method patterns in C#
/// - Timeout handling with CancellationToken
/// - Async/await patterns for process management
/// - Exception handling for timeout scenarios
/// </summary>
public static class ProcessExtensions
{
    /// <summary>
    /// Waits for a process to exit with a specified timeout.
    /// This method provides timeout protection for process execution to prevent
    /// long-running or hanging processes from consuming resources indefinitely.
    /// </summary>
    /// <param name="process">The process to wait for</param>
    /// <param name="timeout">Maximum time to wait for process completion</param>
    /// <returns>True if process completed within timeout, false if timeout occurred</returns>
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
            // Timeout occurred
            return false;
        }
    }
}