using A2A;
using A2A.AspNetCore;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.A2A;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SemanticKernelAgent;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.AddAzureChatCompletionsClient(connectionName: "chat")
    .AddChatClient();

//builder.AddAzureOpenAIClient(connectionName: "chat")
//    .AddChatClient("chat");

var kernelBuilder = builder.Services.AddKernel();

//kernelBuilder.AddAzureOpenAIChatCompletion(
//                   deploymentName: modelId,
//                   endpoint: endpoint,
//                   credentials: new DefaultAzureCredential());

builder.Services.AddTransient<DeveloperAssistant>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DeveloperAssistant>>();
    var kernel = sp.GetRequiredService<Kernel>();
    return new DeveloperAssistant(kernel, configuration, httpClient, logger);
});
var app = builder.Build();

app.MapDefaultEndpoints();


var configuration = app.Configuration;
var httpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
var logger = app.Logger;

var agent = app.Services.GetService<DeveloperAssistant>();

//var taskManager = new TaskManager();
//agent.Attach(taskManager);

var a2aHostAgent = new A2AHostAgent(agent.Agent, await DeveloperAssistant.GetAgentCardAsync("", CancellationToken.None));
var taskManager = a2aHostAgent.TaskManager!;
app.MapA2A(taskManager, "/");
app.MapWellKnownAgentCard(taskManager, "/");

await app.RunAsync();
