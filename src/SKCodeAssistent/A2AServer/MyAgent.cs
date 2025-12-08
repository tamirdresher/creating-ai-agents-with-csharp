using A2A;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Polly;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace SemanticKernelAgent;



/// <summary>
/// Wraps Semantic Kernel-based agents to handle Travel related tasks
/// </summary>
public class DeveloperAssistant : IDisposable
{
    public static readonly ActivitySource ActivitySource = new("A2A.SemanticKernelTravelAgent", "1.0.0");

    /// <summary>
    /// Initializes a new instance of the SemanticKernelTravelAgent
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <param name="httpClient">HTTP client</param>
    /// <param name="logger">Logger for the agent</param>
    public DeveloperAssistant(
        Kernel kernel,
        IConfiguration configuration,
        HttpClient httpClient,
        ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _kernel = kernel;
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

                // Initialize the agent
        _agent = InitializeAgent();
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void Attach(ITaskManager taskManager)
    {
        _taskManager = taskManager;
        taskManager.OnTaskCreated = ExecuteAgentTaskAsync;
        taskManager.OnTaskUpdated = ExecuteAgentTaskAsync;
        taskManager.OnAgentCardQuery = GetAgentCardAsync;
    }

    public async Task ExecuteAgentTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        if (_taskManager == null)
        {
            throw new InvalidOperationException("TaskManager is not attached.");
        }

        try
        {
            await _taskManager.UpdateStatusAsync(task.Id, TaskState.Working, cancellationToken: cancellationToken);

            // Get message from the user
            var userMessage = task.History!.Last().Parts.First().AsTextPart().Text;

            // Get the response from the agent
            var artifact = new Artifact();
            await foreach (AgentResponseItem<ChatMessageContent> response in Agent.InvokeAsync(userMessage, cancellationToken: cancellationToken))
            {
                var content = response.Message.Content;
                artifact.Parts.Add(new TextPart() { Text = content! });
            }

            // Return as artifacts
            await _taskManager.ReturnArtifactAsync(task.Id, artifact, cancellationToken);
            await _taskManager.UpdateStatusAsync(task.Id, TaskState.Completed, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {

            throw;
        }
    }

    public static Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<AgentCard>(cancellationToken);
        }

        var capabilities = new AgentCapabilities()
        {
            Streaming = false,
            PushNotifications = false,
        };

        var skillTripPlanning = new AgentSkill()
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
        };

        return Task.FromResult(new AgentCard()
        {
            Name = "SK Coding Agent",
            Description = "Semantic Kernel-based developer assistant.",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [skillTripPlanning],
        });
    }

    #region private
    private readonly ILogger _logger;
    private readonly Kernel _kernel;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ChatCompletionAgent _agent;
    private ITaskManager? _taskManager;

    public List<string> SupportedContentTypes { get; } = ["text", "text/plain"];

    public ChatCompletionAgent Agent => _agent;

    private ChatCompletionAgent InitializeAgent()
    {
        try
        {   
            var codingAssistantAgent = new ChatCompletionAgent()
            {
                Kernel = _kernel,
                Arguments = new KernelArguments(new PromptExecutionSettings()
                { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
                Name = "DevAssistant",
                Instructions = "You are a helpful assistant for developers.",

            };

            return codingAssistantAgent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Semantic Kernel agent");
            throw;
        }
    }

    #endregion
}
