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
    public async Task GetAllAsync_WithValidData_ReturnsMappedDtos()
    {
        // Arrange
        var mockEntities = new List<RecipeRejectReasonsSelect>
        {
            new() { Id = 1, Description = "Contains allergens" },
            new() { Id = 2, Description = "Does not match dietary preferences" },
            new() { Id = 3, Description = "Missing required ingredients" },
            new() { Id = 4, Description = "Cooking time too long" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockEntities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(4, resultList.Count);

        Assert.Equal(1, resultList[0].Id);
        Assert.Equal("Contains allergens", resultList[0].Description);
        Assert.Equal(2, resultList[1].Id);
        Assert.Equal("Does not match dietary preferences", resultList[1].Description);
        Assert.Equal(3, resultList[2].Id);
        Assert.Equal("Missing required ingredients", resultList[2].Description);
        Assert.Equal(4, resultList[3].Id);
        Assert.Equal("Cooking time too long", resultList[3].Description);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepositoryResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockEntities = new List<RecipeRejectReasonsSelect>();

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockEntities);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetAllAsync());

        Assert.Contains("Configuration error: No reject reasons found", exception.Message);
        Assert.IsType<InvalidOperationException>(exception);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_SingleEntity_ReturnsSingleDto()
    {
        // Arrange
        var mockEntities = new List<RecipeRejectReasonsSelect>
        {
            new() { Id = 5, Description = "Other reason" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockEntities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(5, resultList[0].Id);
        Assert.Equal("Other reason", resultList[0].Description);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_LargeDataset_HandlesCorrectly()
    {
        // Arrange
        var mockEntities = Enumerable.Range(1, 50).Select(i => new RecipeRejectReasonsSelect
        {
            Id = (short)i,
            Description = $"Reject reason number {i}"
        }).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockEntities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(50, resultList.Count);

        for (int i = 0; i < 50; i++)
        {
            Assert.Equal(i + 1, resultList[i].Id);
            Assert.Equal($"Reject reason number {i + 1}", resultList[i].Description);
        }

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var testException = new Exception("Database connection failed");

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(testException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.GetAllAsync());

        Assert.Equal(testException, exception);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_SpecialCharactersInDescriptions_HandlesCorrectly()
    {
        // Arrange
        var mockEntities = new List<RecipeRejectReasonsSelect>
        {
            new() { Id = 1, Description = "Contains: nuts, dairy & gluten" },
            new() { Id = 2, Description = "Too expensive (£50+)" },
            new() { Id = 3, Description = "Complex prep (45+ minutes)" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockEntities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);
        Assert.Equal("Contains: nuts, dairy & gluten", resultList[0].Description);
        Assert.Equal("Too expensive (£50+)", resultList[1].Description);
        Assert.Equal("Complex prep (45+ minutes)", resultList[2].Description);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_LogsInformationOnSuccess()
    {
        // Arrange
        var mockEntities = new List<RecipeRejectReasonsSelect>
        {
            new() { Id = 1, Description = "Test reason 1" },
            new() { Id = 2, Description = "Test reason 2" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockEntities);

        // Act
        await _service.GetAllAsync();

        // Assert - Verify success logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Successfully retrieved")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_LogsErrorOnEmptyResult()
    {
        // Arrange
        var mockEntities = new List<RecipeRejectReasonsSelect>();

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockEntities);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetAllAsync());

        // Assert - Verify error logging for empty result
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("No reject reasons found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_LogsErrorOnException()
    {
        // Arrange
        var testException = new Exception("Database error");

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(testException);

        // Act
        await Assert.ThrowsAsync<Exception>(() =>
            _service.GetAllAsync());

        // Assert - Verify error logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Failed to retrieve reject reasons")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    public async Task GetAllAsync_VariousEntityCounts_ReturnsCorrectCount(int count)
    {
        // Arrange
        var mockEntities = Enumerable.Range(1, count).Select(i => new RecipeRejectReasonsSelect
        {
            Id = (short)i,
            Description = $"Reason {i}"
        }).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockEntities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count());

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }
}