# AI Agents Course Material Refresh Plan

## Overview
This document outlines the comprehensive plan to refresh the "Creating AI Applications and Agents with C#" workshop material with the latest versions and new content.

## Current State
- **Aspire Version**: 9.3.1
- **Semantic Kernel Version**: 1.61.0
- **Notebooks**: 5 existing notebooks (0-5)
- **Orchestration Patterns**: 4 patterns (Concurrent, Sequential, Group Chat, Custom)

## Target State
- **Aspire Version**: 13.x (latest stable)
- **Semantic Kernel Version**: Latest stable release
- **Notebooks**: 7+ notebooks (adding 2+ new ones)
- **New Topics**: A2A Protocol, Process Framework, HITL, Additional Orchestration Patterns

---

## Phase 1: Version Updates

### 1.1 Aspire Update (9.3.1 → 13.x)

**Affected Files:**
- `src/SKCodeAssistent/Directory.Packages.props`
- `src/SKCodeAssistent/SKCodeAssistent.AppHost/SKCodeAssistent.AppHost.csproj`
- `src/SKCodeAssistent/SKCodeAssistent.ServiceDefaults/SKCodeAssistent.ServiceDefaults.csproj`

**Tasks:**
1. Review Aspire 13 release notes and migration guide
2. Identify breaking changes between 9.3.1 and 13.x
3. Update package versions:
   - `Aspire.Hosting.AppHost`
   - `Microsoft.Extensions.ServiceDiscovery`
   - Related Aspire packages
4. Update configuration files if needed
5. Test build and runtime

**Potential Breaking Changes to Watch:**
- Configuration schema changes
- Service discovery API updates
- Telemetry/OpenTelemetry updates
- Dashboard changes

### 1.2 Semantic Kernel Update (1.61.0 → Latest)

**Affected Files:**
- `src/SKCodeAssistent/Directory.Packages.props`
- All notebook files (*.ipynb)

**Packages to Update:**
- `Microsoft.SemanticKernel`
- `Microsoft.SemanticKernel.Agents.Core`
- `Microsoft.SemanticKernel.Agents.Magentic`
- `Microsoft.SemanticKernel.Agents.Orchestration`
- `Microsoft.SemanticKernel.Agents.Runtime.InProcess`
- `Microsoft.SemanticKernel.Connectors.AzureOpenAI`
- `Microsoft.SemanticKernel.Connectors.OpenAI`
- `Microsoft.SemanticKernel.Plugins.OpenApi.Extensions`

**Notebooks to Update:**
1. `notebooks/1-SemanticKernel-Intro.ipynb` - Lines 156, 312
2. `notebooks/2-SemanticKernel-Agents.ipynb` - Lines 54, 99
3. `notebooks/3-Functions-Plugins-MCP.ipynb` - Lines 26-27
4. `notebooks/3.1-OpenAPIPlugin.ipynb` - Lines 59, 172
5. `notebooks/4-MultiAgent-Orchestration.ipynb` - Lines 45, 108-110
6. `notebooks/5-ChatHistoryReducers.ipynb` - Lines 45, 90

**API Changes to Review:**
- Agent API updates
- Orchestration framework changes
- Function calling behavior updates
- Plugin system changes

---

## Phase 2: New Content - Agent to Agent (A2A) Protocol

### 2.1 New Notebook: 6-Agent-to-Agent-Protocol.ipynb

**Content Structure:**

```markdown
# Agent to Agent (A2A) Protocol

## Introduction
- What is A2A protocol
- Why inter-agent communication matters
- Use cases and scenarios

## Core Concepts
- Message passing between agents
- Agent discovery and registration
- Protocol contracts and interfaces
- Event-driven communication

## Implementation Patterns
1. Direct Agent Communication
2. Message Bus Pattern
3. Request-Response Pattern
4. Publish-Subscribe Pattern

## Practical Examples
### Example 1: Research Team with A2A
- Researcher agent finds information
- Analyst agent processes data
- Reporter agent synthesizes findings

### Example 2: DevOps Pipeline
- Developer agent writes code
- Reviewer agent validates
- Deployer agent publishes

## Best Practices
- Error handling in A2A
- Timeout management
- Message serialization
- Agent state management

## Integration with Orchestration
- Combining A2A with orchestration patterns
- When to use A2A vs orchestration
```

### 2.2 C# Solution Integration
- Add A2A examples to SCHOOL_SOLUTIONS
- Create reusable A2A communication classes
- Add to VS Code extension if applicable

---

## Phase 3: Enhanced Orchestration Patterns

### 3.1 Expand Notebook 4: Additional Patterns

