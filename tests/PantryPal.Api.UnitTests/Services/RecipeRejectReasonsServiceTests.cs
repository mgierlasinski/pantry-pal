using Microsoft.Extensions.Logging;
using Moq;
using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Services;

/// <summary>
/// Unit tests for RecipeRejectReasonsService
/// </summary>
public class RecipeRejectReasonsServiceTests
{
    private readonly Mock<IRecipeRejectReasonsRepository> _mockRepository;
    private readonly Mock<ILogger<RecipeRejectReasonsService>> _mockLogger;
    private readonly RecipeRejectReasonsService _service;

    public RecipeRejectReasonsServiceTests()
    {
        _mockRepository = new Mock<IRecipeRejectReasonsRepository>();
        _mockLogger = new Mock<ILogger<RecipeRejectReasonsService>>();
        _service = new RecipeRejectReasonsService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithValidRejectReasons_ReturnsCorrectDtos()
    {
        // Arrange
        var mockRejectReasons = new List<RecipeRejectReasonsSelect>
        {
            new() { Id = 1, Description = "I don't have these ingredients" },
            new() { Id = 2, Description = "I don't like this dish" },
            new() { Id = 3, Description = "Other" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockRejectReasons);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());

        var reasonsList = result.ToList();
        Assert.Equal((short)1, reasonsList[0].Id);
        Assert.Equal("I don't have these ingredients", reasonsList[0].Description);
        Assert.Equal((short)2, reasonsList[1].Id);
        Assert.Equal("I don't like this dish", reasonsList[1].Description);
        Assert.Equal((short)3, reasonsList[2].Id);
        Assert.Equal("Other", reasonsList[2].Description);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRejectReasons = new List<RecipeRejectReasonsSelect>();

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockRejectReasons);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllAsync());
        Assert.Contains("Configuration error: No reject reasons found", exception.Message);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetAllAsync());

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_MapsAllFieldsCorrectly()
    {
        // Arrange
        var mockRejectReasons = new List<RecipeRejectReasonsSelect>
        {
            new() { Id = 100, Description = "Custom reject reason" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockRejectReasons);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        var rejectReason = result.First();
        Assert.Equal((short)100, rejectReason.Id);
        Assert.Equal("Custom reject reason", rejectReason.Description);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithLargeDataset_HandlesCorrectly()
    {
        // Arrange
        var mockRejectReasons = Enumerable.Range(1, 10).Select(i => new RecipeRejectReasonsSelect
        {
            Id = (short)i,
            Description = $"Reject reason {i}"
        }).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockRejectReasons);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Count());

        for (int i = 0; i < 10; i++)
        {
            var rejectReason = result.ElementAt(i);
            Assert.Equal((short)(i + 1), rejectReason.Id);
            Assert.Equal($"Reject reason {i + 1}", rejectReason.Description);
        }

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithSingleRejectReason_ReturnsSingleDto()
    {
        // Arrange
        var mockRejectReasons = new List<RecipeRejectReasonsSelect>
        {
            new() { Id = 42, Description = "Single reason" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockRejectReasons);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        var rejectReason = result.First();
        Assert.Equal((short)42, rejectReason.Id);
        Assert.Equal("Single reason", rejectReason.Description);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
