/*
 * SKCodeAssistent.Server - Program.cs
 *
 * This file serves as the entry point for the SKCodeAssistent web application,
 * which provides AI-powered coding assistance through Semantic Kernel agents.
 *
 * The application supports multiple AI backends (Azure OpenAI, OpenAI) and provides
 * a REST API for VSCode extension integration, enabling various coding assistance modes
 * including single agent, multi-agent orchestration, and plugin-enhanced workflows.
 */

using SKCodeAssistent.Server.Services;
using SKCodeAssistent.Server.Plugins;
using SKCodeAssistent.Server.Configuration;
using SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Plugins;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// OpenTelemetry Configuration
// ========================================

// Enable OpenTelemetry diagnostics for Semantic Kernel operations
// This provides observability into AI operations, token usage, and performance metrics
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnostics", true);

// Enable sensitive data logging (prompts, completions, function calls)
// WARNING: Only enable in development - contains sensitive AI interaction data
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

// ========================================
// Configuration Setup
// ========================================

// Configure strongly-typed configuration binding for AI agent settings
// This binds appsettings.json "AIAgents" section to AgentConfiguration class
builder.Services.Configure<AgentConfiguration>(
    builder.Configuration.GetSection(AgentConfiguration.SectionName));

// Add configuration validation to ensure required AI service settings are present
// Validates at startup to fail fast if configuration is incomplete
builder.Services.AddOptions<AgentConfiguration>()
    .Bind(builder.Configuration.GetSection(AgentConfiguration.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ========================================
// Observability Setup
// ========================================

// Enable Semantic Kernel tracing for debugging and monitoring AI operations
// Provides detailed traces of agent interactions, plugin calls, and token usage
builder.Services.AddOpenTelemetry().WithTracing(b => b.AddSource("Microsoft.SemanticKernel*"));

// Enable Semantic Kernel metrics for performance monitoring
// Tracks response times, token counts, and success rates
builder.Services.AddOpenTelemetry().WithMetrics(b => b.AddMeter("Microsoft.SemanticKernel*"));

// Add .NET Aspire service defaults for distributed application support
builder.AddServiceDefaults();

// ========================================
// Web Framework Configuration
// ========================================

// Register MVC controllers and views for web UI and API endpoints
builder.Services.AddControllersWithViews();

// Configure CORS policy for VSCode extension communication
// Allows the VSCode extension to make API calls to this server
builder.Services.AddCors(options =>
{
    options.AddPolicy("VSCodePolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ========================================
// Core Service Registration
// ========================================

// Register workspace context service as singleton to maintain VSCode workspace state
// This service tracks the current workspace path and active document
builder.Services.AddSingleton<WorkspaceContextService>();

// Register session management service as singleton to handle multiple concurrent sessions
// Manages the lifecycle of coding assistant sessions and their state
builder.Services.AddSingleton<CodingAssistentsSessionService>();

// Register default coding assistant session implementation
// This is the base implementation used when no specific school solution is selected
builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession>();

// ========================================
// Implementation Examples
// ========================================
// The following section contains various implementation examples for educational purposes.
// These demonstrate different patterns of agent usage and orchestration techniques.
// Uncomment one of these lines to switch between different implementation approaches:

// Single agent implementation - demonstrates basic agent usage
//builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_SingleAgent>();

// Single agent with plugins - shows how to extend agent capabilities with plugins
//builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_SingleAgentWithPlugins>();

// Single agent with plugins and MCP - demonstrates Model Context Protocol integration
//builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_SingleAgentWithPluginsAndMCP>();

// Multi-agent orchestration using built-in Semantic Kernel patterns
//builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_MagenticOrchestration>();

// Custom orchestration implementation - shows advanced agent coordination
//builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_CustomOrchestration>();

// Custom orchestration with A2A remote agent
builder.Services.AddTransient<ICodingAssistentSession, CodingAssistentSession_CustomOrchestrationWithA2A>();

// ========================================
// Plugin Registration (Educational)
// ========================================
// These plugins extend agent capabilities with file operations and command execution.
// Uncomment these lines when using plugin-enabled implementations:

// File operations plugin - enables agents to read, write, and manipulate files
//builder.Services.AddSingleton<IKernelPlugin, FileOperationsPlugin>();

// Command execution plugin - allows agents to execute system commands
//builder.Services.AddSingleton<IKernelPlugin, CommandExecutionPlugin>();

// ========================================
// Logging Configuration
// ========================================

// Configure logging providers for development and production environments
// Console logging is useful for development, while debug logging helps with troubleshooting
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();    // Output logs to console
    logging.AddDebug();      // Output logs to debug window
});

// ========================================
// Application Pipeline Configuration
// ========================================

var app = builder.Build();

// Map .NET Aspire dashboard and health check endpoints
app.MapDefaultEndpoints();

// Configure HTTPS redirection only in production for security
// Development environments often use HTTP for simplicity
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Enable static file serving for web UI assets (CSS, JS, images)
app.UseStaticFiles();

// Apply CORS policy to allow VSCode extension communication
app.UseCors("VSCodePolicy");

// Configure routing middleware for MVC pattern
app.UseRouting();

// Configure default MVC route pattern for web UI
// Routes requests to {controller}/{action}/{id?} pattern
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Start the application and begin listening for requests
app.Run();
