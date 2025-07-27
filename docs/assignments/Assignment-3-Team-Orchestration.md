# Assignment 3: Software Development Team Orchestration

## Objective
Transform your individual agents from Assignments 1 and 2 into a coordinated software development team that works together using advanced orchestration patterns. This assignment focuses on making agents collaborate effectively, hand off work appropriately, and coordinate their efforts like a real development team.

## Prerequisites
- Complete Assignment 1 (Three Agents with Semantic Kernel)
- Complete Assignment 2 (Plugins and MCP Integration)
- Have working file system and command execution plugins
- Understand agent group chat basics

## Requirements

### 1. Implementation Location
Continue implementing your solution in:

[src\SKCodeAssistent\SKCodeAssistent.Server\Services\CodingAssistentSession.cs](../../src/SKCodeAssistent/SKCodeAssistent.Server/Services/CodingAssistentSession.cs)

### 2. Team Orchestration Patterns

#### A. Custom Orchestration Strategy
Implement a custom orchestration strategy that manages how agents interact and coordinate their work:

**Key Components:**
- **Team Leader/Orchestrator**: Coordinates the overall workflow
- **Work Assignment**: Intelligently assigns tasks to appropriate agents
- **Progress Tracking**: Monitors work completion and dependencies
- **Quality Gates**: Ensures work meets standards before proceeding

####  Optional: Context Preservation
Implement conversation summarization to maintain context as conversations grow:

```csharp
public class ConversationSummarizer
{
    private readonly Kernel _kernel;
    
    public async Task<string> SummarizeConversation(
        IReadOnlyList<ChatMessageContent> history,
        int maxTokens = 2000)
    {
        if (history.Count <= 5) // Don't summarize short conversations
            return string.Empty;
            
        var conversationText = string.Join("\n", 
            history.Select(m => $"{m.Role}: {m.Content}"));
            
        var summarizeFunction = _kernel.CreateFunctionFromPrompt(@"
            Summarize the following software development team conversation.
            Focus on:
            1. What has been accomplished
            2. Current work in progress
            3. Next steps and dependencies
            4. Key decisions made
            5. Any blockers or issues
            
            Conversation:
            {{$conversation}}
            
            Summary:");
            
        var result = await _kernel.InvokeAsync(summarizeFunction, 
            new KernelArguments { ["conversation"] = conversationText });
            
        return result.GetValue<string>() ?? string.Empty;
    }
}
```


### 3. Example Solution Reference
For guidance, refer to the example solution at:


[src\SKCodeAssistent\SKCodeAssistent.Server/SCHOOL_SOLUTIONS/CodingAssistentSessions/CodingAssistentSession_CustomOrchestration.cs](../../src/SKCodeAssistent/SKCodeAssistent.Server/SCHOOL_SOLUTIONS/CodingAssistentSessions/CodingAssistentSession_CustomOrchestration.cs)


## Advanced Features (Optional)
- Implement parallel work when tasks are independent
- Add support for external stakeholder interactions

## Tips
- Start with a simple sequential workflow before adding complexity
- Test with increasingly complex scenarios
- Pay attention to conversation flow and context preservation
- Use the [4-MultiAgent-Orchestration.ipynb](../../notebooks/4-MultiAgent-Orchestration.ipynb) notebook for refrence


This assignment represents the culmination of the workshop, bringing together all concepts into a cohesive, production-ready multi-agent system.