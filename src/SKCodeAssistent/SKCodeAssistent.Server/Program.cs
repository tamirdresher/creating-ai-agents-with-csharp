/*
 * AICodeAssistant.Server - Program.cs
 *
 * Entry point for the AI Code Assistant web application, powered by
 * Microsoft Agent Framework GA (v1.0.0, April 2026).
 *
 * Previously used Semantic Kernel. Migrated to Agent Framework which unifies
 * SK and AutoGen under a single, stable API surface built on MEAI.
 *
 * The application exposes a REST API consumed by a VS Code extension and
 * demonstrates multiple agent patterns in SCHOOL_SOLUTIONS/.
 */

using SKCodeAssistent.Server.Services;
using SKCodeAssistent.Server.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// Configuration Setup
// ========================================

builder.Services.Configure<AgentConfiguration>(
    builder.Configuration.GetSection(AgentConfiguration.SectionName));

builder.Services.AddOptions<AgentConfiguration>()
    .Bind(builder.Configuration.GetSection(AgentConfiguration.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ========================================
// Observability -- Agent Framework + MEAI
// ========================================
// Agent Framework emits traces under "Microsoft.Agents.*" and "Microsoft.Extensions.AI"
// (no longer "Microsoft.SemanticKernel*" -- those AppContext switches are removed).

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b
        .AddSource("Microsoft.Agents.*")
        .AddSource("Microsoft.Extensions.AI"))
    .WithMetrics(b => b
        .AddMeter("Microsoft.Agents.*")
        .AddMeter("Microsoft.Extensions.AI"));

// .NET Aspire service defaults (health checks, OTEL exporters, etc.)
builder.AddServiceDefaults();

// ========================================
// Web Framework
// ========================================

builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy("VSCodePolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ========================================
// Core Services
// ========================================

builder.Services.AddSingleton<WorkspaceContextService>();
builder.Services.AddSingleton<CodingAssistentsSessionService>();

// Default (template) session -- participants fill in the TODOs
builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession>();

// ──────────────────────────────────────────────────────────────────────────────
//  SCHOOL SOLUTIONS -- uncomment one line to switch the active implementation.
//  All implementations live in SCHOOL_SOLUTIONS/CodingAssistentSessions/.
// ──────────────────────────────────────────────────────────────────────────────

// Step 1 -- Single agent (basic)
//builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_SingleAgent>();

// Step 2 -- Single agent with AIFunction tools
//builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_SingleAgentWithTools>();

// Step 3 -- Single agent with MCP tool server
//builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_SingleAgentWithMCP>();

// Step 4 -- Multi-agent workflow (WorkflowBuilder)
//builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_MultiAgentWorkflow>();

// Step 5 -- Multi-agent with A2A remote agent
//builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_MultiAgentWithA2A>();

// ========================================
// Logging
// ========================================

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// ========================================
// Application Pipeline
// ========================================

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsProduction())
    app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseCors("VSCodePolicy");
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
