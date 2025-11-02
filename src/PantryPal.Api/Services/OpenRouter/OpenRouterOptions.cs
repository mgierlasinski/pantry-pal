namespace PantryPal.Api.Services.OpenRouter;

public class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    public required string ApiKey { get; set; }
    public required string BaseUrl { get; set; }
    public required string SiteName { get; set; }
    public required string Model { get; set; }
}
