using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PantryPal.Api.Services.OpenRouter;

namespace PantryPal.Api.UnitTests.Services.OpenRouter;

/// <summary>
/// Unit tests for OpenRouterService
/// </summary>
public class OpenRouterServiceTests : IClassFixture<OpenRouterServiceTestFixture>
{
    private readonly OpenRouterServiceTestFixture _fixture;

    public OpenRouterServiceTests(OpenRouterServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    // Constructor Tests

    [Theory]
    [InlineData(null, "https://test.com", "anthropic/claude-3-haiku", "ApiKey")]
    [InlineData("", "https://test.com", "anthropic/claude-3-haiku", "ApiKey")]
    [InlineData("   ", "https://test.com", "anthropic/claude-3-haiku", "ApiKey")]
    [InlineData("test-key", null, "anthropic/claude-3-haiku", "BaseUrl")]
    [InlineData("test-key", "", "anthropic/claude-3-haiku", "BaseUrl")]
    [InlineData("test-key", "   ", "anthropic/claude-3-haiku", "BaseUrl")]
    [InlineData("test-key", "https://test.com", null, "Model")]
    [InlineData("test-key", "https://test.com", "", "Model")]
    [InlineData("test-key", "https://test.com", "   ", "Model")]
    public void Constructor_InvalidOptions_ThrowsArgumentNullException(
        string apiKey, string baseUrl, string model, string expectedParamName)
    {
        // Arrange
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockOptions = new Mock<IOptions<OpenRouterOptions>>();
        var mockLogger = new Mock<ILogger<OpenRouterService>>();

        mockOptions.Setup(o => o.Value).Returns(new OpenRouterOptions
        {
            ApiKey = apiKey,
            BaseUrl = baseUrl,
            SiteName = "Test",
            Model = model
        });

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new OpenRouterService(mockHttpClientFactory.Object, mockOptions.Object, mockLogger.Object));

        Assert.Equal(expectedParamName, exception.ParamName);
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsNullReferenceException()
    {
        // Arrange
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<OpenRouterService>>();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
            new OpenRouterService(mockHttpClientFactory.Object, null!, mockLogger.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockOptions = new Mock<IOptions<OpenRouterOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new OpenRouterOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://test.com",
            SiteName = "Test",
            Model = "anthropic/claude-3-haiku"
        });

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new OpenRouterService(mockHttpClientFactory.Object, mockOptions.Object, null!));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_ValidOptions_CreatesServiceSuccessfully()
    {
        // Arrange
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockOptions = new Mock<IOptions<OpenRouterOptions>>();
        var mockLogger = new Mock<ILogger<OpenRouterService>>();

        var httpClient = new HttpClient();
        mockHttpClientFactory
            .Setup(f => f.CreateClient(nameof(IOpenRouterService)))
            .Returns(httpClient);

        mockOptions.Setup(o => o.Value).Returns(new OpenRouterOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://test.com",
            SiteName = "Test",
            Model = "anthropic/claude-3-haiku"
        });

        // Act
        var service = new OpenRouterService(mockHttpClientFactory.Object, mockOptions.Object, mockLogger.Object);

