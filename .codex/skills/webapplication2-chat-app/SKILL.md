---
name: webapplication2-chat-app
description: Project-specific guidance for working on the WebApplication2 .NET 10 + Angular chat application. Use when Codex is asked to modify, debug, test, review, or extend this repository, especially chat streaming, SignalR events, Microsoft.Extensions.AI integration, provider configuration, app-specific agent prompt loading from Agents/{AgentName}/skills.md, Angular chat UI, or build/publish behavior.
---

# WebApplication2 Chat App

## Workflow

1. Inspect the relevant server and client files before editing.
2. Preserve the app's current split: ASP.NET Core backend in `WebApplication2/`, Angular frontend in `WebApplication2/ClientApp/`, tests in `WebApplication2.Tests/`.
3. Keep generated `bin/`, `obj/`, `wwwroot/`, coverage, and publish output out of intentional source changes unless the user explicitly asks for build artifacts.
4. Prefer small, focused changes with tests for chat behavior, prompt loading, provider configuration, and SignalR contract changes.
5. Run `dotnet test WebApplication2.sln -c Release` when Debug binaries are locked by a running app.

## Project Map

Read `references/project-map.md` when you need file ownership, validation commands, or details about the runtime agent prompt file.

## Runtime Agent Prompt

The app-specific prompt is not a Codex skill. It lives at:

```text
WebApplication2/Agents/{AgentName}/skills.md
```

`Ai:AgentName` in `appsettings.json` selects the folder. The current default is `assistant`.

When changing this behavior:

- Keep `Agents/**/*.md` included as content in `WebApplication2/WebApplication2.csproj`.
- Add or update tests proving the selected Markdown file is used as the system prompt.
- Treat missing or blank prompt files as a fallback path, not a hard failure, unless the user asks for strict validation.

## AI Provider Configuration

Provider setup is in `WebApplication2/Program.cs`; option shape is in `WebApplication2/Options/AiOptions.cs`.

- Prefer user secrets or environment variables for API keys.
- Do not introduce new committed secrets.
- Keep Groq compatible with the OpenAI-compatible endpoint path already used by the app.

## Frontend Notes

The Angular app is in `WebApplication2/ClientApp/`.

- Keep chat service changes aligned with backend contracts in `WebApplication2/Contracts/`.
- If SignalR event names or payload records change, update both backend constants/records and frontend listeners.
- Release builds run `npm install` and `npm run build` from `ClientApp`.

## Validation

Use the narrowest validation that covers the change:

```powershell
dotnet test WebApplication2.sln -c Release
```

For publish behavior:

```powershell
dotnet publish WebApplication2\WebApplication2.csproj -c Release -o artifacts\verify-publish
```

After publish checks, remove temporary verification output from `artifacts/` if it was created only for inspection.
