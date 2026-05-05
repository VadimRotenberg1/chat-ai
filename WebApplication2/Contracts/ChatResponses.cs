namespace WebApplication2.Contracts;

public sealed record AssistantStartedMessage(string ConversationId, string ResponseId);

public sealed record AssistantChunkMessage(string ConversationId, string ResponseId, string Text);

public sealed record AssistantCompletedMessage(string ConversationId, string ResponseId);

public sealed record AssistantErrorMessage(string ConversationId, string ResponseId, string Error);
