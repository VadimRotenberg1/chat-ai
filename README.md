# WebApplication2 AI Chat Skeleton

This project is a minimal .NET 10 web app with:

- Static frontend in `wwwroot`
- Rich message composer based on `contenteditable`
- Minimal API endpoint at `POST /api/chat`
- SignalR hub at `/chatHub`
- LLM access through `Microsoft.Extensions.AI.IChatClient`
- OpenAI adapter through `Microsoft.Extensions.AI.OpenAI`

## Configure

Keep the API key out of `appsettings.json`. Use user secrets for local development:

```powershell
cd .\WebApplication2
dotnet user-secrets set "Ai:OpenAI:ApiKey" "<your-api-key>"
dotnet user-secrets set "Ai:OpenAI:Model" "gpt-4.1-mini"
```

`appsettings.json` contains the non-secret default model name.

## Run

```powershell
dotnet run --project .\WebApplication2\WebApplication2.csproj
```

Open the URL printed by `dotnet run`, usually `http://localhost:5042`.

## Current Flow

1. Browser starts a SignalR connection to `/chatHub`.
2. User submits a message to `POST /api/chat` with the current SignalR `connectionId`.
3. `ChatResponseService` calls `IChatClient.GetStreamingResponseAsync`.
4. Each streamed chunk is sent back to that browser connection with SignalR events.
5. The frontend appends chunks into the assistant message bubble.

## Next Steps

- Persist conversation history per user/session.
- Add authentication and map SignalR messages by user instead of raw connection id.
- Add markdown rendering and sanitization if assistant answers should be formatted.
- Add request cancellation for long-running responses.
- Add provider selection if you need more than OpenAI.
