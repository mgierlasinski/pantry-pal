using Microsoft.Extensions.Options;
using PantryPal.Api.Services.OpenRouter;
using Polly;

namespace PantryPal.Api.Extensions;

public static class OpenRouterExtensions
{
    public static void AddOpenRouter(this IServiceCollection services, IConfigurationManager configuration)
    {
        // Configure OpenRouter options
        services.Configure<OpenRouterOptions>(configuration.GetSection(OpenRouterOptions.SectionName));

        // Configure OpenRouter HttpClient with retry policy
        services.AddHttpClient<IOpenRouterService, OpenRouterService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenRouterOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new("Bearer", options.ApiKey);
            client.DefaultRequestHeaders.Add("HTTP-Referer", options.SiteName);
            client.DefaultRequestHeaders.Add("X-Title", "PantryPal");
        })
        .AddPolicyHandler(Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response => !response.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        // Register OpenRouter service
        services.AddScoped<IOpenRouterService, OpenRouterService>();
    }
}
