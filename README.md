# Creating AI Applications and Agents with C# - Semantic Kernel C# Workshop

A comprehensive workshop for building AI applications and agents using Microsoft's Semantic Kernel framework in C#.

## 🆕 What's Changed — Microsoft Agent Framework (July 2025)

### The Big Picture: SK → MAF Evolution

Microsoft has unified **Semantic Kernel** and **AutoGen** under the **Microsoft Agent Framework (MAF)** umbrella — a single platform for building AI agents on .NET. Here is the key takeaway:

> **Semantic Kernel is the foundation of MAF.** Everything you learn in this workshop — kernels, plugins, agents, orchestration — is the stable base that MAF builds on. There is no "start over" moment.

```
  Microsoft Agent Framework (MAF)
  ├── AgentKernel · Declarative Agents · A2A protocol
  ├── Semantic Kernel 1.74.0 (this workshop)
  │   └── Kernel · Plugins · Agents API · Memory
  └── Microsoft.Extensions.AI 10.4.1
      └── IChatClient · Embeddings · Middleware
```

### What This Workshop Uses (Stable)

| Component | Version | Status |
|-----------|---------|--------|
| **Semantic Kernel** | 1.74.0 | ✅ GA — all notebooks and projects pinned here |
| **Microsoft.Extensions.AI** | 10.4.1 | ✅ GA — `IChatClient` abstraction (Notebook 1) |
| **MCP C# SDK** | 1.2.0 | ✅ GA — Model Context Protocol (Notebook 3.2) |
| **SK Agents API** | 1.74.0 (GA since 1.45.x) | ✅ GA — `ChatCompletionAgent`, `AgentGroupChat` |

### What's New — Console Samples Added

| Component | Version | Status |
|-----------|---------|--------|
| **MAF packages** (`Microsoft.Agents.AI.*`) | 1.0.0 | ✅ GA — console samples 01-08 use these |
| **MAF Workflows** (`Microsoft.Agents.AI.Workflows`) | 1.0.0 | ✅ GA — samples 04, 07 demonstrate workflows |
| **A2A Protocol** | Conceptual | 📋 Sample 05 demonstrates the pattern |
| **MCP C# SDK** (`ModelContextProtocol`) | 0.4.0-preview | ✅ Sample 03 demonstrates MCP integration |

### Breaking Changes on the Horizon

- `IChatClient.CompleteAsync` → `GetResponseAsync` (not yet, but planned)
- `ChatCompletion` → `ChatResponse`
- Planner packages (Handlebars, Stepwise) are deprecated → agent-driven orchestration replaces them

### When MAF Reaches GA

Workshop exercises will be updated to use MAF APIs. Your SK knowledge transfers directly — MAF adds concepts on top, it does not replace them. See the **[Microsoft Agent Framework Guide](docs/microsoft-agent-framework-guide.md)** for the full migration path.

> ⚠️ **Polyglot Notebooks Deprecation Notice**: Microsoft has announced that Polyglot Notebooks support ends March 2026. The interactive `.ipynb` notebooks in this workshop still work today, but console app alternatives for all exercises are planned. The `src/SKCodeAssistent` project already serves as the primary runnable application.

## 📚 Workshop Overview

This workshop teaches you how to create intelligent AI agents that can collaborate as a software development team. You'll learn to build an AI Coding Assistant capable of performing complex software development tasks through multi-agent orchestration.

## 🎯 Learning Path

### 📓 Interactive Notebooks
Hands-on learning materials covering core concepts:

