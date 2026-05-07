# Project Map

## Server

- `WebApplication2/Program.cs`: ASP.NET Core startup, SignalR registration, AI provider registration, HTTP endpoints, request validation.
- `WebApplication2/Services/ChatResponseService.cs`: Loads the app-specific agent prompt, sends streaming responses, maps failures to SignalR events.
- `WebApplication2/Options/AiOptions.cs`: `Ai` configuration shape, including `AgentName`, `OpenAI`, and `Groq`.
- `WebApplication2/Contracts/`: Shared request, response, and SignalR event contract names.
- `WebApplication2/Hubs/ChatHub.cs`: SignalR hub endpoint implementation.
- `WebApplication2/Agents/assistant/skills.md`: Runtime system prompt for the configured assistant agent.
- `WebApplication2/WebApplication2.csproj`: Package references, content-copy rules, and Release frontend build target.

## Client

- `WebApplication2/ClientApp/src/app/services/chat.ts`: Frontend chat and SignalR service.
- `WebApplication2/ClientApp/src/app/app.ts`: Main Angular component logic.
- `WebApplication2/ClientApp/src/app/app.html`: Main Angular template.
- `WebApplication2/ClientApp/src/app/app.css`: Main Angular styles.
- `WebApplication2/ClientApp/package.json`: Frontend scripts and dependencies.

## Tests

- `WebApplication2.Tests/ChatResponseServiceTests.cs`: Unit tests for chat streaming and runtime prompt loading.

## Important Behavior

The app loads exactly one runtime prompt file per configured agent:

```text
ContentRootPath/Agents/{Ai:AgentName}/skills.md
```

This is separate from Codex/OpenAI-style skills, which use:

```text
<skill-name>/SKILL.md
<skill-name>/agents/openai.yaml
```

## Validation Commands

Run backend tests:

```powershell
dotnet test WebApplication2.sln -c Release
```

Run publish verification:

```powershell
dotnet publish WebApplication2\WebApplication2.csproj -c Release -o artifacts\verify-publish
```

Check whether runtime prompt files are copied:

```powershell
Test-Path artifacts\verify-publish\Agents\assistant\skills.md
```

## Known Cautions

- Debug binaries may be locked when the app is already running from Rider or `dotnet run`; use Release validation or stop the process.
- `appsettings.json` must not contain real API keys in committed changes.
- Release builds regenerate frontend output in `WebApplication2/wwwroot/`.
