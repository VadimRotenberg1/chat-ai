namespace WebApplication2.Contracts;

public sealed record ChatRequest(string ConnectionId, string Message, string ConversationId);