- **[Introduction to Semantic Kernel](notebooks/1-SemanticKernel-Intro.ipynb)** - Get started with SK basics
- **[Semantic Kernel Agents](notebooks/2-SemanticKernel-Agents.ipynb)** - Building intelligent agents
- **[Functions & Plugins](notebooks/3-Functions-Plugins.ipynb)** - Extending agent capabilities
- **[OpenAPI Plugin](notebooks/3.1-OpenAPIPlugin.ipynb)** - Working with external APIs
- **[Model Context Protocol (MCP)](notebooks/3.2-MCP.ipynb)** - Connecting agents to external systems
- **[Multi-Agent Orchestration](notebooks/4-MultiAgent-Orchestration.ipynb)** - Coordinating multiple agents
- **[Chat History Reducers](notebooks/5-ChatHistoryReducers.ipynb)** - Managing conversation context
- **[Agent-to-Agent (A2A) Protocol](notebooks/6-Agent-to-Agent-Protocol.ipynb)** - 🆕 Standardized agent communication
- **[Process Framework & HITL](notebooks/7-Process-Framework-and-HITL.ipynb)** - 🆕 Event-driven workflows with human oversight
- **[Guardrails & AI Safety](notebooks/8-Guardrails-and-AI-Safety.ipynb)** - 🆕 Control mechanisms and safety filters

### 🏗️ SKCodeAssistent Project
A complete AI coding assistant implementation with:
- **Backend Service** - ASP.NET Core application hosting AI agents
- **VSCode Extension** - Integrated development environment support
- **Multi-Agent System** - Architect, Developer, and Tester agents working together

👉 **[View detailed project documentation](docs/README.md)**

### 🖥️ Console Samples (NEW — Microsoft Agent Framework GA)
Standalone console apps demonstrating MAF concepts — no notebooks required:

| # | Sample | Topic |
|---|--------|-------|
| 01 | [BasicAgent](src/samples/01-BasicAgent/) | Create an AIAgent, stream responses |
| 02 | [AgentWithTools](src/samples/02-AgentWithTools/) | Function tools with `AIFunctionFactory` |
| 03 | [MCPAgent](src/samples/03-MCPAgent/) | Model Context Protocol integration |
| 04 | [MultiAgentWorkflow](src/samples/04-MultiAgentWorkflow/) | Sequential agent pipeline with workflows |
| 05 | [A2AProtocol](src/samples/05-A2AProtocol/) | Agent-to-Agent communication |
| 06 | [HITL](src/samples/06-HITL/) | Human-in-the-Loop approval gates |
| 07 | [WorkflowFramework](src/samples/07-WorkflowFramework/) | Custom executors and workflow graphs |
| 08 | [Guardrails](src/samples/08-Guardrails/) | Input/output safety validation |

👉 **[View all console samples](src/samples/README.md)**

### 📝 Assignments
Progressive exercises to build your skills:

- **[Assignment 1: Three Agents](docs/assignments/Assignment-1-Three-Agents.md)** - Create your first agent team
- **[Assignment 2: Plugins and MCP](docs/assignments/Assignment-2-Plugins-and-MCP.md)** - Extend agents with external capabilities
- **[Assignment 3: Team Orchestration](docs/assignments/Assignment-3-Team-Orchestration.md)** - Advanced multi-agent coordination
- **[Assignment 4: A2A Protocol](docs/assignments/Assignment-4-A2A-Protocol.md)** - 🆕 Implement agent-to-agent communication
- **[Assignment 5: Process Framework](docs/assignments/Assignment-5-Process-Framework.md)** - 🆕 Build event-driven workflows with HITL

## 🚀 Quick Start

1. **Prerequisites**: .NET 9 SDK, Node.js 18+, AI model access
2. **Configuration**: Set up your AI model credentials
3. **Run**: Start the Aspire Host project to launch the backend
4. **Explore**: Work through the notebooks and assignments

👉 **[See complete setup instructions](docs/README.md#-quick-start)**

## 📖 Additional Resources

- **[Microsoft Agent Framework Guide](docs/microsoft-agent-framework-guide.md)** - 🆕 SK → MAF evolution, key concepts, migration path
- **[GitHub Models Setup Guide](docs/Using_GitHub_Models.md)** - Configure GitHub marketplace models
- **[Notebook Learning Materials](notebooks/)** - Interactive Jupyter notebooks
- **[Assignment Solutions](docs/assignments/)** - Guided exercises


