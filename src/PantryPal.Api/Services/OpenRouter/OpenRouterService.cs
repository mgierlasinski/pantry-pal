using Microsoft.Extensions.Options;
using PantryPal.Api.Services.OpenRouter.Dto;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PantryPal.Api.Services.OpenRouter;

public class OpenRouterService : IOpenRouterService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterService> _logger;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenRouterService(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenRouterOptions> options,
        ILogger<OpenRouterService> logger)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(IOpenRouterService));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate required configuration
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ArgumentNullException(nameof(_options.ApiKey), "OpenRouter API key is required");
        }
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new ArgumentNullException(nameof(_options.BaseUrl), "OpenRouter base URL is required");
        }
        if (string.IsNullOrWhiteSpace(_options.Model))
        {
            throw new ArgumentNullException(nameof(_options.Model), "OpenRouter model is required");
        }
    }

    public async Task<TResponse?> GetStructuredResponseAsync<TResponse>(
        string systemMessage,
        string userMessage,
        object jsonSchema,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting structured response request for type {ResponseType}", typeof(TResponse).Name);

            var requestPayload = BuildRequestPayload(systemMessage, userMessage, jsonSchema);
            var response = await SendRequestAsync(requestPayload, cancellationToken);

            if (response == null || !response.Choices.Any())
            {
                _logger.LogWarning("No choices returned in API response");
                return default;
            }

            var choice = response.Choices.First();
            var content = choice.Message.Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Empty content in API response message");
                return default;
            }

            return ParseStructuredResponse<TResponse>(content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while calling OpenRouter API");
            return default;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization/deserialization failed");
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while calling OpenRouter API");
            return default;
        }
    }

    private ChatRequest BuildRequestPayload(string systemMessage, string userMessage, object jsonSchema)
    {
        var messages = new List<ChatMessage>
        {
            new("system", systemMessage),
            new("user", userMessage)
        };

        var responseFormat = new JsonResponseFormat(
            "json_schema",
            new JsonSchemaObject("response", true, jsonSchema)
        );

        return new ChatRequest(_options.Model, messages, responseFormat);
    }

    private async Task<ChatResponse?> SendRequestAsync(ChatRequest requestPayload, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v1/chat/completions", requestPayload, _jsonSerializerOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenRouter API returned non-success status code: {StatusCode}", response.StatusCode);

            // Try to read error response
            try
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("API error response: {ErrorContent}", errorContent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read error response content");
            }

            response.EnsureSuccessStatusCode(); // This will throw
        }

        return await response.Content.ReadFromJsonAsync<ChatResponse>(_jsonSerializerOptions, cancellationToken);
    }

    private TResponse? ParseStructuredResponse<TResponse>(string content)
    {
        try
        {
            var result = JsonSerializer.Deserialize<TResponse>(content, _jsonSerializerOptions);
            _logger.LogInformation("Successfully parsed structured response for type {ResponseType}", typeof(TResponse).Name);
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize content into {ResponseType}. Content: {Content}", typeof(TResponse).Name, content);
            return default;
        }
    }
}
