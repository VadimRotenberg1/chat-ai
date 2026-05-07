using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using WebApplication2.Contracts;
using WebApplication2.Hubs;
using WebApplication2.Options;

namespace WebApplication2.Services;

public sealed class ChatResponseService(
    IHubContext<ChatHub> chatHub,
    IServiceProvider services,
    IWebHostEnvironment environment,
    IOptions<AiOptions> aiOptions,
    ILogger<ChatResponseService> logger)
{
    public async Task SendAnswerAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        var responseId = Guid.NewGuid().ToString("N");
        var client = chatHub.Clients.Client(request.ConnectionId);

        await client.SendAsync(
            ChatClientEvents.AssistantStarted,
            new AssistantStartedMessage(request.ConversationId, responseId),
            cancellationToken);

        var chatClient = services.GetService<IChatClient>();
        if (chatClient is null)
        {
            await client.SendAsync(
                ChatClientEvents.AssistantError,
                new AssistantErrorMessage(request.ConversationId, responseId, "LLM is not configured. Set Ai:Groq:ApiKey or Ai:OpenAI:ApiKey first."),
                cancellationToken);

            return;
        }

        var systemPrompt = "You are a concise, helpful assistant.";
        var agentName = aiOptions.Value.AgentName ?? "assistant";
        var skillsPath = Path.Combine(environment.ContentRootPath, "Agents", agentName, "skills.md");
        if (File.Exists(skillsPath))
        {
            try
            {
                var skills = await File.ReadAllTextAsync(skillsPath, cancellationToken);
                if (!string.IsNullOrWhiteSpace(skills))
                {
                    systemPrompt = skills;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to read skills.md from {Path}", skillsPath);
            }
        }

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, request.Message.Trim())
        };

        try
        {
            await foreach (var update in chatClient.GetStreamingResponseAsync(messages, cancellationToken: cancellationToken))
            {
                if (string.IsNullOrWhiteSpace(update.Text))
                {
                    continue;
                }

                await client.SendAsync(
                    ChatClientEvents.AssistantChunk,
                    new AssistantChunkMessage(request.ConversationId, responseId, update.Text),
                    cancellationToken);
            }

            await client.SendAsync(
                ChatClientEvents.AssistantCompleted,
                new AssistantCompletedMessage(request.ConversationId, responseId),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LLM chat request failed: {Message}", ex.Message);

            await client.SendAsync(
                ChatClientEvents.AssistantError,
                new AssistantErrorMessage(request.ConversationId, responseId, "The assistant failed to produce a response."),
                cancellationToken);
        }
    }
}
