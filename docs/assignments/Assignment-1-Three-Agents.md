# Assignment 1: Creating Three Agents with Semantic Kernel

## Objective
Create three specialized agents using Semantic Kernel: an Architect, Developer, and Tester. These agents need to be workspace-aware and work collaboratively on software development tasks.

## Requirements

### 1. Implementation Location
You need to implement your solution in:

[src\SKCodeAssistent\SKCodeAssistent.Server\Services\CodingAssistentSession.cs](../../src/SKCodeAssistent/SKCodeAssistent.Server/Services/CodingAssistentSession.cs)


### 2. Agent Specifications

#### Architect Agent
- **Role**: System design and architecture planning
- **Responsibilities**:
  - Analyze requirements and create technical specifications
  - Design system architecture and component interactions
  - Define interfaces and data models
  - Create implementation roadmaps

#### Developer Agent
- **Role**: Code implementation and development
- **Responsibilities**:
  - Implement features based on architectural designs
  - Write clean, maintainable code
  - Follow coding standards and best practices
  - Create unit tests for implemented features

#### Tester Agent
- **Role**: Quality assurance and testing
- **Responsibilities**:
  - Create comprehensive test plans
  - Identify potential bugs and edge cases
  - Validate implementations against requirements
  - Suggest improvements for code quality

### 3. Workspace Awareness
All agents must be aware of the current workspace using the WorkspaceContextService:
- Use dependency injection to access [`WorkspaceContextService`](../src/SKCodeAssistent/SKCodeAssistent.Server/Services/WorkspaceContextService.cs)
- Access the current workspace path via `WorkspaceContextService.WorkspacePath`
- Access the active document path via `WorkspaceContextService.ActiveDocumentPath`


#### WorkspaceContextService Usage
```csharp
// Inject WorkspaceContextService into your CodingAssistentSession
public CodingAssistentSession(WorkspaceContextService workspaceContext, ...)
{
    _workspaceContext = workspaceContext;
}

// Use workspace context in agent instructions
var workspacePath = _workspaceContext.WorkspacePath;
var activeDocument = _workspaceContext.ActiveDocumentPath;
```


### 4. Testing Your Implementation
1. Start the application by running the AppHost project and navigate to the chat interface or use the VS Code extension
2. Submit a software development request (e.g., "Create a simple calculator application")
3. Verify that all three agents participate in the conversation
4. Check that each agent contributes according to their role
5. Ensure agents reference the correct workspace path

### 5. Example Solution Reference
For guidance, you can refer to the example solution at:

[src\SKCodeAssistent\SKCodeAssistent.Server/SCHOOL_SOLUTIONS/CodingAssistentSessions/CodingAssistentSession_SingleAgent.cs](../../src/SKCodeAssistent/SKCodeAssistent.Server/SCHOOL_SOLUTIONS/CodingAssistentSessions/CodingAssistentSession_SingleAgent.cs)

You can also change the registrations in the [Program.cs](../../src/SKCodeAssistent/SKCodeAssistent.Server/Program.cs) to make the ```CodingAssistentSession_SingleAgent``` the active implementation

```csharp
// Single agent implementation - demonstrates basic agent usage
builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_SingleAgent>();
```

## Tips
- Start by understanding the existing [`CodingAssistentSession.cs`](../src/SKCodeAssistent/SKCodeAssistent.Server/Services/CodingAssistentSession.cs) structure
- Use the [2-SemanticKernel-Agents.ipynb](../../notebooks/2-SemanticKernel-Agents.ipynb) notebook to get up to speed on agents in Semantic Kernel 
- Test incrementally - start with one agent and add others
- Pay attention to agent instructions - they define the agent's behavior
- Make sure to handle the workspace context properly
- Review the example solution to understand the pattern


## Next Steps
Once you complete this assignment, proceed to Assignment 2 where you'll add plugins and MCP integration to enhance your agents' capabilities.