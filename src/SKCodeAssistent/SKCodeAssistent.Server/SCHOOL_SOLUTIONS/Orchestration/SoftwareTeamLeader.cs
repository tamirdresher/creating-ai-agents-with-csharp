using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

#pragma warning disable SKEXP0110 

class SoftwareTeamLeader(string topic, IChatCompletionService chatCompletion) : GroupChatManager
{
    private static class Prompts
    {
        public static string Termination(string request) =>
            $"""
                You are software team manager that work for creating the user software related request, User request: '{request}'. 
                You need to determine if the discussion has reached a conclusion and if all artifacts were created.
                if no artifacts were produced yet and you don't have knowledge of them yet, then continue the dicsussion and planning.
                The MOST important thing is that we need to have a working code! so the user expect to have as output the code files names and their contents.
                To end the discussion, make sure all the code files were created and that there are no more artifacts (such as files) to produce or are missing.
                If there we the rest of the team discussion is goind in circles, or if to complete the task you need more infor from the user and there's nothing else you can accomplish, then terminate the discussion.
                If you would like to end the discussion, please respond with True. Otherwise, respond with False.
                """;

        public static string Selection(string request, string participants) =>
            $"""
                You are software team manager that work for creatingthe user software related request: '{request}'. 
                You need to select the next team member to speak and create artifacts. 
                Here are the roles and descriptions of the participants: 
                {participants}\n
                Please respond with only the role of the participant you would like to select.
                """;

        public static string Filter(string request) =>
            $"""
                You are software team manager that work for creating the user software related request: '{request}'. 
                You have just concluded the discussion and team work. 
                Please summarize the discussion and provide the final artifacts.
                """;
    }

    /// <inheritdoc/>
    public override ValueTask<GroupChatManagerResult<string>> FilterResults(ChatHistory history, CancellationToken cancellationToken = default) =>
        this.GetResponseAsync<string>(history, Prompts.Filter(topic), cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<GroupChatManagerResult<string>> SelectNextAgent(ChatHistory history, GroupChatTeam team, CancellationToken cancellationToken = default) =>
        this.GetResponseAsync<string>(history, Prompts.Selection(topic, team.FormatList()), cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<GroupChatManagerResult<bool>> ShouldRequestUserInput(ChatHistory history, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(new GroupChatManagerResult<bool>(false) { Reason = "The AI group chat manager does not request user input." });

    /// <inheritdoc/>
    public override async ValueTask<GroupChatManagerResult<bool>> ShouldTerminate(ChatHistory history, CancellationToken cancellationToken = default)
    {
        GroupChatManagerResult<bool> result = await base.ShouldTerminate(history, cancellationToken);
        if (!result.Value)
        {
            result = await this.GetResponseAsync<bool>(history, Prompts.Termination(topic), cancellationToken);
        }
        return result;
    }

    private async ValueTask<GroupChatManagerResult<TValue>> GetResponseAsync<TValue>(ChatHistory history, string prompt, CancellationToken cancellationToken = default)
    {
        OpenAIPromptExecutionSettings executionSettings = new() { ResponseFormat = typeof(GroupChatManagerResult<TValue>) };
        ChatHistory request = [.. history, new ChatMessageContent(AuthorRole.System, prompt)];
        ChatMessageContent response = await chatCompletion.GetChatMessageContentAsync(request, executionSettings, kernel: null, cancellationToken);
        string responseText = response.ToString();
        
        return
            JsonSerializer.Deserialize<GroupChatManagerResult<TValue>>(responseText) ??
            throw new InvalidOperationException($"Failed to parse response: {responseText}");
    }
}
