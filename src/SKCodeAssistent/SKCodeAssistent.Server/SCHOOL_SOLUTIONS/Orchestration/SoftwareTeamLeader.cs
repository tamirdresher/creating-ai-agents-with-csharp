using Microsoft.Extensions.AI;
using SKCodeAssistent.Server.Services;

namespace SKCodeAssistent.Server.SCHOOL_SOLUTIONS.Orchestration;

/// <summary>
/// Utility class that holds the team-coordination prompts and provides
/// a helper for building the contextual message handed to the workflow.
/// Migrated from the SK GroupChatManager pattern; coordination is now
/// managed by the Agent Framework WorkflowBuilder.
/// </summary>
public class SoftwareTeamLeader
{
    private readonly string _topic;
    private readonly IChatClient _chatClient;

    public SoftwareTeamLeader(string topic, IChatClient chatClient)
    {
        _topic      = topic;
        _chatClient = chatClient;
    }

    // -- Prompt templates (preserved from SK implementation) --

    public string TerminationPrompt() =>
        $"""
        You are a software team manager working on: '{_topic}'.
        Determine if the discussion has reached a conclusion and all artifacts were created.
        The most important outcome is working code — make sure all code files were produced.
        If the discussion is going in circles, or you need more user input to proceed, terminate.
        Respond with True to end, False to continue.
        """;

    public string SelectionPrompt(IEnumerable<string> participantDescriptions) =>
        $"""
        You are a software team manager working on: '{_topic}'.
        Select the next team member to speak. Participants:
        {string.Join("\n", participantDescriptions)}
        Respond with only the role name of the participant you select.
        """;

    public string FilterResultsPrompt() =>
        $"""
        You are a software team manager working on: '{_topic}'.
        The team discussion has concluded. Summarise the discussion and list the final artifacts produced.
        """;

    /// <summary>
    /// Builds the contextual first message that is fed into the workflow.
    /// </summary>
    public string BuildContextualMessage(
        string workspacePath,
        string activeDocPath,
        string historySummary,
        string userMessage) =>
        $"""
        Working in workspace: {workspacePath}
        Active Document: {activeDocPath}

        If the workspace is not set, don't attempt to write any file unless asked for.
        You are part of a collaborative team of agents — pass requests to teammates when they are better suited.
        Keep the discussion as short as possible; finish as early as you can.

        {(string.IsNullOrWhiteSpace(historySummary) ? string.Empty : $"Summary of chat so far: {historySummary}\n")}
        User Request: {userMessage}
        """;
}
