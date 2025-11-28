# Reference Materials Summary

## Documentation Sources Reviewed

### 1. Aspire 13.0 Documentation
**Location**: `C:\Users\tamirdresher\source\repos\docs-aspire`

**Key Findings:**
- ✅ Aspire 13.0 exists and is documented
- Reference: https://aspire.dev/whats-new/aspire-13/
- Breaking changes documentation available at: `docs/compatibility/breaking-changes.md`
- Latest versions: 13.0, 9.5, 9.4.2, 9.4.1, 9.4, 9.3 (current)

**Upgrade Path:**
- From: Aspire 9.3.1
- To: Aspire 13.0
- Need to review: Breaking changes between 9.3.1 and 13.0

---

### 2. Semantic Kernel Main Repository
**Location**: `C:\Users\tamirdresher\source\repos\semantic-kernel-1`

**Key Features Documented:**
- ✅ Agent Framework with multi-agent systems
- ✅ Process Framework (New feature to add to course)
- ✅ A2A Protocol (Agent-to-Agent communication)
- ✅ Plugin ecosystem and MCP support
- ✅ Multi-modal support (text, vision, audio)

**System Requirements:**
- .NET: 8.0+ (Project uses .NET 9)
- Python: 3.10+
- Java: JDK 17+

---

### 3. A2A Protocol (Agent-to-Agent Communication)
**Location**: `C:\Users\tamirdresher\source\repos\semantic-kernel-1\dotnet\samples\Demos\A2AClientServer`

**Protocol Specification**: https://google.github.io/A2A/

**Key Concepts:**
- A2A Server: Makes agents available via A2A protocol
- A2A Client: Invokes remote agents using A2A protocol
- Agent Card: `.well-known/agent.json` endpoint
- Message Protocol: JSON-RPC based communication
- SharpA2A.Core: NuGet package for A2A support

**Architecture Pattern:**
```
┌─────────────┐         A2A Protocol         ┌──────────────────┐
│  A2A Client │ ───────────────────────────> │  A2A Server 1    │
│             │                               │  (Invoice Agent) │
│             │ ───────────────────────────> │──────────────────┤
│             │                               │  A2A Server 2    │
│             │                               │  (Policy Agent)  │
│             │ ───────────────────────────> │──────────────────┤
│             │                               │  A2A Server 3    │
└─────────────┘                               │ (Logistics Agent)│
                                              └──────────────────┘
```

**Use Cases Demonstrated:**
- Customer service with specialized agents (Invoice, Policy, Logistics)
- Remote agent invocation
- Inter-agent communication
- Distributed agent architecture

**Important Notes:**
- ⚠️ A2A protocol is still under development and changing fast
- Uses JSON-RPC 2.0 for message exchange
- Supports both HTTP and HTTPS
- Can be tested with A2A Inspector tool

**Sample Implementation Files:**
- Server: `A2AServer/Program.cs`
- Client: `A2AClient/Program.cs`
- Testing: `A2AServer/A2AServer.http`

---

### 4. Process Framework
**Location**: `C:\Users\tamirdresher\source\repos\semantic-kernel-1\dotnet\samples\Demos\ProcessFrameworkWithAspire`

**Overview:**
The Semantic Kernel Process Framework enables creation of business processes based on events, where each process step may invoke an agent or execute native code.

**Key Features:**
- Event-driven architecture
- Process steps can invoke agents
- Process steps can execute native code
- Integration with .NET Aspire for tracing
- OpenTelemetry support for observability

**Architecture:**
```
┌──────────────────┐
│ Process          │
│ Orchestrator     │
└────────┬─────────┘
         │
         ├─────────> Translation Agent (HTTP)
         │           (Translates text)
         │
         └─────────> Summarization Agent (HTTP)
                     (Summarizes translated text)
```

**Integration with Aspire:**
- Agents defined as external services
- Each process step issues HTTP requests to agents
- .NET Aspire traces process using OpenTelemetry
- Agents can be restarted independently via Aspire dashboard

**Key Benefits:**
- Observable: Full tracing of process execution
- Scalable: Agents as independent services
- Flexible: Event-driven process flow
- Testable: Each agent can be tested independently

**Sample Use Case:**
- Translation and summarization workflow
- Multi-step process with agent coordination
- Metrics and tracing via Aspire dashboard

