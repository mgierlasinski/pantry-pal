using Moq;
using Moq.Protected;
using PantryPal.Data;
using PantryPal.Mobile.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PantryPal.Mobile.UnitTests.Services;

public class PantryServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly PantryService _service;

    public PantryServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _service = new PantryService(_httpClient);
    }

    [Fact]
    public async Task GetPantryItemsAsync_Success_ReturnsItems()
    {
        // Arrange
        var mockItems = new List<PantryItemDto>
        {
            new("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString()),
            new("2", "Onions", true, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new PantryItemsPaginatedResponseDto(mockItems, 1, 20, 2);
        var json = JsonSerializer.Serialize(response);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.GetPantryItemsAsync(1, 20, "name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.Total);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task GetPantryItemsAsync_EmptyList_ReturnsEmptyCollection()
    {
        // Arrange
        var response = new PantryItemsPaginatedResponseDto([], 1, 20, 0);
        var json = JsonSerializer.Serialize(response);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.GetPantryItemsAsync(1, 20, "name");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task CreatePantryItemAsync_Success_ReturnsCreatedItem()
    {
        // Arrange
        var createDto = new PantryItemCreateDto("Carrots");
        var createdItem = new PantryItemDto("3", "Carrots", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var json = JsonSerializer.Serialize(createdItem);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.CreatePantryItemAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("3", result.Id);
        Assert.Equal("Carrots", result.Name);
        Assert.False(result.IsFavorite);
    }

    [Fact]
    public async Task CreatePantryItemAsync_Conflict_ThrowsHttpRequestException()
    {
        // Arrange
        var createDto = new PantryItemCreateDto("Duplicate");

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict,
                Content = new StringContent("Duplicate item", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.CreatePantryItemAsync(createDto));
    }

    [Fact]
    public async Task UpdatePantryItemAsync_Success_ReturnsUpdatedItem()
    {
        // Arrange
        var updateDto = new PantryItemUpdateDto(Name: "Cherry Tomatoes");
        var updatedItem = new PantryItemDto("1", "Cherry Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var json = JsonSerializer.Serialize(updatedItem);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Patch),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.UpdatePantryItemAsync("1", updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.Equal("Cherry Tomatoes", result.Name);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_FavoriteToggle_ReturnsUpdatedItem()
    {
        // Arrange
        var updateDto = new PantryItemUpdateDto(IsFavorite: true);
        var updatedItem = new PantryItemDto("1", "Tomatoes", true, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var json = JsonSerializer.Serialize(updatedItem);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Patch),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.UpdatePantryItemAsync("1", updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.True(result.IsFavorite);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_NotFound_ThrowsHttpRequestException()
    {
        // Arrange
        var updateDto = new PantryItemUpdateDto(Name: "NonExistent");

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Patch),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.UpdatePantryItemAsync("999", updateDto));
    }

    [Fact]
    public async Task DeletePantryItemAsync_Success_CompletesWithoutException()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        await _service.DeletePantryItemAsync("1");

        // Assert - No exception thrown
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Delete &&
                req.RequestUri!.ToString().Contains("1")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task DeletePantryItemAsync_NotFound_ThrowsHttpRequestException()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.DeletePantryItemAsync("999"));
    }

    [Fact]
    public async Task GetPantryItemsAsync_Unauthorized_ThrowsHttpRequestException()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.GetPantryItemsAsync(1, 20, "name"));
        
        Assert.NotNull(exception);
    }
}

