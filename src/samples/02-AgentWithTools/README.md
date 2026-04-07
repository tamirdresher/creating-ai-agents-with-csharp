# 02 — Agent With Tools

Give an agent access to function tools using `AIFunctionFactory.Create()`.

## What You'll Learn

- Defining tools with `[Description]` attributes
- Registering tools via `AIFunctionFactory.Create()`
- How the agent autonomously decides when to call tools

## Key APIs

| API | Purpose |
|-----|---------|
| `AIFunctionFactory.Create()` | Converts a .NET method into an `AITool` |
| `[Description]` attribute | Populates tool descriptions for the model |
| `AsAIAgent(tools: [...])` | Creates an agent with tool access |

## Run

```bash
cp .env.example .env
dotnet run
```

Try: *"What time is it?"* or *"List the files in the current directory"*

## Corresponding Notebook

📓 [3-Functions-Plugins.ipynb](../../../notebooks/3-Functions-Plugins.ipynb)
