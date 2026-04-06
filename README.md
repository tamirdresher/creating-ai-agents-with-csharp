# Creating AI Applications and Agents with C# - Semantic Kernel & Microsoft Agent Framework Workshop

A comprehensive workshop for building AI applications and agents using Microsoft's Semantic Kernel framework and the new **Microsoft Agent Framework** in C#.

## 🚀 What's New — Microsoft Agent Framework GA (.NET 10)

> **April 2025**: [Microsoft Agent Framework has reached General Availability](https://github.com/microsoft/agent-framework) alongside **.NET 10**!  
> This is a major milestone: a unified, multi-language (C# + Python) framework for building, orchestrating, and deploying AI agents.

### Key GA Highlights
| Feature | Detail |
|---------|--------|
| **Package** | `dotnet add package Microsoft.Agents.AI` |
| **GitHub** | [github.com/microsoft/agent-framework](https://github.com/microsoft/agent-framework) |
| **Docs** | [learn.microsoft.com/agent-framework](https://learn.microsoft.com/en-us/agent-framework/) |
| **.NET 10** | Framework requires .NET 10 SDK (this repo updated from .NET 9) |
| **Providers** | OpenAI, Azure OpenAI (Foundry), Anthropic, Ollama, Google Gemini |
| **Graph Workflows** | Streaming, checkpointing, HITL, time-travel |
| **MCP** | `ModelContextProtocol` 1.1.0 — now **stable** (was 0.5.0-preview) |
| **Extensions.AI** | `Microsoft.Extensions.AI` 10.4.0 — GA in .NET 10 (was 9.x preview) |
| **A2A** | A2A protocol 0.3.4-preview |

#### Quick Hello Agent (new MS Agent Framework style)
```csharp
// dotnet add package Microsoft.Agents.AI.OpenAI
using Microsoft.Agents.AI;
using OpenAI;

var agent = new OpenAIClient("<apikey>")
    .GetResponsesClient("gpt-4o-mini")
    .AsAIAgent(name: "HaikuBot", instructions: "You are an upbeat assistant.");

Console.WriteLine(await agent.RunAsync("Write a haiku about AI agents."));
```

### Relationship: Semantic Kernel vs Microsoft Agent Framework
- **Semantic Kernel** (what most notebooks use) — stays GA, rich plugin/function ecosystem, orchestration agents. Still the recommended path for complex enterprise agents.
- **Microsoft Agent Framework** — new unified surface on top of `Microsoft.Extensions.AI`. Simpler API, multi-language, better for quick agent creation and graph-based workflows. Internally still uses SK for vector stores and plugins.
- Both complement each other. Workshop now covers both.

---

## 📚 Workshop Overview

This workshop teaches you how to create intelligent AI agents that can collaborate as a software development team. You'll learn to build an AI Coding Assistant capable of performing complex software development tasks through multi-agent orchestration.

## 🎯 Learning Path

### 📓 Interactive Notebooks
Hands-on learning materials covering core concepts:

> **Note on Notebooks**: These use [Polyglot Notebooks](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode) (VS Code extension). Install the extension and select **.NET Interactive** kernel. See [notebooks/README.md](notebooks/README.md) for full setup and current status.

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

### 📝 Assignments
Progressive exercises to build your skills:

- **[Assignment 1: Three Agents](docs/assignments/Assignment-1-Three-Agents.md)** - Create your first agent team
- **[Assignment 2: Plugins and MCP](docs/assignments/Assignment-2-Plugins-and-MCP.md)** - Extend agents with external capabilities
- **[Assignment 3: Team Orchestration](docs/assignments/Assignment-3-Team-Orchestration.md)** - Advanced multi-agent coordination
- **[Assignment 4: A2A Protocol](docs/assignments/Assignment-4-A2A-Protocol.md)** - 🆕 Implement agent-to-agent communication
- **[Assignment 5: Process Framework](docs/assignments/Assignment-5-Process-Framework.md)** - 🆕 Build event-driven workflows with HITL

## 🚀 Quick Start

1. **Prerequisites**: **.NET 10 SDK**, Node.js 18+, AI model access
2. **Configuration**: Set up your AI model credentials
3. **Run**: Start the Aspire Host project to launch the backend
4. **Explore**: Work through the notebooks and assignments

👉 **[See complete setup instructions](docs/README.md#-quick-start)**

## 📦 Package Versions (Updated for .NET 10 GA)

| Package | Version | Notes |
|---------|---------|-------|
| `Microsoft.SemanticKernel` | 1.68.0 | Latest stable |
| `Microsoft.Agents.AI` | 1.0.0 | 🆕 MS Agent Framework GA |
| `Microsoft.Extensions.AI` | 10.4.0 | 🆕 GA with .NET 10 |
| `ModelContextProtocol` | 1.1.0 | 🆕 Stable release |
| `A2A` / `A2A.AspNetCore` | 0.3.4-preview | Updated |
| `Microsoft.AspNetCore.OpenApi` | 10.0.0 | 🆕 GA (was 9-preview) |
| `Azure.AI.OpenAI` | 2.9.0-beta.1 | Updated |
| `Azure.AI.Projects` | 2.0.0 | 🆕 Foundry GA |

## 📖 Additional Resources

- **[Microsoft Agent Framework GitHub](https://github.com/microsoft/agent-framework)** - 🆕 GA source & samples
- **[MS Agent Framework Docs](https://learn.microsoft.com/en-us/agent-framework/)** - Official documentation
- **[Migration from SK](https://learn.microsoft.com/en-us/agent-framework/migration-guide/from-semantic-kernel)** - Migrate from Semantic Kernel
- **[GitHub Models Setup Guide](docs/Using_GitHub_Models.md)** - Configure GitHub marketplace models
- **[Notebook Learning Materials](notebooks/)** - Interactive Polyglot Notebooks
- **[Assignment Solutions](docs/assignments/)** - Guided exercises
