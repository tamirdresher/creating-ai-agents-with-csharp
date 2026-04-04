/*
 * 09-AgentWithAspire — Worker
 *
 * An ASP.NET Core web application that hosts an AIAgent and exposes it
 * via a minimal HTTP API, with full OpenTelemetry observability.
 *
 * When run under Aspire (via the AppHost project), the OTLP exporter
 * automatically sends traces and metrics to the Aspire dashboard.
 *
 * Endpoints:
 *   POST /chat?message=<text>   — single-turn agent call, returns JSON
 *   GET  /health                — health probe for Aspire
 *
 * Setup:
 *   Use the AppHost project to run this; or set these env vars manually:
 *     OPENAI_API_KEY=...
 *     OPENAI_MODEL_ID=gpt-4o   (optional)
 */

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// ── OpenTelemetry ──────────────────────────────────────────────────────────────
//
// When running under Aspire, OTEL_EXPORTER_OTLP_ENDPOINT is injected automatically.
// Traces and metrics appear in the Aspire dashboard without extra configuration.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("agent-worker"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("Microsoft.Agents.*")    // Agent Framework spans
        .AddSource("Microsoft.Extensions.AI") // MEAI spans
        .AddOtlpExporter())                 // Aspire dashboard / Jaeger / Tempo
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("Microsoft.Agents.*")
        .AddMeter("Microsoft.Extensions.AI")
        .AddOtlpExporter());

// ── IChatClient + AIAgent ──────────────────────────────────────────────────────
var apiKey   = builder.Configuration["OPENAI_API_KEY"]
               ?? throw new InvalidOperationException("Set OPENAI_API_KEY");
var modelId  = builder.Configuration["OPENAI_MODEL_ID"] ?? "gpt-4o";
var endpoint = builder.Configuration["OPENAI_ENDPOINT"];

OpenAIClientOptions? openAiOptions = endpoint is { Length: > 0 }
    ? new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
    : null;

// Register IChatClient as a singleton so Aspire can instrument it
builder.Services.AddSingleton<IChatClient>(_ =>
    new OpenAIClient(new ApiKeyCredential(apiKey), openAiOptions)
        .GetChatClient(modelId)
        .AsIChatClient());

// Register AIAgent built from the injected IChatClient
builder.Services.AddSingleton<AIAgent>(sp =>
{
    var cc = sp.GetRequiredService<IChatClient>();
    return cc.AsAIAgent(
        name: "ObservableAgent",
        instructions: """
            You are a helpful .NET assistant. Answer questions concisely.
            Every trace of your responses is visible in the Aspire dashboard.
            """);
});

// ── Build and configure the app ────────────────────────────────────────────────
var app = builder.Build();

// Simple health endpoint (Aspire polls this)
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Single-turn chat endpoint
// Example: curl -X POST "http://localhost:5000/chat?message=Hello"
app.MapPost("/chat", async (string message, AIAgent agent) =>
{
    if (string.IsNullOrWhiteSpace(message))
        return Results.BadRequest(new { error = "message is required" });

    var sb = new System.Text.StringBuilder();
    await foreach (var chunk in agent.RunStreamingAsync(message))
        sb.Append(chunk);

    return Results.Ok(new { message, response = sb.ToString() });
});

// Streaming endpoint — returns SSE (Server-Sent Events)
// Example: curl -N "http://localhost:5000/chat/stream?message=Tell+me+about+LINQ"
app.MapGet("/chat/stream", async (string message, AIAgent agent, HttpContext ctx) =>
{
    ctx.Response.ContentType = "text/event-stream";
    await foreach (var chunk in agent.RunStreamingAsync(message))
    {
        await ctx.Response.WriteAsync($"data: {chunk}\n\n");
        await ctx.Response.Body.FlushAsync();
    }
    await ctx.Response.WriteAsync("data: [DONE]\n\n");
});

app.Run();
