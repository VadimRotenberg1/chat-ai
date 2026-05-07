namespace WebApplication2.Options;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string AgentName { get; set; } = "assistant";

    public OpenAIOptions OpenAI { get; set; } = new();

    public GroqOptions Groq { get; set; } = new();
}

public sealed class OpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-4o-mini";
}

public sealed class GroqOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "llama-3.1-8b-instant";

    public string Endpoint { get; set; } = "https://api.groq.com/openai/v1";
}
