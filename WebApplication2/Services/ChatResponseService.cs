using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using WebApplication2.Contracts;
using WebApplication2.Hubs;

namespace WebApplication2.Services;

public sealed class ChatResponseService(
    IHubContext<ChatHub> chatHub,
    IServiceProvider services,
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

        var messages = new[]
        {
            new ChatMessage(ChatRole.System, "You are a concise, helpful assistant."),
            new ChatMessage(ChatRole.User, request.Message.Trim())
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
