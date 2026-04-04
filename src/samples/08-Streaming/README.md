# 08-Streaming

Advanced streaming patterns for `AIAgent.RunStreamingAsync`.
Covers metrics, parallel streams, and cancellation.

## What you'll learn

- How to measure **Time to First Token (TTFT)** and total latency
- How to run **parallel streaming** requests concurrently
- How to **cancel** a stream mid-flight with `CancellationToken`

## Demos

| # | Demo | What it shows |
|---|------|---------------|
| 1 | Token-by-token with metrics | TTFT, chunk count, total ms |
| 2 | Parallel streams | Two prompts streamed simultaneously |
| 3 | Cancellation | Stop a stream after 2 seconds |

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
// Measure TTFT
var sw = Stopwatch.StartNew();
var firstChunk = TimeSpan.Zero;
int chunks = 0;
await foreach (var chunk in agent.RunStreamingAsync(prompt))
{
    if (chunks++ == 0) firstChunk = sw.Elapsed;
    Console.Write(chunk);
}
Console.WriteLine($"TTFT: {firstChunk.TotalMilliseconds:F0}ms");

// Cancellation
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
await foreach (var chunk in agent.RunStreamingAsync(prompt, cts.Token))
    Console.Write(chunk);
```

## Next steps

- [09-AgentWithAspire](../09-AgentWithAspire/README.md) — observability with Aspire dashboard
