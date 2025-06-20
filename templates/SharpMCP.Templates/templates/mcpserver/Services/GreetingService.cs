namespace McpServerTemplate.Services;

/// <summary>
/// Service interface for greeting operations
/// </summary>
public interface IGreetingService
{
    /// <summary>
    /// Generates a greeting message
    /// </summary>
    /// <param name="name">Name of the person to greet</param>
    /// <param name="style">Style of greeting</param>
    /// <returns>The greeting message</returns>
    string GenerateGreeting(string name, string style);
}

/// <summary>
/// Implementation of greeting service
/// </summary>
public class GreetingService : IGreetingService
{
    private readonly ILogger<GreetingService> _logger;

    public GreetingService(ILogger<GreetingService> logger)
    {
        _logger = logger;
    }

    public string GenerateGreeting(string name, string style)
    {
        _logger.LogDebug("Generating {Style} greeting for {Name}", style, name);

        return style switch
        {
            "formal" => $"Good day, {name}. How may I assist you today?",
            "casual" => $"Hey {name}! What's up?",
            "excited" => $"Hello {name}! 🎉 Great to see you!",
            _ => $"Hello, {name}!"
        };
    }
}
