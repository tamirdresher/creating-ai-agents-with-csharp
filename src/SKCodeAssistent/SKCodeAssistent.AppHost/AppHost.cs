using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Chat Service with Azure OpenAI
var azureAIModelId = builder.AddParameter("AzureOpenAIModelId", secret: true);
var azureAIEndpoint = builder.AddParameter("AzureOpenAIEndpoint", secret: true);
var azureAIApiKey = builder.AddParameter("AzureOpenAIApiKey", secret: true);
var openAIModelId =  builder.AddParameter("OpenAIModelId", secret: true);
var openAIEndpoint = builder.AddParameter("OpenAIEndpoint", secret: true);
var openAIApiKey = builder.AddParameter("OpenAIApiKey", secret: true);
builder.AddProject<Projects.SKCodeAssistent_Server>("server")
    .WithEnvironment("AIAgents__AzureOpenAI__ModelId", azureAIModelId)
    .WithEnvironment("AIAgents__AzureOpenAI__Endpoint", azureAIEndpoint)
    .WithEnvironment("AIAgents__AzureOpenAI__ApiKey", azureAIApiKey)
    .WithEnvironment("AIAgents__OpenAI__ModelId", openAIModelId)
    .WithEnvironment("AIAgents__OpenAI__Endpoint", openAIEndpoint)
    .WithEnvironment("AIAgents__OpenAI__ApiKey", openAIApiKey);

builder.Build().Run();
