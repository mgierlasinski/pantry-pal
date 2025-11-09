using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using PantryPal.Api.Services.OpenRouter;

namespace PantryPal.Api.UnitTests.Services.OpenRouter;

/// <summary>
/// Test fixture for OpenRouterService tests providing shared mock setup
/// </summary>
public class OpenRouterServiceTestFixture : IDisposable
{
    public Mock<IHttpClientFactory> MockHttpClientFactory { get; }
    public Mock<IOptions<OpenRouterOptions>> MockOptions { get; }
    public Mock<ILogger<OpenRouterService>> MockLogger { get; }
    public Mock<HttpMessageHandler> MockHttpMessageHandler { get; private set; }
    public OpenRouterService Service { get; private set; }

    public OpenRouterServiceTestFixture()
    {
        MockHttpClientFactory = new Mock<IHttpClientFactory>();
        MockOptions = new Mock<IOptions<OpenRouterOptions>>();
        MockLogger = new Mock<ILogger<OpenRouterService>>();
        MockHttpMessageHandler = new Mock<HttpMessageHandler>();

        // Setup default valid options
        MockOptions.Setup(o => o.Value).Returns(new OpenRouterOptions
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://openrouter.ai/api/v1",
            SiteName = "PantryPal",
            Model = "anthropic/claude-3-haiku"
        });

        SetupHttpClient();
        CreateService();
    }

    private void SetupHttpClient()
    {
        var httpClient = new HttpClient(MockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1")
        };

        MockHttpClientFactory
            .Setup(f => f.CreateClient(nameof(IOpenRouterService)))
            .Returns(httpClient);
    }

    private void CreateService()
    {
        Service = new OpenRouterService(
            MockHttpClientFactory.Object,
            MockOptions.Object,
            MockLogger.Object);
    }

    /// <summary>
    /// Sets up the HTTP handler to return a specific response
    /// </summary>
    public void SetupHttpResponse(HttpStatusCode statusCode, object? responseContent = null)
    {
        var response = new HttpResponseMessage(statusCode);

        if (responseContent != null)
        {
            string json;
            if (responseContent is string stringContent)
            {
                json = stringContent;
            }
            else
            {
                json = JsonSerializer.Serialize(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            Console.WriteLine($"Mock response JSON: {json}"); // Debug output
        }

        MockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    /// <summary>
    /// Sets up the HTTP handler to throw an exception
    /// </summary>
    public void SetupHttpException(Exception exception)
    {
        MockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);
    }

    /// <summary>
    /// Resets the HTTP handler to default state
    /// </summary>
    public void ResetHttpHandler()
    {
        MockHttpMessageHandler = new Mock<HttpMessageHandler>();
        SetupHttpClient();
        CreateService();
    }

    public void Dispose()
    {
        // No need to dispose Mock<HttpMessageHandler> - it will be cleaned up by GC
    }
}
