# Microsoft Agent Framework GA — What Changed & How to Migrate

> **Last updated:** June 2026  
> **Framework version:** Microsoft Agent Framework v1.0.0 (GA)  
> **Semantic Kernel version:** v1.74+  
> **Microsoft.Extensions.AI version:** 9.7.1 (Abstractions: 9.10.0)

---

## What is Microsoft Agent Framework?

**Microsoft Agent Framework** (MAF) is Microsoft's unified, production-ready platform for building AI agents with .NET. It merges two previously separate frameworks:

| Before (Preview) | After (GA — Agent Framework) |
|---|---|
| Semantic Kernel (enterprise agents, plugins) | Core component of Agent Framework |
| AutoGen (multi-agent from MSR) | Core component of Agent Framework |
| Separate teams, docs, packages | Single unified platform |

The GA release is available at:
- **GitHub:** https://github.com/microsoft/agent-framework
- **Blog:** https://devblogs.microsoft.com/agent-framework/
- **NuGet:** [microsoft/agent-framework packages](https://www.nuget.org/profiles/MicrosoftAgentFramework/)

---

## What Changed from Preview to GA

### Microsoft.Extensions.AI

| Area | Preview | GA |
|---|---|---|
| `IChatClient` interface | Experimental, evolving | ✅ Stable, versioned |
| `IEmbeddingGenerator<TInput,TEmbedding>` | Experimental | ✅ Stable |
| Platform targets | .NET 6, 7, 8 | .NET 8, .NET 9, .NET 4.6.2, netstandard2.0 |
| Provider integrations | Reference only | Open to all ISVs; plug-and-play |
| OpenTelemetry middleware | Preview | ✅ Stable via `UseOpenTelemetry()` |
| Caching / rate-limiting middleware | Preview | ✅ Stable pipeline |
| `Microsoft.Extensions.VectorData` | Preview | ✅ GA alongside MEAI |

**NuGet package update:**
```xml
<!-- Before (preview) -->
<PackageReference Include="Microsoft.Extensions.AI" Version="9.0.0-preview.*" />

<!-- After (GA) -->
<PackageReference Include="Microsoft.Extensions.AI" Version="9.7.1" />
<PackageReference Include="Microsoft.Extensions.AI.Abstractions" Version="9.10.0" />
```

### Semantic Kernel (as part of Agent Framework)

| Area | Preview / v1.x early | GA (v1.74+) |
|---|---|---|
| `KernelFunction` / plugins | Stable | ✅ No change needed |
| `ChatCompletionAgent` | Experimental | ✅ GA |
| Process Framework (`KernelProcess`) | Preview | ✅ GA |
| A2A Protocol | Preview | ✅ GA |
| MCP tool integration | Preview | ✅ GA |
| Multi-agent `AgentGroupChat` | Experimental | ✅ GA (use `AgentChat` APIs) |
| `ITextEmbeddingGenerationService` | Stable SK interface | ⚠️ Transitioning to `IEmbeddingGenerator` from MEAI |

**Package update:**
```xml
<!-- Minimum recommended version -->
<PackageReference Include="Microsoft.SemanticKernel" Version="1.74.0" />
```

### Microsoft Agent Framework (new unified packages)

For new projects targeting the full Agent Framework:
```xml
<PackageReference Include="Microsoft.Extensions.AI" Version="9.7.1" />
<PackageReference Include="Microsoft.SemanticKernel" Version="1.74.0" />
<PackageReference Include="Microsoft.SemanticKernel.Agents.Core" Version="1.74.0" />
<PackageReference Include="Microsoft.SemanticKernel.Process.Core" Version="1.74.0" />
```

---

## New GA Features in This Workshop

### 1. Graph-Based Agent Orchestration
The GA release introduces stateful, graph-based workflows — think LangGraph for .NET.

```csharp
// GA: Use KernelProcess for stateful, event-driven workflows
var processBuilder = new ProcessBuilder("CodeReviewWorkflow");
var analyzeStep = processBuilder.AddStepFromType<AnalyzeCodeStep>();
var reviewStep = processBuilder.AddStepFromType<ReviewCodeStep>();

processBuilder
    .OnInputEvent("StartReview")
    .SendEventTo(new ProcessFunctionTargetBuilder(analyzeStep));
    
analyzeStep
    .OnFunctionResult()
    .SendEventTo(new ProcessFunctionTargetBuilder(reviewStep));
```

### 2. A2A Protocol (Agent-to-Agent)
Standard protocol for agent-to-agent communication across different frameworks and runtimes:

```csharp
// See notebooks/6-Agent-to-Agent-Protocol.ipynb
// GA: A2A is now a first-class citizen in Semantic Kernel Agents
```

### 3. MCP Integration (GA)
Model Context Protocol for connecting agents to external tools:

```csharp
// See notebooks/3.2-MCP.ipynb
// GA: Microsoft.SemanticKernel.Mcp is stable
```

---

## Notebook Environment: Polyglot Notebooks Deprecation

> ⚠️ **Important for workshop attendees**

Microsoft deprecated **Polyglot Notebooks** (the VS Code extension that powered `.ipynb` C# execution) in **February 2026**. No new features or bug fixes will be released.

### What this means for the workshop notebooks

The `.ipynb` files in `notebooks/` use the C#/Polyglot kernel (`"kernelName": "csharp"`). They **will continue to work** as long as the deprecated extension is installed, but:
- You cannot install it fresh from the marketplace for much longer
- Bug fixes will not come for kernel issues

### Recommended alternatives (in order of preference)

| Option | Best for | Notes |
|---|---|---|
| **[Verso](https://github.com/DataficationSDK/Verso)** | Drop-in replacement for Polyglot | Imports `.ipynb`, .NET engine, open-source |
| **.csx script files** | Running individual notebook cells | Use `dotnet-script` or `csi` |
| **Regular C# console project** | Full code samples | Best for production-pattern learning |
| **Jupyter + dotnet-interactive kernel** | Familiar Jupyter UX | Works while `dotnet-interactive` still has kernel support |

### Running a notebook cell as a .csx script

Any cell that starts with `#r "nuget: ..."` can be run as a C# script:

```bash
# Install dotnet-script (one time)
dotnet tool install -g dotnet-script

# Run a notebook cell as a script
dotnet script notebooks/scripts/1-intro-example.csx
```

---

## Migration Steps for Existing Code

If you have code written against preview packages, here's a quick checklist:

- [ ] Update `Microsoft.Extensions.AI` to `9.7.1`
- [ ] Update `Microsoft.Extensions.AI.Abstractions` to `9.10.0`
- [ ] Update `Microsoft.SemanticKernel` to `1.74.0`
- [ ] Replace `Microsoft.Extensions.AI.OpenAI` preview references with the stable version
- [ ] If using `ITextEmbeddingGenerationService`, plan migration to `IEmbeddingGenerator<string, Embedding<float>>` (SK provides adapters)
- [ ] If using `AgentGroupChat`, check the updated API surface — some overloads changed
- [ ] Update `Azure.AI.OpenAI` from `2.5.0-beta.*` to `2.5.0` (stable)

---

## Resources

- 📖 [Agent Framework blog](https://devblogs.microsoft.com/agent-framework/)
- 🐙 [microsoft/agent-framework on GitHub](https://github.com/microsoft/agent-framework)
- 🐙 [dotnet/extensions on GitHub](https://github.com/dotnet/extensions) (source for `Microsoft.Extensions.AI`)
- 📦 [Microsoft.Extensions.AI on NuGet](https://www.nuget.org/packages/Microsoft.Extensions.AI/)
- 📦 [Microsoft.SemanticKernel on NuGet](https://www.nuget.org/packages/Microsoft.SemanticKernel/)
- 📄 [MEAI official docs](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai)
- 🔗 [Polyglot Notebooks deprecation info](https://www.devclass.com/databases/2026/02/14/microsoft-deprecates-polyglot-notebooks-developers-react/)
- 🔗 [Verso — open-source Polyglot replacement](https://github.com/DataficationSDK/Verso)
- 📄 [Transitioning to new IEmbeddingGenerator interface](https://devblogs.microsoft.com/agent-framework/transitioning-to-new-iembeddinggenerator-interface/)
