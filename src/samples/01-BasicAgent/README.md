# 01-BasicAgent

A minimal "hello world" console app showing the core Microsoft Agent Framework GA pattern.

## What you'll learn

- How to get an `IChatClient` from the OpenAI SDK
- How to create an `AIAgent` with a system prompt
- How to stream a response with `RunStreamingAsync`

## Prerequisites

- .NET 9 SDK
- An OpenAI API key (or GitHub Models token)

## Setup

```bash
cp .env.example .env
# Edit .env and set OPENAI_API_KEY
```

## Run

```bash
dotnet run
```

## Key code

```csharp
// 1. Get an IChatClient
IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey))
    .GetChatClient("gpt-4o")
    .AsIChatClient();

// 2. Create an AIAgent
AIAgent agent = chatClient.AsAIAgent(
    name: "CSharpTutor",
    instructions: "You are a friendly C# tutor...");

// 3. Stream a response
await foreach (var chunk in agent.RunStreamingAsync("Explain records in C#"))
    Console.Write(chunk);
```

## Next steps

- [02-AgentWithTools](../02-AgentWithTools/README.md) — add `AIFunction` tools to your agent
