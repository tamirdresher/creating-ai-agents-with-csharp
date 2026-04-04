# Microsoft Agent Framework (MAF) — What You Need to Know

> **Audience:** Workshop participants who want to understand where the Microsoft AI agent ecosystem is heading and how Semantic Kernel fits into the bigger picture.

---

## 1. What Is the Microsoft Agent Framework?

In 2025 Microsoft unified its two major agent-building libraries — **Semantic Kernel (SK)** and **AutoGen** — under a single umbrella called the **Microsoft Agent Framework (MAF)**. The goal is one cohesive platform for building, orchestrating, and deploying AI agents on .NET.

Key points:

| Aspect | Details |
|--------|---------|
| **Announcement** | Microsoft Build 2025 — MAF positioned as the standard .NET agent platform |
| **GA target** | Ships with .NET 10 (November 2025) |
| **Current status** | MAF-specific packages (`Microsoft.Agents.AI.*`) are at **RC5** — not yet production-ready |
| **Foundation** | Semantic Kernel remains the core runtime; MAF adds higher-level orchestration on top |

---

## 2. How SK and MAF Relate

```
┌─────────────────────────────────────────────────┐
│           Microsoft Agent Framework (MAF)        │
│                                                   │
│  ┌──────────────┐   ┌─────────────────────────┐  │
│  │  AgentKernel  │   │  Declarative Agents     │  │
│  │  A2A comms    │   │  Agent-to-Agent (A2A)   │  │
│  │  Multi-agent  │   │  Agent orchestration    │  │
│  └──────┬───────┘   └────────────┬────────────┘  │
│         │                        │                │
│  ┌──────┴────────────────────────┴──────────┐     │
│  │         Semantic Kernel (SK 1.74.0)       │     │
│  │  Kernel · Plugins · Agents API · Memory   │     │
│  └──────────────────────────────────────────┘     │
│                                                   │
│  ┌──────────────────────────────────────────┐     │
│  │   Microsoft.Extensions.AI (10.4.1 GA)    │     │
│  │   IChatClient · Embeddings · Middleware   │     │
│  └──────────────────────────────────────────┘     │
└─────────────────────────────────────────────────┘
```

**SK is not being replaced.** Think of MAF as an additional layer:

- **Microsoft.Extensions.AI** provides the low-level AI abstractions (`IChatClient`, embeddings, middleware).
- **Semantic Kernel** builds on that with the Kernel, plugins, prompt templates, and the Agents API (`ChatCompletionAgent`, `AgentGroupChat`).
- **MAF** adds a higher-level `AgentKernel`, declarative agent definitions, cross-process agent communication, and tighter AutoGen interop.

Everything you learn in this workshop about SK — kernels, plugins, function calling, multi-agent orchestration — carries forward directly into MAF.

---

## 3. Key MAF Concepts

### AgentKernel
The `AgentKernel` is MAF's top-level orchestrator. Where SK's `Kernel` manages a single agent's capabilities (plugins, memory, services), the `AgentKernel` manages **multiple agents** and their interactions. Think of it as a "kernel of kernels."

### Agent-to-Agent (A2A) Communication
MAF formalizes how agents talk to each other across process boundaries using the **A2A protocol** (currently at v0.3.3 preview). This workshop's Notebook 6 already demonstrates A2A concepts using the current SK preview APIs.

### Declarative Agents
MAF introduces a JSON/YAML-based way to define agents without writing C# code — specifying their instructions, tools, and orchestration rules declaratively. This is still evolving and not yet stable.

### Orchestration Patterns
The orchestration patterns you learn in Notebook 4 (concurrent, sequential, group chat, custom) map directly to MAF's orchestration layer. MAF adds additional patterns like handoff orchestration and more flexible routing.

---

## 4. Why This Workshop Uses SK Directly

| Reason | Explanation |
|--------|-------------|
| **Stability** | SK 1.74.0 is GA and battle-tested. MAF packages are RC5 — APIs may still change. |
| **Foundation first** | Understanding SK deeply makes MAF adoption trivial. MAF builds on SK, not beside it. |
| **Breaking changes ahead** | `IChatClient.CompleteAsync` will become `GetResponseAsync`; `ChatCompletion` becomes `ChatResponse`. Better to learn the stable surface first. |
| **Notebook tooling** | Polyglot Notebooks (used in this workshop) have announced end-of-support for March 2026. Keeping to stable SK avoids compounding risk. |

---

## 5. What Changes When MAF Reaches GA

When MAF ships as GA (expected with .NET 10), here is what you can expect:

1. **New top-level namespace**: `Microsoft.Agents.AI.*` packages become stable.
2. **AgentKernel replaces manual orchestration**: Instead of wiring `AgentGroupChat` by hand, you configure agents in the `AgentKernel` and let MAF handle routing.
3. **Declarative agent definitions**: Define agents in JSON/YAML configuration files rather than purely in C# code.
4. **IChatClient breaking changes**: The `CompleteAsync`/`CompleteStreamingAsync` methods become `GetResponseAsync`/`GetStreamingResponseAsync`, and `ChatCompletion` becomes `ChatResponse`.
5. **Deprecated packages removed**: Planner packages (Handlebars, Stepwise) are already deprecated — agent-driven orchestration is the replacement.
6. **A2A protocol stabilizes**: Cross-process agent communication moves from preview to GA.

### Migration Path for Workshop Exercises

| Workshop Notebook | Current Approach | MAF Equivalent |
|---|---|---|
| 1 — SK Intro | `Kernel` + `IChatCompletionService` | Same — SK Kernel stays |
| 2 — SK Agents | `ChatCompletionAgent` | `ChatCompletionAgent` (unchanged, part of MAF) |
| 3 — Plugins & MCP | SK Plugins + MCP SDK | Same — plugins are SK-native, MCP SDK is standalone |
| 4 — Multi-Agent | `AgentGroupChat` + orchestration | `AgentKernel` with declarative orchestration |
| 6 — A2A Protocol | Preview A2A SDK | GA A2A protocol via MAF |
| 7 — Process Framework | SK Process Framework | Evolves into MAF's event-driven workflows |

---

## 6. Resources

- [Semantic Kernel GitHub](https://github.com/microsoft/semantic-kernel) — the SK source and samples
- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai) — the AI abstraction layer
- [Agent-to-Agent Protocol](https://github.com/google/A2A) — the open A2A specification
- [Semantic Kernel Blog](https://devblogs.microsoft.com/semantic-kernel/) — announcements and deep dives
- [AutoGen GitHub](https://github.com/microsoft/autogen) — the AutoGen side of the MAF story

---

## 7. TL;DR for Students

> **You are learning the right thing.** Semantic Kernel is the foundation of Microsoft's agent platform. Everything in this workshop — kernels, plugins, agents, orchestration — is the stable base that MAF builds on. When MAF reaches GA, you will add a few new concepts (AgentKernel, declarative agents) on top of what you already know. There is no "start over" moment.

---

*Last updated: July 2025 — Aligned with SK 1.74.0, Microsoft.Extensions.AI 10.4.1, MAF RC5*
