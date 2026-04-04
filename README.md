# Creating AI Applications and Agents with C# - Semantic Kernel C# Workshop

A comprehensive workshop for building AI applications and agents using Microsoft's Semantic Kernel framework in C#.

## 🆕 What's Changed — Microsoft Agent Framework GA (July 2025)

Microsoft has unified Semantic Kernel and AutoGen under the **Microsoft Agent Framework (MAF)** umbrella. Here's what you need to know:

- **Semantic Kernel remains the recommended way to build agents in C#.** SK Agents API reached GA at 1.45.x and is now at **1.74.0**. All notebooks and projects in this workshop are pinned to 1.74.0.
- **Microsoft.Extensions.AI** has reached GA at **10.4.1**, providing the `IChatClient` abstraction that bridges AI providers with a common interface. Notebook 1 demonstrates this directly.
- **Model Context Protocol (MCP)** C# SDK is now GA at **1.2.0** (was preview). Notebook 3.2 covers MCP integration.
- **MAF-specific packages** (`Microsoft.Agents.AI.*`) are still at RC status and are **not used** in this workshop. When they reach GA, migration guidance will be added.
- **Agent-to-Agent (A2A) Protocol** SDK remains in preview (0.3.3-preview). Notebook 6 and the A2AServer project demonstrate this protocol.

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

- **[GitHub Models Setup Guide](docs/Using_GitHub_Models.md)** - Configure GitHub marketplace models
- **[Notebook Learning Materials](notebooks/)** - Interactive Jupyter notebooks
- **[Assignment Solutions](docs/assignments/)** - Guided exercises


