using A2A;
using A2A.AspNetCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.A2A;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddAzureChatCompletionsClient(connectionName: "chat")
    .AddChatClient();

builder.Services.AddKernel();

var app = builder.Build();

app.MapDefaultEndpoints();

// Get kernel and create the agent
var kernel = app.Services.GetRequiredService<Kernel>();

var codingAgent = new ChatCompletionAgent()
{
    Kernel = kernel,
    Arguments = new KernelArguments(new PromptExecutionSettings()
    { 
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() 
    }),
    Name = "DevAssistant",
    Instructions = "You are a helpful assistant for developers.",
};

// Create the agent card
var agentCard = new AgentCard()
{
    Name = "A2ACodingAgent",
    Description = "Semantic Kernel-based developer assistant.",
    Url = "",
    Version = "1.0.0",
    DefaultInputModes = ["text"],
    DefaultOutputModes = ["text"],
    Capabilities = new AgentCapabilities()
    {
        Streaming = false,
        PushNotifications = false,
    },
    Skills =
    [
        new AgentSkill()
        {
            Id = "dev_assistant_sk",
            Name = "Semantic Kernel Developer Assistant",
            Description = "Handles comprehensive developer assistance, including code suggestions, debugging help, and API usage guidance.",
            Tags = ["development", "assistance", "semantic-kernel"],
            Examples =
            [
                "Help create a C# console application.",
                "Create a sudoku game in html and javascript",
            ],
        }
    ],
};

// Create A2A host agent and expose endpoints
var a2aHostAgent = new A2AHostAgent(codingAgent, agentCard);
var taskManager = a2aHostAgent.TaskManager!;

app.MapA2A(taskManager, "/");
app.MapWellKnownAgentCard(taskManager, "/");

await app.RunAsync();
