# Semantic Kernel Workshop Notebooks

This directory contains interactive Polyglot Notebooks that guide you through learning Semantic Kernel concepts and building AI agents with C#.

## ⚠️ Notebook Status (Updated April 2025)

**Polyglot Notebooks are still the recommended format for interactive C# exploration**, but here's the current landscape:

| Approach | Status | Recommended for |
|----------|--------|-----------------|
| **Polyglot Notebooks** (`.ipynb` + VS Code extension) | ✅ Active | Quick exploration, teaching, demos |
| **C# console/service projects** | ✅ Preferred for production | Anything you'd deploy or test seriously |
| *.NET Interactive (legacy CLI)* | ⚠️ Superseded by Polyglot Notebooks | — |

**Bottom line**: Notebooks here use the [Polyglot Notebooks VS Code extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode). They are **not deprecated** — Microsoft continues to maintain Polyglot Notebooks. However, for the new **Microsoft Agent Framework** samples (GA with .NET 10), the official Microsoft repo uses console apps rather than notebooks. For serious workshop projects, graduating from notebooks to the [SKCodeAssistent project](../src/SKCodeAssistent/) is recommended.

---

## 📚 Learning Sequence

Work through these notebooks in order to build your understanding progressively:

### 1. Foundation
- **[0-AI-settings.ipynb](0-AI-settings.ipynb)** - Configure your AI model settings
- **[1-SemanticKernel-Intro.ipynb](1-SemanticKernel-Intro.ipynb)** - Basic Semantic Kernel concepts

### 2. Agent Development
- **[2-SemanticKernel-Agents.ipynb](2-SemanticKernel-Agents.ipynb)** - Creating and managing AI agents
- **[3-Functions-Plugins.ipynb](3-Functions-Plugins.ipynb)** - Extending agents with functions and plugins
- **[3.1-OpenAPIPlugin.ipynb](3.1-OpenAPIPlugin.ipynb)** - Integrating with external APIs
- **[3.2-MCP.ipynb](3.2-MCP.ipynb)** - Connecting agents to external systems using Model Context Protocol (MCP 1.1.0 stable)

### 3. Advanced Orchestration
- **[4-MultiAgent-Orchestration.ipynb](4-MultiAgent-Orchestration.ipynb)** - Coordinating multiple agents
- **[5-ChatHistoryReducers.ipynb](5-ChatHistoryReducers.ipynb)** - Managing conversation context
- **[6-Agent-to-Agent-Protocol.ipynb](6-Agent-to-Agent-Protocol.ipynb)** - Standardized agent communication (A2A 0.3.4)
- **[7-Process-Framework-and-HITL.ipynb](7-Process-Framework-and-HITL.ipynb)** - Event-driven workflows with human oversight
- **[8-Guardrails-and-AI-Safety.ipynb](8-Guardrails-and-AI-Safety.ipynb)** - Control mechanisms and safety filters

## 🛠️ Setup Requirements

### Prerequisites
- **.NET 10 SDK** (updated from .NET 9 — required for `Microsoft.Extensions.AI` 10.x and MS Agent Framework)
- **VS Code** with [Polyglot Notebooks extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)
- AI model access (Azure OpenAI / GitHub Models / local LLM)

> **Note:** The notebooks use the `.NET Interactive` kernel inside Polyglot Notebooks. After installing the VS Code extension, open any `.ipynb` file and select **.NET Interactive** as the kernel.

### Configuration
1. **AI Settings**: Start with [`0-AI-settings.ipynb`](0-AI-settings.ipynb) to configure your model
2. **Dependencies**: Each notebook installs required NuGet packages automatically via `#r "nuget:..."` directives
3. **Settings File**: Update [`config/settings.json`](config/settings.json) with your API keys

## 📁 Supporting Files

- **[`config/`](config/)** - Configuration files and utilities
  - [`Settings.cs`](config/Settings.cs) - Configuration model
  - [`settings.json`](config/settings.json) - API keys and endpoints
  - [`Utils.cs`](config/Utils.cs) - Helper utilities
  - [`SkiaUtils.cs`](config/SkiaUtils.cs) - Graphics utilities

- **[`PromptPlugins/`](PromptPlugins/)** - Pre-built prompt plugins
  - [`ClassificationPlugin/`](PromptPlugins/ClassificationPlugin/) - Text classification prompts

- **[`img/`](img/)** - Images and screenshots used in notebooks

## 🎯 Learning Objectives

By completing these notebooks, you will:

1. **Understand Semantic Kernel fundamentals** - Kernels, services, and basic operations
2. **Build AI agents** - Create specialized agents with distinct roles and capabilities
3. **Implement function calling** - Extend agents with custom functions and external plugins
4. **Master multi-agent orchestration** - Coordinate teams of agents working together
5. **Manage conversation context** - Handle chat history and context reduction strategies
6. **Use MCP (stable 1.1.0)** - Connect agents to external tools via Model Context Protocol
7. **Implement A2A** - Cross-agent communication using the Agent-to-Agent protocol

## 💡 Tips for Success

- **Run notebooks sequentially** - Each builds upon concepts from previous ones
- **Experiment with examples** - Modify code samples to see how changes affect behavior
- **Check configuration** - Ensure your AI model settings are correct before starting
- **Review outputs** - Pay attention to agent responses and conversation flows
- **Graduate to projects** - After notebooks, explore [`../src/SKCodeAssistent/`](../src/SKCodeAssistent/) for production patterns

## 🔗 Related Resources

- **[Microsoft Agent Framework (GA)](https://github.com/microsoft/agent-framework)** - The new unified .NET 10 agent framework
- **[Main Workshop Documentation](../docs/README.md)** - Complete project setup and architecture
- **[Assignments](../docs/assignments/)** - Hands-on exercises to practice concepts
- **[SKCodeAssistent Project](../src/SKCodeAssistent/)** - Full implementation example
