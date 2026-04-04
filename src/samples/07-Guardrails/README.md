# 07-Guardrails

Validate every agent response with a dedicated **guard agent** before showing it to the user.
This sample implements a two-stage pipeline: generate, then evaluate.

## What you'll learn

- How to use a second `AIAgent` as a content safety reviewer
- How to collect a full streaming response into a string for evaluation
- How to design guard agent prompts that produce structured verdicts

## Pipeline

```
User prompt
    │
    ▼
┌─────────────┐  response  ┌───────────┐  SAFE / BLOCKED
│ CodingAgent │───────────►│ GuardAgent│──────────────────► User (if SAFE)
└─────────────┘            └───────────┘                    🚫 (if BLOCKED)
```

## Guard checks

1. **Harmful code** — system deletes, malware patterns, exploits
2. **Sensitive data** — passwords, API keys, PII
3. **Relevance** — the response must answer the actual question

## Prerequisites

- .NET 9 SDK
- An OpenAI API key (each request calls the model twice — budget accordingly)

## Setup

```bash
cp .env.example .env
# Set OPENAI_API_KEY in .env
dotnet run
```

## Key code

```csharp
// Step 1: collect main agent's full response
var mainResponse = await CollectResponseAsync(mainAgent, userInput);

// Step 2: guard evaluates both prompt + response
var verdict = await CollectResponseAsync(guardAgent,
    $"User request: {userInput}\n\nDraft response:\n{mainResponse}");

// Step 3: only show SAFE responses
if (verdict.StartsWith("SAFE"))
    Console.WriteLine(mainResponse);
else
    Console.WriteLine($"🚫 Blocked: {verdict}");
```

## Extending this pattern

- Add an **input guardrail** (run guard on user prompt before calling main agent)
- Add a **grounding check** (verify factual claims against a knowledge base)
- Make the guard run in parallel with the main agent to reduce latency

## Next steps

- [08-Streaming](../08-Streaming/README.md) — advanced streaming patterns with metrics
