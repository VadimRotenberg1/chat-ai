namespace WebApplication2.Contracts;

public sealed record LoginRequest(string? Username, string? Password);

public sealed record LoginResponse(string AccessToken, string TokenType, int ExpiresIn, UserProfile User);

public sealed record UserProfile(string Id, string Username, string DisplayName, string Email, string[] Roles);
