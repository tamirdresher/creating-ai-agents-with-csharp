# Semantic Kernel Workshop Notebooks

> ⚠️ **Polyglot Notebooks Deprecation Notice (February 2026)**
>
> Microsoft deprecated the Polyglot Notebooks VS Code extension in February 2026. The `.ipynb` C# notebooks in this directory **use the Polyglot/C# kernel** and may not work in fresh VS Code setups.
>
> **Recommended:** Use **[Verso](https://github.com/DataficationSDK/Verso)** as a drop-in replacement — it imports `.ipynb` files and supports the .NET kernel. Alternatively, run code samples as `.csx` scripts with `dotnet-script`.
>
> See the [Microsoft Agent Framework GA guide](../docs/MICROSOFT_AGENT_FRAMEWORK_GA.md#notebook-environment-polyglot-notebooks-deprecation) for full details and alternatives.

This directory contains interactive notebooks that guide you through building AI agents with **Semantic Kernel** and **Microsoft Agent Framework** in C#.

## 📚 Learning Sequence

Work through these notebooks in order to build your understanding progressively:

### 1. Foundation
- **[0-AI-settings.ipynb](0-AI-settings.ipynb)** - Configure your AI model settings
- **[1-SemanticKernel-Intro.ipynb](1-SemanticKernel-Intro.ipynb)** - Basic Semantic Kernel concepts

### 2. Agent Development
- **[2-SemanticKernel-Agents.ipynb](2-SemanticKernel-Agents.ipynb)** - Creating and managing AI agents
- **[3-Functions-Plugins.ipynb](3-Functions-Plugins.ipynb)** - Extending agents with functions and plugins
- **[3.1-OpenAPIPlugin.ipynb](3.1-OpenAPIPlugin.ipynb)** - Integrating with external APIs
- **[3.2-MCP.ipynb](3.2-MCP.ipynb)** - Connecting agents to external systems using Model Context Protocol

### 3. Advanced Orchestration
- **[4-MultiAgent-Orchestration.ipynb](4-MultiAgent-Orchestration.ipynb)** - Coordinating multiple agents
- **[5-ChatHistoryReducers.ipynb](5-ChatHistoryReducers.ipynb)** - Managing conversation context
- **[6-Agent-to-Agent-Protocol.ipynb](6-Agent-to-Agent-Protocol.ipynb)** - Standardized agent communication
- **[7-Process-Framework-and-HITL.ipynb](7-Process-Framework-and-HITL.ipynb)** - Event-driven workflows with human oversight
- **[8-Guardrails-and-AI-Safety.ipynb](8-Guardrails-and-AI-Safety.ipynb)** - Control mechanisms and safety filters

## 🛠️ Setup Requirements

### Prerequisites
- .NET 9 SDK or later
- **Notebook runner** — one of:
  - [Verso](https://github.com/DataficationSDK/Verso) (recommended — open-source Polyglot replacement)
  - Polyglot Notebooks VS Code extension (deprecated but still installable for now)
  - `dotnet-script` for running cells as `.csx` scripts
- AI model access (Azure OpenAI, GitHub Models, or local LLM)

### Package Versions (GA)

The notebooks reference these GA package versions:

```
Microsoft.SemanticKernel             1.74.0
Microsoft.Extensions.AI             9.7.1
Microsoft.Extensions.AI.Abstractions 9.10.0
Azure.AI.OpenAI                      2.5.0
```

> Some notebooks may still reference older preview versions in their `#r nuget:` directives. Update them to the GA versions above for best results.

### Configuration
1. **AI Settings**: Start with [`0-AI-settings.ipynb`](0-AI-settings.ipynb) to configure your model
2. **Dependencies**: Each notebook installs required NuGet packages automatically
3. **Settings File**: Update [`config/settings.json`](config/settings.json) with your API keys

## 📁 Supporting Files

- **[`config/`](config/)** - Configuration files and utilities
  - [`Settings.cs`](config/Settings.cs) - Configuration model
  - [`settings.json`](config/settings.json) - API keys and endpoints
  - [`Utils.cs`](config/Utils.cs) - Helper utilities
  - [`SkiaUtils.cs`](config/SkiaUtils.cs) - Graphics utilities

- **[`PromptPlugins/`](PromptPlugins/)** - Pre-built prompt plugins
  - [`ClassificationPlugin/`](PromptPlugins/ClassificationPlugin/) - Text classification prompts

- **[`knowledgebase/`](knowledgebase/)** - Sample knowledge documents for RAG scenarios

- **[`img/`](img/)** - Images and screenshots used in notebooks

## 🎯 Learning Objectives

By completing these notebooks, you will:

1. **Understand Semantic Kernel fundamentals** - Kernels, services, and basic operations
2. **Build AI agents** - Create specialized agents with distinct roles and capabilities
3. **Implement function calling** - Extend agents with custom functions and external plugins
4. **Master multi-agent orchestration** - Coordinate teams of agents working together
5. **Manage conversation context** - Handle chat history and context reduction strategies

## 💡 Tips for Success

- **Run notebooks sequentially** - Each builds upon concepts from previous ones
- **Experiment with examples** - Modify code samples to see how changes affect behavior
- **Check configuration** - Ensure your AI model settings are correct before starting
- **Review outputs** - Pay attention to agent responses and conversation flows

## 🔗 Related Resources

- **[Main Workshop Documentation](../docs/README.md)** - Complete project setup and architecture
- **[Assignments](../docs/assignments/)** - Hands-on exercises to practice concepts
- **[SKCodeAssistent Project](../src/SKCodeAssistent/)** - Full implementation example