# Assignment 2: Creating Plugins and Adding MCP Integration

## Objective
Enhance your agents from Assignment 1 by creating plugins and integrating with Model Context Protocol (MCP) servers. This will allow your agents to interact with files, execute commands, and access external knowledge sources.

## Prerequisites
- Complete Assignment 1 (Three Agents with Semantic Kernel)
- Have your agents working with WorkspaceContextService

## Requirements

### 1. Implementation Location
Continue implementing your solution in:

[src\SKCodeAssistent\SKCodeAssistent.Server\Services\CodingAssistentSession.cs](../../src/SKCodeAssistent/SKCodeAssistent.Server/Services/CodingAssistentSession.cs)

### 2. Plugin Development


**IMPORTANT**: All plugins must implement the [`IKernelPlugin`](../../src/SKCodeAssistent/SKCodeAssistent.Server/Plugins/IKernelPlugin.cs) interface for dependency injection to work properly.

```csharp
// Ensure your plugins implement IKernelPlugin
public interface IPlugin
{
    // Plugin identification and metadata
}

// Your plugins should look like:
public class FileSystemPlugin : IKernelPlugin
{
    // Implementation
}

public class CommandExecutionPlugin : IKernelPlugin  
{
    // Implementation
}
```

#### A. File System Plugin
Create a plugin that allows agents to interact with files in the workspace:

**Required Functions:**
- `ReadFile(string filePath)` - Read file contents
- `WriteFile(string filePath, string content)` - Write content to file
- `ListFiles(string directoryPath)` - List files in directory
- `CreateDirectory(string directoryPath)` - Create directories
- `DeleteFile(string filePath)` - Delete files (with safety checks)

**Safety Requirements:**
- All file operations must be restricted to the workspace directory
- Validate file paths to prevent directory traversal attacks
- Add proper error handling and logging
- Implement size limits for file operations

```csharp
// Example implementation structure
[Description("Allows agents to interact with files in the workspace")]
public class FileSystemPlugin : IPlugin
{
    private readonly WorkspaceContextService _workspaceContext;
    
    public FileSystemPlugin(WorkspaceContextService workspaceContext)
    {
        _workspaceContext = workspaceContext;
    }
    
    [KernelFunction]
    [Description("Read the contents of a file")]
    public async Task<string> ReadFile(
        [Description("The file path relative to workspace")] string filePath)
    {
        // Validate path is within workspace
        var fullPath = ValidateAndGetFullPath(filePath);
        // Implementation here
    }
    
    private string ValidateAndGetFullPath(string relativePath)
    {
        // Ensure path is within workspace bounds
        // Prevent directory traversal attacks
    }
}
```

#### B. Command Line Plugin
Create a plugin for executing system commands:

**Required Functions:**
- `ExecuteCommand(string command)` - Execute system commands

OR 

- `BuildProject()` - Execute `dotnet build`
- `RunTests()` - Execute `dotnet test`
- `RunApplication()` - Start the application

**Safety Requirements:**
- Restrict commands to safe, predefined operations
- Set working directory to workspace
- Implement timeouts for long-running commands
- Sanitize command inputs
- Log all command executions

```csharp
[Description("Allows agents to execute system commands safely")]
public class CommandExecutionPlugin : IPlugin
{
    private readonly WorkspaceContextService _workspaceContext;
    private readonly ILogger<CommandExecutionPlugin> _logger;
    
    [KernelFunction]
    [Description("Execute a dotnet build command")]
    public async Task<string> BuildProject(
        [Description("Optional project path")] string projectPath = "")
    {
        // Validate and execute dotnet build
        // Implementation here
    }
    
    [KernelFunction]
    [Description("Execute dotnet test command")]
    public async Task<string> RunTests(
        [Description("Optional test project path")] string testProjectPath = "")
    {
        // Implementation here
    }
}
```


#### Dependency Injection Registration
Register your plugins in the DI container:

```csharp
// In Program.cs or your DI configuration
services.AddSingleton<IPlugin, FileSystemPlugin>();
services.AddSingleton<IPlugin, CommandExecutionPlugin>();

// In your CodingAssistentSession
public CodingAssistentSession(
    WorkspaceContextService workspaceContext,
    IEnumerable<IPlugin> plugins,
    ...)
{
    _workspaceContext = workspaceContext;
    _plugins = plugins;
}
```

### 3. MCP (Model Context Protocol) Integration

#### A. Connect to Microsoft Docs MCP
Integrate with Microsoft's documentation MCP server to provide your agents with access to official documentation.

**Setup Steps:**
1. Configure MCP client connection
2. Add Microsoft Docs MCP server endpoint
3. Register MCP tools with your agents
4. Test documentation queries


#### B. Optional: Create Your Own MCP Server
For advanced students, create a custom MCP server that provides project-specific knowledge.

**Suggested MCP Server Features:**
- Project documentation search
- Code pattern recommendations
- Best practices database
- Troubleshooting guides




### 8. Testing Your Implementation

1. **File Operations Test:**
   - Ask agents to read existing files
   - Request file creation and modification
   - Test directory listing functionality

2. **Command Execution Test:**
   - Ask agents to build the project
   - Request test execution
   - Try running the application

3. **MCP Integration Test:**
   - Query Microsoft documentation
   - Ask for .NET-specific information
   - Test documentation search functionality

4. **Safety Validation:**
   - Try operations outside workspace (should fail)
   - Test with malicious paths (should be blocked)
   - Verify command restrictions work

### 9. Example Solution Reference
For guidance, refer to the example solution at:

[src\SKCodeAssistent\SKCodeAssistent.Server/SCHOOL_SOLUTIONS/CodingAssistentSessions/CodingAssistentSession_SingleAgentWithPluginsAndMCP.cs](../../src/SKCodeAssistent/SKCodeAssistent.Server/SCHOOL_SOLUTIONS/CodingAssistentSessions/CodingAssistentSession_SingleAgentWithPluginsAndMCP.cs)

Use the existing [`CommandExecutionPlugin.cs`](../src/SKCodeAssistent/SKCodeAssistent.Server/Plugins/CommandExecutionPlugin.cs) as reference


## Next Steps
Once you complete this assignment, proceed to Assignment 3 where you'll implement orchestration to make your agents work as a coordinated software development team.