**Current Patterns:**
1. ✅ Concurrent Orchestration
2. ✅ Sequential Orchestration
3. ✅ Group Chat Orchestration
4. ✅ Custom Orchestration

**New Patterns to Add:**

#### Pattern 5: Aggregator Pattern
```markdown
### Scenario: Market Research Team
- Multiple agents gather data from different sources
- Aggregator collects and combines results
- Produces unified market analysis
```

#### Pattern 6: Reducer Pattern
```markdown
### Scenario: Code Review Pipeline
- Multiple reviewers provide feedback
- Reducer consolidates feedback
- Eliminates duplicates and conflicts
- Produces actionable review summary
```

#### Pattern 7: Scatter-Gather Pattern
```markdown
### Scenario: Travel Planning
- Scatter: Query multiple travel agents for options
- Process: Each agent finds flights/hotels/activities
- Gather: Combine best options
- Present: Unified travel itinerary
```

#### Pattern 8: Pipeline Pattern
```markdown
### Scenario: Content Creation Pipeline
- Input → Outliner → Writer → Editor → Proofreader → Output
- Each stage transforms content
- Linear flow with validation gates
```

### 3.2 Comparison Matrix

Add section comparing all patterns:

| Pattern | Execution | Best For | Complexity |
|---------|-----------|----------|------------|
| Concurrent | Parallel | Independent tasks | Low |
| Sequential | Linear | Dependent steps | Low |
| Group Chat | Collaborative | Discussion/debate | Medium |
| Custom | Flexible | Complex logic | High |
| Aggregator | Parallel + Merge | Data collection | Medium |
| Reducer | Collect + Consolidate | Feedback/analysis | Medium |
| Scatter-Gather | Distribute + Collect | Search/options | Medium |
| Pipeline | Sequential + Transform | Multi-stage processing | Medium |

---

## Phase 4: Process Framework and Human-in-the-Loop

### 4.1 New Notebook: 7-Process-Framework-and-HITL.ipynb

**Content Structure:**

```markdown
# Semantic Kernel Process Framework and Human-in-the-Loop

## Part 1: Process Framework

### Introduction
- What is the SK Process Framework
- Stateful vs Stateless processes
- Process lifecycle management

### Core Components
1. Process Definition
2. Steps and Transitions
3. State Management
4. Event Handling

### Building Your First Process
```csharp
// Define process steps
// Set up transitions
// Execute process
// Monitor state
```

### Advanced Process Features
- Conditional branching
- Parallel steps
- Error handling and compensation
- Process persistence

## Part 2: Human-in-the-Loop (HITL)

### Why HITL?
- AI limitations and human oversight
- Critical decision points
- Quality assurance
- Regulatory compliance

### HITL Patterns

#### Pattern 1: Approval Gates
- AI proposes action
- Human approves/rejects
- System executes or revises

#### Pattern 2: Collaborative Decision Making
- AI provides recommendations
- Human adds context
- Combined decision

#### Pattern 3: Review and Refine
- AI generates output
- Human reviews and edits
- AI learns from feedback

#### Pattern 4: Exception Handling
- AI handles routine cases
- Escalates complex cases to human
- Human handles edge cases

### Implementing HITL in SK Process Framework

```csharp
// Process with approval step
// Human input integration
// Timeout and fallback handling
// Audit trail
```

## Part 3: Real-World Example

### Scenario: Expense Approval System
1. Employee submits expense
2. AI validates receipt and policy
3. AI calculates risk score
4. **HITL**: If high-risk, manager approval required
5. AI processes approved expense
6. **HITL**: Periodic human audit of decisions

### Implementation
- Complete working example
- Process definition
- HITL integration points
- State management
- Error handling

## Best Practices
- When to involve humans
- Designing approval workflows
- Handling timeouts
- Audit and compliance
- Testing HITL processes
```

### 4.2 C# Solution Integration
- Add Process Framework examples
- Create HITL workflow templates
- Add to orchestration options

---

## Phase 5: Documentation Updates

### 5.1 README.md Updates

**Add sections for:**
- New notebooks 6 and 7
- Updated version requirements
- New topics overview

```markdown
### 📓 Interactive Notebooks
...existing notebooks...
- **[Agent-to-Agent Protocol](notebooks/6-Agent-to-Agent-Protocol.ipynb)** - Inter-agent communication
- **[Process Framework & HITL](notebooks/7-Process-Framework-and-HITL.ipynb)** - Stateful processes and human oversight
```

### 5.2 docs/README.md Updates

**Update Prerequisites:**
```markdown
### Prerequisites
- .NET 9 SDK
- Node.js 18+
- **Aspire 13.x** (updated from 9.3.1)
- AI Model - Served from Azure OpenAI or GitHub marketplace
```

