using Microsoft.Extensions.Logging;
using Moq;
using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;

namespace PantryPal.Api.UnitTests.Services;

/// <summary>
/// Unit tests for PantryService
/// </summary>
public class PantryServiceTests
{
    private readonly Mock<IPantryRepository> _mockRepository;
    private readonly Mock<ILogger<PantryService>> _mockLogger;
    private readonly PantryService _service;

    public PantryServiceTests()
    {
        _mockRepository = new Mock<IPantryRepository>();
        _mockLogger = new Mock<ILogger<PantryService>>();
        _service = new PantryService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetPantryItemsAsync_WithValidParameters_ReturnsCorrectResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 20;
        var sortField = "created_at";

        var mockItems = new List<PantryItemsSelect>
        {
            new() { Id = "1", Name = "Apples", IsFavorite = true, CreatedAt = "2024-01-01", UpdatedAt = "2024-01-01", UserId = userId.ToString() },
            new() { Id = "2", Name = "Bananas", IsFavorite = false, CreatedAt = "2024-01-02", UpdatedAt = "2024-01-02", UserId = userId.ToString() }
        };

        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField))
            .ReturnsAsync((mockItems, 2));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, pageSize, sortField);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(2, result.Total);
        
        var firstItem = result.Items.First();
        Assert.Equal("1", firstItem.Id);
        Assert.Equal("Apples", firstItem.Name);
        Assert.True(firstItem.IsFavorite);
        
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField), Times.Once);
    }

    [Fact]
    public async Task GetPantryItemsAsync_WithNameSort_ReturnsSortedItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 20;
        var sortField = "name";

        var mockItems = new List<PantryItemsSelect>
        {
            new() { Id = "1", Name = "Apples", IsFavorite = true, CreatedAt = "2024-01-01", UpdatedAt = "2024-01-01", UserId = userId.ToString() }
        };

        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField))
            .ReturnsAsync((mockItems, 1));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, pageSize, sortField);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.Total);
        
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField), Times.Once);
    }

    [Fact]
    public async Task GetPantryItemsAsync_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 20;
        var sortField = "created_at";

        var mockItems = new List<PantryItemsSelect>();

        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField))
            .ReturnsAsync((mockItems, 0));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, pageSize, sortField);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField), Times.Once);
    }

    [Fact]
    public async Task GetPantryItemsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 2;
        var pageSize = 10;
        var sortField = "name";

        var mockItems = new List<PantryItemsSelect>
        {
            new() { Id = "11", Name = "Item 11", IsFavorite = false, CreatedAt = "2024-01-11", UpdatedAt = "2024-01-11", UserId = userId.ToString() },
            new() { Id = "12", Name = "Item 12", IsFavorite = false, CreatedAt = "2024-01-12", UpdatedAt = "2024-01-12", UserId = userId.ToString() }
        };

        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField))
            .ReturnsAsync((mockItems, 25)); // Total of 25 items

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, pageSize, sortField);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.Total);
        
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField), Times.Once);
    }

    [Fact]
    public async Task GetPantryItemsAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 20;
        var sortField = "created_at";

        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _service.GetPantryItemsAsync(userId, page, pageSize, sortField));
        
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField), Times.Once);
    }

    [Fact]
    public async Task GetPantryItemsAsync_MapsAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 20;
        var sortField = "created_at";

        var mockItems = new List<PantryItemsSelect>
        {
            new() 
            { 
                Id = "test-id-123", 
                Name = "Test Item", 
                IsFavorite = true, 
                CreatedAt = "2024-01-15T10:30:00Z", 
                UpdatedAt = "2024-01-16T15:45:00Z", 
                UserId = userId.ToString() 
            }
        };

        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField))
            .ReturnsAsync((mockItems, 1));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, pageSize, sortField);

        // Assert
        var item = result.Items.First();
        Assert.Equal("test-id-123", item.Id);
        Assert.Equal("Test Item", item.Name);
        Assert.True(item.IsFavorite);
        Assert.Equal("2024-01-15T10:30:00Z", item.CreatedAt);
        Assert.Equal("2024-01-16T15:45:00Z", item.UpdatedAt);
    }

    [Fact]
    public async Task GetPantryItemsAsync_WithDifferentSortField_PassesCorrectParameter()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 20;
        var sortField = "name";

        var mockItems = new List<PantryItemsSelect>
        {
            new() { Id = "1", Name = "Apples", IsFavorite = false, CreatedAt = "2024-01-01", UpdatedAt = "2024-01-01", UserId = userId.ToString() }
        };

        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField))
            .ReturnsAsync((mockItems, 1));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, pageSize, sortField);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, pageSize, "name"), Times.Once);
    }

    [Fact]
    public async Task GetPantryItemsAsync_WithLargeDataset_HandlesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 100;
        var sortField = "created_at";

        var mockItems = Enumerable.Range(1, 100).Select(i => new PantryItemsSelect
        {
            Id = i.ToString(),
            Name = $"Item {i}",
            IsFavorite = i % 2 == 0,
            CreatedAt = $"2024-01-{i:D2}",
            UpdatedAt = $"2024-01-{i:D2}",
            UserId = userId.ToString()
        }).ToList();

        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField))
            .ReturnsAsync((mockItems, 1000)); // 1000 total items

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, pageSize, sortField);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Items.Count());
        Assert.Equal(1000, result.Total);
        Assert.Equal(100, result.PageSize);
    }
}

