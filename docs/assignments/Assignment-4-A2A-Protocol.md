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

Implement **two specialized A2A agent services**. Each agent should be a separate ASP.NET Core project:

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