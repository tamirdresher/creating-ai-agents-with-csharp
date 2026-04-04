/*
 * 05-A2AProtocol — Microsoft Agent Framework GA (v1.0.0)
 *
 * Demonstrates the Agent-to-Agent (A2A) protocol — a standard for connecting
 * to remote agents regardless of what framework they are built with.
 *
 * A2A lets you:
 *   - Discover a remote agent's capabilities via its Agent Card
 *   - Send messages and receive streaming responses
 *   - Build multi-agent systems that span different services / processes
 *
 * Prerequisites:
 *   - Set A2A_AGENT_URL in .env to a running A2A-compatible server
 *   - If you have none, start 09-AgentWithAspire which exposes an A2A endpoint
 *
 * TODO: verify A2A package API against latest A2A SDK — see MIGRATION.md
 */

using A2A;
using DotNetEnv;

// ── Config ─────────────────────────────────────────────────────────────────────
Env.TraversePath().Load();

var agentUrl = new Uri(
    Environment.GetEnvironmentVariable("A2A_AGENT_URL") ?? "http://localhost:5000");

Console.WriteLine($"=== 05-A2AProtocol ===");
Console.WriteLine($"Connecting to remote agent at: {agentUrl}\n");

// ── Discover the remote agent via its Agent Card ──────────────────────────────
//
// The Agent Card (served at /.well-known/agent.json) describes the remote agent:
// its name, capabilities, supported input/output modes, and authentication.
var httpClient    = new HttpClient();
var cardResolver  = new A2ACardResolver(agentUrl, httpClient);
var agentCard     = await cardResolver.GetAgentCardAsync();

Console.WriteLine($"Connected to: {agentCard.Name}");
Console.WriteLine($"Description:  {agentCard.Description}");
Console.WriteLine($"Version:      {agentCard.Version}\n");

// ── Create the A2A client ─────────────────────────────────────────────────────
//
// A2AClient handles the HTTP protocol for sending messages and receiving responses.
// NOTE: There is no A2AAgent wrapper in A2A 0.3.3-preview — use A2AClient directly.
var a2aClient = new A2AClient(agentUrl, httpClient);

// ── Chat loop ──────────────────────────────────────────────────────────────────
Console.WriteLine("(type 'exit' to quit)\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    Console.Write($"[{agentCard.Name}]: ");

    // Build the A2A message with the user's text
    var userMsg = new AgentMessage
    {
        Role  = MessageRole.User,
        Parts = [new TextPart { Text = input }]
    };

    // SendMessageAsync sends a message to the remote agent and waits for the
    // final response. The result is either an AgentTask (with Status.Message)
    // or a direct AgentMessage when the agent replies without creating a task.
    var response = await a2aClient.SendMessageAsync(
        new MessageSendParams { Message = userMsg });

    // Extract the reply message from the response discriminated union
    AgentMessage? reply = response switch
    {
        AgentTask task   => task.Status.Message,
        AgentMessage msg => msg,
        _                => null
    };

    if (reply?.Parts is { } parts)
        foreach (var part in parts)
            if (part.AsTextPart() is TextPart textPart)
                Console.Write(textPart.Text);

    Console.WriteLine("\n");
}

Console.WriteLine("Goodbye!");