**Add new features:**
```markdown
## 🚀 New in This Version
- ✨ Updated to Aspire 13
- ✨ Latest Semantic Kernel with enhanced agents
- ✨ Agent-to-Agent (A2A) communication protocol
- ✨ Additional orchestration patterns (8 total)
- ✨ Process Framework for stateful workflows
- ✨ Human-in-the-Loop patterns
```

### 5.3 New Assignments

#### Assignment 4: Agent-to-Agent Communication
```markdown
# Assignment 4: Implement A2A Protocol

## Objective
Build a multi-agent system where agents communicate directly using A2A protocol.

## Tasks
1. Create 3 agents with distinct roles
2. Implement direct agent-to-agent messaging
3. Add event-driven communication
4. Handle communication failures

## Deliverables
- Working A2A implementation
- Communication flow diagram
- Error handling demonstration
```

#### Assignment 5: Process Framework with HITL
```markdown
# Assignment 5: Build HITL Approval Process

## Objective
Create a process with human approval gates.

## Tasks
1. Define a multi-step process
2. Add approval gate requiring human input
3. Implement timeout handling
4. Add audit logging

## Deliverables
- Process definition
- HITL integration
- Test scenarios
```

---

## Phase 6: Testing and Validation

### 6.1 Notebook Testing
- [ ] All notebooks run without errors
- [ ] Package versions are consistent
- [ ] Code examples are up-to-date
- [ ] Output is as expected

### 6.2 Solution Testing
- [ ] Solution builds successfully
- [ ] All projects run
- [ ] Aspire dashboard works
- [ ] VS Code extension compatible

### 6.3 Workshop Flow Testing
- [ ] Prerequisites are clear
- [ ] Setup instructions work
- [ ] Learning path is logical
- [ ] Assignments are achievable

---

## Implementation Timeline

### Week 1: Version Updates
- Days 1-2: Research Aspire 13 and SK updates
- Days 3-4: Update packages and test builds
- Day 5: Fix breaking changes and validate

### Week 2: A2A Protocol
- Days 1-2: Create notebook 6
- Days 3-4: Add C# solution examples
- Day 5: Test and refine

### Week 3: Enhanced Orchestration
- Days 1-3: Add new orchestration patterns
- Days 4-5: Test and document

### Week 4: Process Framework & HITL
- Days 1-3: Create notebook 7
- Days 4-5: Add C# examples and test

### Week 5: Documentation & Assignments
- Days 1-2: Update all documentation
- Days 3-4: Create new assignments
- Day 5: Final testing and validation

---

## Risk Mitigation

### Potential Risks:
1. **Breaking changes in Aspire 13**: Mitigation - Keep 9.3.1 fallback branch
2. **SK API changes**: Mitigation - Test early, document changes
3. **Notebook compatibility**: Mitigation - Version-specific examples
4. **Time constraints**: Mitigation - Prioritize critical updates first

### Rollback Plan:
- Maintain separate branches for each phase
- Test incrementally
- Have version-pinned fallback configurations

---

## Success Criteria

- ✅ All packages updated to latest versions
- ✅ Zero build errors
- ✅ All notebooks execute successfully
- ✅ 2 new notebooks added (A2A, Process Framework)
- ✅ 4 new orchestration patterns documented
- ✅ 2 new assignments created
- ✅ Documentation fully updated
- ✅ Workshop flow validated end-to-end

---

## Resources Needed

### Documentation:
- Aspire 13 release notes and migration guide
- Latest Semantic Kernel documentation
- A2A protocol specifications
- Process Framework documentation

### Reference Materials:
- `C:\Users\tamirdresher\source\repos\docs-aspire` - Aspire docs
- SK GitHub repository
- Community examples and patterns

### Tools:
- .NET 9 SDK
- VS Code with Polyglot Notebooks
- Visual Studio 2022
- Git for version control

---

## Next Steps

1. **Get approval** on this plan
2. **Set up tracking** for todo items
3. **Begin Phase 1** - Version updates
4. **Schedule reviews** at end of each phase
5. **Iterate based on feedback**

---

## Notes and Observations

### Current Strengths:
- Well-structured notebook progression
- Clear examples with real-world scenarios
- Good separation of concerns in solution architecture

### Areas for Enhancement:
- More advanced orchestration patterns needed
- Process state management not covered
- Human oversight patterns missing
- Direct agent communication not demonstrated

### Opportunities:
- Leverage new SK Process Framework
- Demonstrate more enterprise patterns
- Add production-ready examples
- Include monitoring and observability patterns

---

*Last Updated: November 13, 2025*
*Status: Planning Phase - Awaiting Approval*