/*
 * SKCodeAssistent.Server - AgentConfiguration.cs
 *
 * This file defines the configuration structure for AI service providers and agent settings.
 * It supports both Azure OpenAI and OpenAI services with comprehensive validation.
 *
 * The configuration is bound from appsettings.json and validated at startup to ensure
 * all required AI service credentials and settings are properly configured.
 */

using System.ComponentModel.DataAnnotations;

namespace SKCodeAssistent.Server.Configuration;

/// <summary>
/// Main configuration class for AI agents and their associated services.
/// This class aggregates all AI-related configuration settings and is bound to the "AIAgents" section in appsettings.json.
/// </summary>
public class AgentConfiguration
{
    /// <summary>
    /// The configuration section name in appsettings.json where AI agent settings are stored.
    /// </summary>
    public const string SectionName = "AIAgents";
    
    /// <summary>
    /// Configuration settings for Azure OpenAI service integration.
    /// Used when connecting to Azure-hosted OpenAI models.
    /// </summary>
    [Required]
    public AzureOpenAIConfiguration AzureOpenAI { get; set; } = new();
    
    /// <summary>
    /// Configuration settings for OpenAI service integration.
    /// Used when connecting to OpenAI's API or compatible endpoints (e.g., GitHub Models).
    /// </summary>
    [Required]
    public OpenAIConfiguration OpenAI { get; set; } = new();
    
    /// <summary>
    /// General agent behavior settings that control conversation flow and limits.
    /// </summary>
    [Required]
    public AgentSettings Settings { get; set; } = new();

    public string RemoteDevAgentUrl { get; set; }
}

/// <summary>
/// Configuration for Azure OpenAI service connection.
/// Contains all necessary settings to connect to Azure-hosted OpenAI models.
/// </summary>
public class AzureOpenAIConfiguration
{
    /// <summary>
    /// The deployment name of the Azure OpenAI model to use.
    /// This corresponds to the deployment name configured in Azure OpenAI Service.
    /// </summary>
    [Required]
    public string ModelId { get; set; } = string.Empty;
    
    /// <summary>
    /// The Azure OpenAI service endpoint URL.
    /// Format: https://{your-resource-name}.openai.azure.com/
    /// </summary>
    [Required, Url]
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// The API key for accessing the Azure OpenAI service.
    /// This should be stored securely and not committed to source control.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Configuration for OpenAI service connection.
/// Supports both OpenAI API and compatible services like GitHub Models.
/// </summary>
public class OpenAIConfiguration
{
    /// <summary>
    /// The model identifier to use for OpenAI API calls.
    /// Examples: "gpt-4", "gpt-3.5-turbo", "gpt-4-turbo-preview"
    /// </summary>
    [Required]
    public string ModelId { get; set; } = string.Empty;
    
    /// <summary>
    /// The OpenAI API endpoint URL.
    /// Example: https://api.openai.com/v1 for OpenAI
    /// For GitHub Models: https://models.inference.ai.azure.com
    /// </summary>
    [Required, Url]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// The API key for accessing the OpenAI service.
    /// For GitHub Models, this would be a GitHub personal access token.
    /// This should be stored securely and not committed to source control.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// General settings that control agent behavior and conversation management.
/// These settings help prevent runaway conversations and manage resource usage.
/// </summary>
public class AgentSettings
{
    /// <summary>
    /// Maximum number of iterations an agent can perform in a single conversation turn.
    /// Prevents infinite loops and controls resource usage.
    /// </summary>
    [Range(1, 50)]
    public int MaximumIterations { get; set; } = 15;
    
    /// <summary>
    /// Maximum number of times an agent can invoke functions in a single turn.
    /// Controls how many tool calls or plugin invocations are allowed.
    /// </summary>
    [Range(1, 10)]
    public int MaximumInvocationCount { get; set; } = 15;
    
    /// <summary>
    /// Whether to automatically reset conversation history when limits are reached.
    /// Helps maintain conversation flow while preventing resource exhaustion.
    /// </summary>
    public bool AutomaticReset { get; set; } = true;
    
    /// <summary>
    /// Target number of messages to keep when summarizing conversation history.
    /// Used to maintain context while keeping conversations manageable.
    /// </summary>
    [Range(1, 100)]
    public int HistorySummaryTargetCount { get; set; } = 1;
}
