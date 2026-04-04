# Creating AI Applications and Agents with C# - Semantic Kernel & Microsoft Agent Framework Workshop

A comprehensive workshop for building AI applications and agents using Microsoft's Semantic Kernel framework and the newly GA **Microsoft Agent Framework** in C#.

> **🆕 Updated for Microsoft Agent Framework GA (v1.0.0) and Microsoft.Extensions.AI GA**  
> See the [Microsoft Agent Framework GA Migration Guide](docs/MICROSOFT_AGENT_FRAMEWORK_GA.md) for what changed and how to adapt your code.

## 📚 Workshop Overview

This workshop teaches you how to create intelligent AI agents that can collaborate as a software development team. You'll learn to build an AI Coding Assistant capable of performing complex software development tasks through multi-agent orchestration.

The workshop content is based on **Semantic Kernel** — which is now a core component of the unified **Microsoft Agent Framework** alongside AutoGen. Everything you learn here applies directly to the GA framework.

## 🎯 Learning Path

### ⚠️ Notebook Environment Notice (Polyglot Notebooks Deprecation)

The interactive notebooks (`.ipynb`) in this workshop use the **C# kernel** (previously known as Polyglot Notebooks / .NET Interactive). **Microsoft deprecated Polyglot Notebooks in February 2026** — no new features or bug fixes will be shipped for the VS Code extension.

**Recommended alternatives for running C# notebooks:**
- **[Verso](https://github.com/DataficationSDK/Verso)** — open-source drop-in replacement that imports `.ipynb` files and supports the .NET kernel (best option for this workshop)
- **[.NET Script files](https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-options/top-level-statements)** — run the C# snippets from each notebook as standalone `.csx` scripts
- **Jupyter with the C# kernel** — if you already have Jupyter installed, install the `dotnet-interactive` Jupyter kernel while it still works

> If you're running the workshop and notebooks don't execute, switch to Verso or the `.csx` scripts in `notebooks/scripts/` (coming soon).

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

## 🤖 Microsoft Agent Framework GA

Microsoft Agent Framework v1.0.0 (GA, April 2026) is the **unified AI agent platform** for .NET that merges:
- **Semantic Kernel** — enterprise agents, plugins, memory, telemetry
- **AutoGen** — multi-agent orchestration from Microsoft Research

Key GA capabilities relevant to this workshop:

| Feature | Package | Status |
|---------|---------|--------|
| `IChatClient` / `IEmbeddingGenerator` abstractions | `Microsoft.Extensions.AI` | ✅ **GA** |
| Semantic Kernel agents & plugins | `Microsoft.SemanticKernel` (v1.74+) | ✅ **GA** |
| Process Framework (stateful workflows) | `Microsoft.SemanticKernel.Process.Core` | ✅ **GA** |
| A2A Protocol support | Built into SK Agents | ✅ **GA** |
| MCP tool integration | `Microsoft.SemanticKernel.Mcp` | ✅ **GA** |
| Graph-based multi-agent orchestration | `Microsoft.Extensions.AI` pipeline | ✅ **GA** |

**Key links:**
- 📖 [Microsoft Agent Framework blog](https://devblogs.microsoft.com/agent-framework/)
- 🐙 [GitHub: microsoft/agent-framework](https://github.com/microsoft/agent-framework)
- 📦 [NuGet: Microsoft.SemanticKernel](https://www.nuget.org/packages/Microsoft.SemanticKernel/) (v1.74+)
- 📦 [NuGet: Microsoft.Extensions.AI](https://www.nuget.org/packages/Microsoft.Extensions.AI/)
- 🔄 [Migration guide from preview → GA](docs/MICROSOFT_AGENT_FRAMEWORK_GA.md)

## 📦 Current Package Versions

This workshop targets the following GA package versions (as of mid-2026):

```xml
<PackageReference Include="Microsoft.SemanticKernel" Version="1.74.0" />
<PackageReference Include="Microsoft.Extensions.AI" Version="9.7.1" />
<PackageReference Include="Microsoft.Extensions.AI.Abstractions" Version="9.10.0" />
<PackageReference Include="Azure.AI.OpenAI" Version="2.5.0" />
```

> **Note:** Some notebooks may reference older preview versions (e.g., `Microsoft.Extensions.AI.OpenAI 9.7.1-preview.*`). Update to GA versions when running locally. See the [migration guide](docs/MICROSOFT_AGENT_FRAMEWORK_GA.md) for details.



## 📖 Additional Resources

- **[Microsoft Agent Framework GA Guide](docs/MICROSOFT_AGENT_FRAMEWORK_GA.md)** - What changed from preview to GA and how to migrate
- **[GitHub Models Setup Guide](docs/Using_GitHub_Models.md)** - Configure GitHub marketplace models
- **[Notebook Learning Materials](notebooks/)** - Interactive Jupyter notebooks
- **[Assignment Solutions](docs/assignments/)** - Guided exercises
- **[Microsoft Agent Framework blog](https://devblogs.microsoft.com/agent-framework/)** - Official announcements and deep-dives
- **[dotnet/extensions on GitHub](https://github.com/dotnet/extensions)** - Source for `Microsoft.Extensions.AI`

