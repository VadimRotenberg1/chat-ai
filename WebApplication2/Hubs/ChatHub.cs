using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebApplication2.Contracts;
using WebApplication2.Services;

namespace WebApplication2.Hubs;

[Authorize]
public sealed class ChatHub(ChatResponseService chat) : Hub
{
    public Task SendMessage(ChatRequest request, CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
        {
            throw new HubException(validationError);
        }

        return chat.SendAnswerAsync(Clients.Caller, request, cancellationToken);
    }

    private static string? Validate(ChatRequest request)
    {
        if (request is null)
        {
            return "Request payload is required.";
        }

        if (string.IsNullOrWhiteSpace(request.ConversationId))
        {
            return "ConversationId is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return "Message is required.";
        }

        return request.Message.Length > 8_000
            ? "Message is too long. Keep it under 8,000 characters."
            : null;
    }
}
