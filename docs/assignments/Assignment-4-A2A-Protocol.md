# Assignment 4: Agent-to-Agent (A2A) Protocol Implementation

## Objective
Learn to implement the Agent-to-Agent (A2A) protocol to enable standardized communication between distributed AI agents. You'll create specialized agents as independent services and coordinate them using the A2A protocol, simulating a microservices architecture for AI systems.

## Prerequisites
- Complete Assignment 1, 2, and 3
- Review [Notebook 6: A2A Protocol](../../notebooks/6-Agent-to-Agent-Protocol.ipynb)
- Understanding of REST APIs and HTTP communication
- Familiarity with microservices concepts

## Background: What is A2A Protocol?

The Agent-to-Agent (A2A) protocol is an open standard developed by Google for enabling AI agents to discover and communicate with each other, regardless of their underlying implementation or hosting infrastructure. Think of it as **HTTP for AI agents**.

### Key Concepts

**1. Agent Discovery**
- Agents expose their capabilities via `/.well-known/agent.json`
- Agent cards describe what the agent can do
- Enables dynamic agent discovery and integration

**2. Standardized Communication**
- JSON-RPC 2.0 based protocol
- Request/response message patterns
- Event-driven interactions

**3. Benefits**
- **Separation of Concerns**: Each agent has a specific responsibility
- **Independent Scaling**: Scale agents based on individual load
- **Technology Agnostic**: Mix different AI providers and models
- **Reusability**: Share agents across applications

## Requirements

### Part 1: Create Specialized Agent Services

Implement **three specialized A2A agent services**. Each agent should be a separate ASP.NET Core minimal API project:

#### Agent 1: Code Analysis Agent
**Purpose**: Analyzes code quality, identifies issues, and suggests improvements

**Capabilities**:
- Analyze code for common issues (security, performance, style)
- Calculate code complexity metrics
- Identify code smells and anti-patterns
- Suggest refactoring opportunities

**Agent Card** (`/.well-known/agent.json`):
```json
{
  "name": "CodeAnalysisAgent",
  "description": "Analyzes code quality and provides improvement suggestions",
  "version": "1.0.0",
  "capabilities": [
    "code_analysis",
    "complexity_metrics",
    "security_scan",
    "refactoring_suggestions"
  ],
  "endpoints": {
    "message": "/"
  }
}
```

#### Agent 2: Documentation Agent
**Purpose**: Generates and improves code documentation

**Capabilities**:
- Generate XML doc comments
- Create README files
- Generate API documentation
- Explain complex code sections

**Agent Card**:
```json
{
  "name": "DocumentationAgent",
  "description": "Generates and improves code documentation",
  "version": "1.0.0",
  "capabilities": [
    "generate_comments",
    "create_readme",
    "api_docs",
    "code_explanation"
  ],
  "endpoints": {
    "message": "/"
  }
}
```

#### Agent 3: Testing Agent
**Purpose**: Generates and reviews unit tests

**Capabilities**:
- Generate unit tests for code
- Identify missing test coverage
- Suggest edge cases to test
- Review existing tests for quality

**Agent Card**:
```json
{
  "name": "TestingAgent",
  "description": "Generates and reviews unit tests",
  "version": "1.0.0",
  "capabilities": [
    "generate_tests",
    "coverage_analysis",
    "edge_case_identification",
    "test_review"
  ],
  "endpoints": {
    "message": "/"
  }
}
```

### Part 2: Implement A2A Protocol Support

Each agent service must implement:

#### 1. Agent Card Endpoint
```csharp
// Program.cs
app.MapGet("/.well-known/agent.json", () => 
{
    return Results.Ok(new
    {
        name = "CodeAnalysisAgent",
        description = "Analyzes code quality and provides improvement suggestions",
        version = "1.0.0",
        capabilities = new[] 
        { 
            "code_analysis", 
            "complexity_metrics", 
            "security_scan",
            "refactoring_suggestions"
        },
        endpoints = new { message = "/" }
    });
});
```