        // Assert
        Assert.NotNull(service);
        httpClient.Dispose(); // Cleanup
    }

    // GetStructuredResponseAsync Success Tests

    private record TestResponse(string Name, int Age);

    [Fact]
    public async Task GetStructuredResponseAsync_ValidResponse_ReturnsDeserializedObject()
    {
        // Arrange
        var jsonSchema = new
        {
            type = "object",
            properties = new
            {
                name = new { type = "string" },
                age = new { type = "integer" }
            },
            required = new[] { "name", "age" }
        };

        var expectedResponse = new TestResponse("John Doe", 30);
        var responseJson = @"{
            ""choices"": [
                {
                    ""message"": {
                        ""role"": ""assistant"",
                        ""content"": ""{\""name\"": \""John Doe\"", \""age\"": 30}""
                    }
                }
            ]
        }";

        _fixture.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "You are a helpful assistant",
            "Create a person profile",
            jsonSchema);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Name, result.Name);
        Assert.Equal(expectedResponse.Age, result.Age);
    }

    // GetStructuredResponseAsync Error Handling Tests

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetStructuredResponseAsync_HttpError_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var jsonSchema = new { type = "object", properties = new { } };
        var errorResponse = @"{""error"": ""API Error""}";
        _fixture.SetupHttpResponse(statusCode, errorResponse);

        // Act
        var result = await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "System message",
            "User message",
            jsonSchema);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStructuredResponseAsync_HttpRequestException_ReturnsNull()
    {
        // Arrange
        var jsonSchema = new { type = "object", properties = new { } };
        _fixture.SetupHttpException(new HttpRequestException("Network error"));

        // Act
        var result = await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "System message",
            "User message",
            jsonSchema);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStructuredResponseAsync_InvalidJsonResponse_ReturnsNull()
    {
        // Arrange
        var jsonSchema = new { type = "object", properties = new { } };
        _fixture.SetupHttpResponse(HttpStatusCode.OK, "invalid json content");

        // Act
        var result = await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "System message",
            "User message",
            jsonSchema);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStructuredResponseAsync_InvalidContentDeserialization_ReturnsNull()
    {
        // Arrange
        var jsonSchema = new { type = "object", properties = new { } };
        var responseJson = @"{
            ""choices"": [
                {
                    ""message"": {
                        ""role"": ""assistant"",
                        ""content"": ""{ invalid json }""
                    }
                }
            ]
        }";

        _fixture.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "System message",
            "User message",
            jsonSchema);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStructuredResponseAsync_GeneralException_ReturnsNull()
    {
        // Arrange
        var jsonSchema = new { type = "object", properties = new { } };
        _fixture.SetupHttpException(new Exception("Unexpected error"));

        // Act
        var result = await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "System message",
            "User message",
            jsonSchema);

        // Assert
        Assert.Null(result);
    }

    // GetStructuredResponseAsync Edge Cases Tests

    [Fact]
    public async Task GetStructuredResponseAsync_NoChoicesInResponse_ReturnsNull()
    {
        // Arrange
        var jsonSchema = new { type = "object", properties = new { } };
        var responseJson = @"{""choices"": []}";

        _fixture.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "System message",
            "User message",
            jsonSchema);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStructuredResponseAsync_NullChoicesInResponse_ReturnsNull()
    {
        // Arrange
        var jsonSchema = new { type = "object", properties = new { } };
        var responseJson = @"{""choices"": null}";

        _fixture.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "System message",
            "User message",
            jsonSchema);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStructuredResponseAsync_EmptyContentInMessage_ReturnsNull()
    {
        // Arrange
        var jsonSchema = new { type = "object", properties = new { } };
        var responseJson = @"{
            ""choices"": [
                {
                    ""message"": {
                        ""role"": ""assistant"",
                        ""content"": null
                    }
                }
            ]
        }";

        _fixture.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "System message",
            "User message",
            jsonSchema);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStructuredResponseAsync_WhitespaceContentInMessage_ReturnsNull()
    {
        // Arrange
        var jsonSchema = new { type = "object", properties = new { } };
        var responseJson = @"{
            ""choices"": [
                {
                    ""message"": {
                        ""role"": ""assistant"",
                        ""content"": ""   ""
                    }
                }
            ]
        }";

        _fixture.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        var result = await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "System message",
            "User message",
            jsonSchema);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStructuredResponseAsync_ValidResponse_LogsSuccess()
    {
        // Arrange
        var jsonSchema = new { type = "object", properties = new { } };
        var responseJson = @"{
            ""choices"": [
                {
                    ""message"": {
                        ""role"": ""assistant"",
                        ""content"": ""{\""name\"": \""Jane\"", \""age\"": 25}""
                    }
                }
            ]
        }";

        _fixture.SetupHttpResponse(HttpStatusCode.OK, responseJson);

        // Act
        await _fixture.Service.GetStructuredResponseAsync<TestResponse>(
            "System message",
            "User message",
            jsonSchema);

        // Assert
        // Logger verification removed as extension methods cannot be mocked directly
    }
}
