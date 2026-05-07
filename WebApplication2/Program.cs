using Microsoft.Extensions.AI;
using WebApplication2.Contracts;
using WebApplication2.Hubs;
using WebApplication2.Options;
using WebApplication2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.Configure<AiOptions>(builder.Configuration.GetSection(AiOptions.SectionName));
builder.Services.AddScoped<ChatResponseService>();

var aiOptions = builder.Configuration.GetSection(AiOptions.SectionName).Get<AiOptions>() ?? new AiOptions();

if (!string.IsNullOrWhiteSpace(aiOptions.Groq.ApiKey))
{
    builder.Services.AddChatClient(_ =>
        new OpenAI.Chat.ChatClient(aiOptions.Groq.Model, new System.ClientModel.ApiKeyCredential(aiOptions.Groq.ApiKey), new OpenAI.OpenAIClientOptions
        {
            Endpoint = new Uri(aiOptions.Groq.Endpoint)
        }).AsIChatClient());
}
else if (!string.IsNullOrWhiteSpace(aiOptions.OpenAI.ApiKey))
{
    builder.Services.AddChatClient(_ =>
        new OpenAI.Chat.ChatClient(aiOptions.OpenAI.Model, aiOptions.OpenAI.ApiKey).AsIChatClient());
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chatHub");

app.MapPost("/api/chat", async (ChatRequest request, ChatResponseService chat, CancellationToken cancellationToken) =>
{
    var validationError = Validate(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { error = validationError });
    }

    await chat.SendAnswerAsync(request, cancellationToken);
    return Results.Accepted();
});

app.Run();

static string? Validate(ChatRequest request)
{
    if (string.IsNullOrWhiteSpace(request.ConnectionId))
    {
        return "SignalR connectionId is required.";
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
