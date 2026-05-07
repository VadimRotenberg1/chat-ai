using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using WebApplication2.Contracts;
using WebApplication2.Hubs;
using WebApplication2.Options;
using WebApplication2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddProblemDetails();
builder.Services.Configure<AiOptions>(builder.Configuration.GetSection(AiOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<ChatResponseService>();
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<TokenService>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) || jwtOptions.SigningKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SigningKey must be configured with at least 32 characters. Set it in appsettings.json or user-secrets.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        // Allow SignalR clients to send the token as ?access_token=... on the negotiate/WS request.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub").RequireAuthorization();

app.MapPost("/api/auth/login", (LoginRequest request, UserStore users, TokenService tokens) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { error = "Username and password are required." });
    }

    var profile = users.Authenticate(request.Username, request.Password);
    if (profile is null)
    {
        return Results.Unauthorized();
    }

    var (token, expiresIn) = tokens.CreateAccessToken(profile);
    return Results.Ok(new LoginResponse(token, "Bearer", expiresIn, profile));
})
.AllowAnonymous();

app.MapGet("/api/user/info", (ClaimsPrincipal user, UserStore users) =>
{
    var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(id))
    {
        return Results.Unauthorized();
    }

    var profile = users.FindById(id);
    return profile is null ? Results.Unauthorized() : Results.Ok(profile);
})
.RequireAuthorization();

app.MapPost("/api/chat", async (ChatRequest request, ChatResponseService chat, CancellationToken cancellationToken) =>
{
    var validationError = Validate(request);
    if (validationError is not null)
    {
        return Results.BadRequest(new { error = validationError });
    }

    await chat.SendAnswerAsync(request, cancellationToken);
    return Results.Accepted();
})
.RequireAuthorization();

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
