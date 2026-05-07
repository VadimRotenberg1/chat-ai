namespace WebApplication2.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "WebApplication2";

    public string Audience { get; set; } = "WebApplication2.Client";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; set; } = 60;
}
