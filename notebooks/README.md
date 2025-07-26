# Semantic Kernel Workshop Notebooks

This directory contains interactive Jupyter notebooks that guide you through learning Semantic Kernel concepts and building AI agents with C#.

## 📚 Learning Sequence

Work through these notebooks in order to build your understanding progressively:

### 1. Foundation
- **[0-AI-settings.ipynb](0-AI-settings.ipynb)** - Configure your AI model settings
- **[1-SemanticKernel-Intro.ipynb](1-SemanticKernel-Intro.ipynb)** - Basic Semantic Kernel concepts
- **[1-SemanticKernel-Intro-Enhanced.ipynb](1-SemanticKernel-Intro-Enhanced.ipynb)** - Advanced introduction topics

### 2. Agent Development
- **[2-SemanticKernel-Agents.ipynb](2-SemanticKernel-Agents.ipynb)** - Creating and managing AI agents
- **[3-Functions-Plugins-MCP.ipynb](3-Functions-Plugins-MCP.ipynb)** - Extending agents with functions and plugins
- **[3.1-OpenAPIPlugin.ipynb](3.1-OpenAPIPlugin.ipynb)** - Integrating with external APIs

### 3. Advanced Orchestration
- **[4-MultiAgent-Orchestration.ipynb](4-MultiAgent-Orchestration.ipynb)** - Coordinating multiple agents
- **[5-ChatHistoryReducers.ipynb](5-ChatHistoryReducers.ipynb)** - Managing conversation context

## 🛠️ Setup Requirements

### Prerequisites
- .NET 8 SDK or later
- Jupyter notebook environment with .NET Interactive kernel
- AI model access (Azure OpenAI, GitHub Models, or local LLM)

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