#### 2. A2A Message Handler
```csharp
app.MapPost("/", async (A2AMessageRequest request, Kernel kernel) =>
{
    // Extract message content from A2A request
    var userMessage = request.Params.Message.Parts
        .FirstOrDefault(p => p.Kind == "text")?.Text;
    
    if (string.IsNullOrEmpty(userMessage))
        return Results.BadRequest("No message content provided");
    
    // Process with your agent
    var agent = /* create or get your agent */;
    var history = new ChatHistory();
    history.AddUserMessage(userMessage);
    
    var response = "";
    await foreach (var message in agent.InvokeAsync(history))
    {
        response += message.Content;
    }
    
    // Return A2A formatted response
    return Results.Ok(new A2AMessageResponse
    {
        JsonRpc = "2.0",
        Id = request.Id,
        Result = new A2AMessageResult
        {
            Message = new A2AMessage
            {
                Role = "assistant",
                MessageId = Guid.NewGuid().ToString(),
                Parts = new[]
                {
                    new A2AMessagePart
                    {
                        Kind = "text",
                        Text = response
                    }
                }
            }
        }
    });
});
```

#### 3. A2A Data Models
```csharp
public record A2AMessageRequest(
    string JsonRpc,
    string Id,
    string Method,
    A2AMessageParams Params
);

public record A2AMessageParams(
    string Id,
    A2AMessage Message
);

public record A2AMessage(
    string Role,
    string MessageId,
    A2AMessagePart[] Parts
);

public record A2AMessagePart(
    string Kind,
    string Text
);

public record A2AMessageResponse(
    string JsonRpc,
    string Id,
    A2AMessageResult Result
);

public record A2AMessageResult(
    A2AMessage Message
);
```

### Part 3: Create A2A Coordinator

Build a coordinator service that:

1. **Discovers Available Agents**
```csharp
public class A2AAgentDiscovery
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, AgentCard> _agentRegistry = new();
    
    public async Task<AgentCard> DiscoverAgentAsync(string agentUrl)
    {
        var cardUrl = $"{agentUrl}/.well-known/agent.json";
        var response = await _httpClient.GetFromJsonAsync<AgentCard>(cardUrl);
        
        if (response != null)
        {
            _agentRegistry[response.Name] = response;
        }
        
        return response;
    }
    
    public IEnumerable<AgentCard> GetAvailableAgents() 
        => _agentRegistry.Values;
}
```

2. **Routes Requests to Appropriate Agents**
```csharp
public class A2AOrchestrator
{
    private readonly A2AAgentDiscovery _discovery;
    private readonly HttpClient _httpClient;
    
    public async Task<string> ProcessRequestAsync(string userRequest)
    {
        // Analyze request to determine which agents to call
        var agentsToCall = DetermineRequiredAgents(userRequest);
        
        // Call agents via A2A protocol
        var results = new List<string>();
        foreach (var agent in agentsToCall)
        {
            var result = await CallAgentViaA2AAsync(agent, userRequest);
            results.Add(result);
        }
        
        // Synthesize responses
        return SynthesizeResponses(results);
    }
    
    private async Task<string> CallAgentViaA2AAsync(
        AgentCard agent, 
        string message)
    {
        var a2aRequest = new A2AMessageRequest(
            JsonRpc: "2.0",
            Id: Guid.NewGuid().ToString(),
            Method: "message/send",
            Params: new A2AMessageParams(
                Id: Guid.NewGuid().ToString(),
                Message: new A2AMessage(
                    Role: "user",
                    MessageId: Guid.NewGuid().ToString(),
                    Parts: new[]
                    {
                        new A2AMessagePart(
                            Kind: "text",
                            Text: message
                        )
                    }
                )
            )
        );
        
        var response = await _httpClient.PostAsJsonAsync(
            agent.BaseUrl, 
            a2aRequest);
            
        var a2aResponse = await response.Content
            .ReadFromJsonAsync<A2AMessageResponse>();
            
        return a2aResponse?.Result?.Message?.Parts
            ?.FirstOrDefault()?.Text ?? "";
    }
}
```

3. **Coordinates Multi-Agent Workflows**
```csharp
public async Task<WorkflowResult> ExecuteCodeReviewWorkflowAsync(string code)
{
    var results = new WorkflowResult();
    
    // Step 1: Analyze code
    results.Analysis = await CallAgentViaA2AAsync(
        _agentRegistry["CodeAnalysisAgent"],
        $"Analyze this code:\n{code}"
    );
    
    // Step 2: Generate documentation
    results.Documentation = await CallAgentViaA2AAsync(
        _agentRegistry["DocumentationAgent"],
        $"Generate documentation for:\n{code}"
    );
    
    // Step 3: Generate tests
    results.Tests = await CallAgentViaA2AAsync(
        _agentRegistry["TestingAgent"],
        $"Generate tests for:\n{code}"
    );
    
    return results;
}
```

