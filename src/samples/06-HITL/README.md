# 06-HITL (Human-in-the-Loop)

Keep a human in the loop for **dangerous or irreversible actions**.
The approval gate lives inside the tool function — the agent is unaware of it and the framework needs no special configuration.

## What you'll learn

- How to gate a tool call behind a `Console.ReadLine()` prompt
- How to give the agent safe read-only tools alongside restricted write tools
- Why this pattern is simple, robust, and transparent to the agent

## How it works

```
User: "Delete old-file.txt"
       │
       ▼
   AIAgent calls DeleteFile("old-file.txt")
       │
       ▼  (inside the tool)
   ⚠️  Agent wants to DELETE: old-file.txt
   Approve? (yes / no): _
       │
   yes ──► File.Delete()  → returns "✅ Deleted"
   no  ──► returns "❌ Cancelled"  (agent reports back to user)
```

## Prerequisites

- .NET 9 SDK
- An OpenAI API key

## Setup

```bash
cp .env.example .env
# Set OPENAI_API_KEY in .env
dotnet run
```

## Key code

```csharp
[Description("Deletes a file. REQUIRES explicit human approval before executing.")]
static string DeleteFile(string filePath)
{
    Console.WriteLine($"⚠️  Agent wants to DELETE: {filePath}");
    Console.Write("Approve? (yes / no): ");
    var answer = Console.ReadLine()?.ToLowerInvariant();

    if (answer is "yes" or "y")
    {
        File.Delete(filePath);
        return $"✅ Deleted: {filePath}";
    }
    return $"❌ Deletion cancelled by user.";
}
```

## Next steps

- [07-Guardrails](../07-Guardrails/README.md) — validate every response with a guard agent
