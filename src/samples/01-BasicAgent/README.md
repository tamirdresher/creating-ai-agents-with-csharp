# 01 — Basic Agent

The simplest Microsoft Agent Framework sample: create an `AIAgent`, give it instructions, and chat.

## What You'll Learn

- Creating an `AIAgent` from an OpenAI `ChatClient` using `.AsAIAgent()`
- Streaming responses with `RunStreamingAsync()`
- Configuring API keys via `.env` files

## Key APIs

| API | Purpose |
|-----|---------|
| `ChatClient.AsAIAgent()` | Wraps a chat client as an AI agent |
| `agent.RunAsync()` | Get a complete response |
| `agent.RunStreamingAsync()` | Stream response chunks |

## Run

```bash
cp .env.example .env    # fill in your API key
dotnet run
```

## Corresponding Notebook

📓 [1-SemanticKernel-Intro.ipynb](../../../notebooks/1-SemanticKernel-Intro.ipynb)