### Part 4: Configuration with .NET Aspire

Integrate your A2A agents with .NET Aspire for service orchestration:

```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Register A2A agent services
var codeAnalysisAgent = builder.AddProject<CodeAnalysisAgent>("code-analysis")
    .WithHttpsEndpoint(port: 5001, name: "https");

var documentationAgent = builder.AddProject<DocumentationAgent>("documentation")
    .WithHttpsEndpoint(port: 5002, name: "https");

var testingAgent = builder.AddProject<TestingAgent>("testing")
    .WithHttpsEndpoint(port: 5003, name: "https");

// Register coordinator with references to all agents
var coordinator = builder.AddProject<A2ACoordinator>("coordinator")
    .WithReference(codeAnalysisAgent)
    .WithReference(documentationAgent)
    .WithReference(testingAgent)
    .WithHttpsEndpoint(port: 5000, name: "https");

builder.Build().Run();
```

## Testing Your Implementation

### 1. Test Individual Agents

Create a `.http` file to test each agent:

```http
### Test Agent Card Discovery
GET https://localhost:5001/.well-known/agent.json

### Test A2A Message to Code Analysis Agent
POST https://localhost:5001/
Content-Type: application/json

{
  "jsonrpc": "2.0",
  "id": "1",
  "method": "message/send",
  "params": {
    "id": "conv_123",
    "message": {
      "role": "user",
      "messageId": "msg_1",
      "parts": [
        {
          "kind": "text",
          "text": "Analyze this code: public void DoSomething() { Console.WriteLine(\"test\"); }"
        }
      ]
    }
  }
}
```

### 2. Test Coordinator

```csharp
// Test the complete workflow
var result = await orchestrator.ExecuteCodeReviewWorkflowAsync(@"
    public class Calculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
");

Console.WriteLine($"Analysis: {result.Analysis}");
Console.WriteLine($"Documentation: {result.Documentation}");
Console.WriteLine($"Tests: {result.Tests}");
```

### 3. Test with A2A Inspector (Optional)

Use the [A2A Inspector](https://github.com/a2aproject/a2a-inspector) to validate your implementation:

```bash
docker run -p 8080:8080 a2aproject/a2a-inspector
```

Navigate to http://localhost:8080 and test your agents.

## Deliverables

1. **Three A2A Agent Services**
   - Each in a separate project
   - Implementing agent card endpoint
   - Implementing A2A message handling
   - Using Semantic Kernel for AI processing

2. **A2A Coordinator Service**
   - Agent discovery implementation
   - Multi-agent workflow orchestration
   - Response aggregation

3. **Aspire AppHost Configuration**
   - All services registered
   - Service discovery configured
   - Proper endpoint configuration

4. **Testing Documentation**
   - Example requests for each agent
   - Workflow execution examples
   - Screenshots from Aspire dashboard

## Advanced Challenges (Optional)

1. **Circuit Breaker Pattern**
   - Handle agent failures gracefully
   - Implement retry logic with exponential backoff

2. **Agent Registry Service**
   - Dynamic agent registration
   - Health checks for agents
   - Load balancing between agent instances

3. **Streaming Responses**
   - Implement streaming A2A responses
   - Show real-time progress in coordinator

4. **Authentication & Authorization**
   - Secure agent-to-agent communication
   - API key management
   - Role-based access control

## Tips

- Start with one agent and get A2A working before adding more
- Test each agent individually before building the coordinator
- Use Aspire dashboard to monitor agent communications
- Pay attention to A2A protocol compliance (JSON-RPC 2.0 format)
- Keep agent responsibilities focused and specific
- Log all A2A interactions for debugging

## Resources

- [A2A Protocol Specification](https://google.github.io/A2A/)
- [Notebook 6: A2A Protocol](../../notebooks/6-Agent-to-Agent-Protocol.ipynb)
- [SharpA2A.Core Package](https://www.nuget.org/packages/SharpA2A.Core)
- [SK A2A Demo](https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Demos/A2AClientServer)

## Success Criteria

- ✅ All three agents expose valid agent cards
- ✅ Agents respond to A2A protocol messages correctly
- ✅ Coordinator can discover and communicate with all agents
- ✅ Multi-agent workflow executes successfully
- ✅ All services run together in Aspire
- ✅ Proper error handling and logging implemented

This assignment demonstrates modern microservices architecture applied to AI agents, a crucial skill for building scalable, production-ready AI systems.