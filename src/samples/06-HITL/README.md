# 06 — HITL (Human-in-the-Loop)

Require human approval before an agent executes destructive actions.

## What You'll Learn

- Implementing approval gates inside tool functions
- The HITL pattern: agent proposes → human approves → tool executes
- No framework changes needed — pure C# control flow

## How It Works

```
Agent wants to DELETE file.txt
  ⚠ Agent wants to DELETE: file.txt
  Approve? (yes/no): _
```

The approval check happens inside the tool function itself. The agent is unaware of the gate — it just sees the tool succeed or fail based on user response.

## Key Pattern

```csharp
[Description("Deletes a file. REQUIRES human approval.")]
static string DeleteFile(string filePath)
{
    Console.Write("Approve? (yes/no): ");
    if (Console.ReadLine() is "yes")
        File.Delete(filePath);
    else
        return "Cancelled by user.";
}
```

## Run

```bash
cp .env.example .env
dotnet run
```

Try: *"Create a file called test.txt with hello world"*

## Corresponding Notebook

📓 [7-Process-Framework-and-HITL.ipynb](../../../notebooks/7-Process-Framework-and-HITL.ipynb)
