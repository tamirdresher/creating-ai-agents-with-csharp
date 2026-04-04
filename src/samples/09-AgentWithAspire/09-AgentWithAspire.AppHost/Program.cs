/*
 * 09-AgentWithAspire — AppHost
 *
 * The Aspire AppHost is the entry point for the distributed application.
 * It declares all resources (projects, containers, databases) and their
 * relationships; Aspire then starts, monitors, and connects them.
 *
 * Run this project (not the Worker directly) to see the Aspire dashboard
 * with live traces, metrics, and structured logs from your agent.
 *
 * Usage:
 *   dotnet run --project src/samples/09-AgentWithAspire/09-AgentWithAspire.AppHost
 *
 * Then open the Aspire dashboard URL printed in the console.
 */

var builder = DistributedApplication.CreateBuilder(args);

// Add the Worker project as a managed resource.
// Aspire will start it, inject OTLP endpoints, and surface its telemetry.
var worker = builder.AddProject<Projects._09_AgentWithAspire_Worker>("agent-worker")
    // Pass OpenAI config from the AppHost's environment / secrets into the Worker
    .WithEnvironment("OPENAI_API_KEY",   builder.Configuration["OPENAI_API_KEY"] ?? "")
    .WithEnvironment("OPENAI_MODEL_ID",  builder.Configuration["OPENAI_MODEL_ID"] ?? "gpt-4o")
    .WithEnvironment("OPENAI_ENDPOINT",  builder.Configuration["OPENAI_ENDPOINT"] ?? "");

builder.Build().Run();