---

## New Content to Add to Course

### 1. Notebook 6: Agent-to-Agent (A2A) Protocol

**Content Structure:**
```markdown
# Agent-to-Agent (A2A) Protocol

## Introduction
- What is A2A protocol
- Why inter-agent communication matters
- Google A2A specification

## Core Concepts
- A2A Server architecture
- A2A Client implementation
- Agent Card (metadata)
- JSON-RPC message protocol

## Implementation
1. Creating an A2A Server
2. Creating an A2A Client  
3. Agent discovery
4. Message exchange patterns

## Practical Examples
- Customer Service Scenario (Invoice, Policy, Logistics agents)
- Testing with A2A Inspector
- Testing with REST client

## Best Practices
- Error handling
- Security considerations
- Protocol versioning
- Agent metadata management
```

**Key Code Examples:**
- A2A Server setup
- A2A Client connection
- Agent registration
- Message sending/receiving

---

### 2. Notebook 7: Process Framework and Human-in-the-Loop

**Content Structure:**
```markdown
# Semantic Kernel Process Framework

## Part 1: Process Framework Basics
- What is the Process Framework
- Event-driven processes
- Process steps
- Agent invocation from processes

## Part 2: Integration with Aspire
- Agents as external services
- OpenTelemetry tracing
- Metrics and observability
- Dashboard monitoring

## Part 3: Human-in-the-Loop (HITL)
- Why HITL is important
- Approval gates
- Human decision points
- Timeout handling

## Practical Example
- Translation and Summarization workflow
- Process orchestration
- Error handling
- Testing and debugging
```

---

### 3. Enhanced Orchestration Patterns (Notebook 4)

**Additional Patterns to Add:**

#### Aggregator Pattern
- Multiple agents gather data
- Central aggregator combines results
- Example: Market research from multiple sources

#### Reducer Pattern
- Multiple agents provide input
- Reducer consolidates and filters
- Example: Code review feedback consolidation

#### Scatter-Gather Pattern
- Distribute work to multiple agents
- Gather and combine results
- Example: Travel planning with multiple providers

#### Pipeline Pattern
- Sequential transformation
- Each agent processes and passes to next
- Example: Content creation pipeline

---

## Version Information Needed

### Semantic Kernel Versions
From SK README.md, we see references to:
- Latest NuGet badge shows preview versions
- Supports .NET 8.0+
- System requirements indicate .NET 8.0+ support

**Action Required:**
Need to determine exact latest SK version for:
- Microsoft.SemanticKernel
- Microsoft.SemanticKernel.Agents.Core
- Microsoft.SemanticKernel.Agents.Orchestration
- Microsoft.SemanticKernel.Agents.Runtime.InProcess

**Checking Method:**
1. Visit NuGet.org
2. Search for "Microsoft.SemanticKernel"
3. Note latest stable version
4. Check for preview versions if needed

---

## Implementation Roadmap

### Phase 1: Version Updates ✅ Ready
- Aspire 9.3.1 → 13.0 (Confirmed available)
- Semantic Kernel 1.61.0 → Latest (TBD)
- Update all related packages

### Phase 2: A2A Protocol Content ✅ Ready
- Reference materials available
- Sample code exists
- Implementation patterns clear

### Phase 3: Process Framework Content ✅ Ready
- Documentation reviewed
- Aspire integration understood
- Sample architecture clear

### Phase 4: Orchestration Enhancements ✅ Ready
- Pattern concepts identified
- Example scenarios defined
- Teaching approach clear

---

## Key Takeaways

### ✅ Confirmed:
1. Aspire 13.0 exists and is documented
2. A2A protocol samples available with SharpA2A.Core
3. Process Framework well-documented with Aspire integration
4. Multi-agent patterns are production-ready

### ⚠️ Important Notes:
1. A2A protocol is still evolving (per warning in samples)
2. Some SK packages are preview versions
3. Need to verify exact target SK versions
4. Breaking changes between Aspire 9.3.1 and 13.0 need review

### 📋 Next Actions:
1. Determine latest stable SK version numbers
2. Review Aspire 13.0 breaking changes
3. Begin Phase 1: Version updates
4. Test build compatibility

---

*Last Updated: November 13, 2025*
*Status: Reference materials reviewed and documented*