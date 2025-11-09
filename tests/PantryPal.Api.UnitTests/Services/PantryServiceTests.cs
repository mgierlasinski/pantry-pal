using Microsoft.Extensions.Logging;
using Moq;
using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;
using PantryPal.Data;

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

    [Theory]
    [InlineData("created_at")]
    [InlineData("name")]
    [InlineData("updated_at")]
    public async Task GetPantryItemsAsync_ValidParameters_ReturnsCorrectResponse(string sortField)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 20;

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
        Assert.Equal(page, result.Page);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(2, result.Total);

        var firstItem = result.Items.First();
        Assert.Equal("1", firstItem.Id);
        Assert.Equal("Apples", firstItem.Name);
        Assert.True(firstItem.IsFavorite);

        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField), Times.Once);
    }

    [Fact]
    public async Task GetPantryItemsAsync_EmptyResult_ReturnsEmptyList()
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
        Assert.Equal(page, result.Page);
        Assert.Equal(pageSize, result.PageSize);

        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField), Times.Once);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 5)]
    [InlineData(3, 20)]
    public async Task GetPantryItemsAsync_Pagination_ReturnsCorrectPageData(int page, int pageSize)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sortField = "name";
        var totalItems = 25;

        var mockItems = new List<PantryItemsSelect>();
        for (int i = 1; i <= pageSize; i++)
        {
            var itemNumber = ((page - 1) * pageSize) + i;
            mockItems.Add(new PantryItemsSelect
            {
                Id = itemNumber.ToString(),
                Name = $"Item {itemNumber}",
                IsFavorite = false,
                CreatedAt = $"2024-01-{itemNumber:D2}",
                UpdatedAt = $"2024-01-{itemNumber:D2}",
                UserId = userId.ToString()
            });
        }

        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField))
            .ReturnsAsync((mockItems, totalItems));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, pageSize, sortField);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pageSize, result.Items.Count());
        Assert.Equal(page, result.Page);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(totalItems, result.Total);

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

    // ================================
    // CreatePantryItemAsync Tests
    // ================================

    [Fact]
    public async Task CreatePantryItemAsync_ValidData_ReturnsCreatedItem()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new PantryItemCreateDto("Test Item");
        var expectedCreatedItem = new PantryItemsSelect
        {
            Id = "generated-id-123",
            Name = "Test Item",
            IsFavorite = false,
            CreatedAt = "2024-01-15T10:30:00Z",
            UpdatedAt = "2024-01-15T10:30:00Z",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.CreatePantryItemAsync(It.Is<PantryItemsInsert>(p =>
                p.UserId == userId.ToString() &&
                p.Name == "Test Item" &&
                p.IsFavorite == false)))
            .ReturnsAsync(expectedCreatedItem);

        // Act
        var result = await _service.CreatePantryItemAsync(userId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("generated-id-123", result.Id);
        Assert.Equal("Test Item", result.Name);
        Assert.False(result.IsFavorite);
        Assert.Equal("2024-01-15T10:30:00Z", result.CreatedAt);
        Assert.Equal("2024-01-15T10:30:00Z", result.UpdatedAt);

        _mockRepository.Verify(r => r.CreatePantryItemAsync(It.IsAny<PantryItemsInsert>()), Times.Once);
    }

    [Fact]
    public async Task CreatePantryItemAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new PantryItemCreateDto("Test Item");

        _mockRepository
            .Setup(r => r.CreatePantryItemAsync(It.IsAny<PantryItemsInsert>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.CreatePantryItemAsync(userId, createDto));

        _mockRepository.Verify(r => r.CreatePantryItemAsync(It.IsAny<PantryItemsInsert>()), Times.Once);
    }

    [Theory]
    [InlineData("Apple")]
    [InlineData("Banana")]
    [InlineData("Organic Free-Range Chicken Breast")]
    public async Task CreatePantryItemAsync_DifferentNames_SetsNameCorrectly(string itemName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new PantryItemCreateDto(itemName);
        var expectedCreatedItem = new PantryItemsSelect
        {
            Id = "test-id",
            Name = itemName,
            IsFavorite = false,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.CreatePantryItemAsync(It.Is<PantryItemsInsert>(p => p.Name == itemName)))
            .ReturnsAsync(expectedCreatedItem);

        // Act
        var result = await _service.CreatePantryItemAsync(userId, createDto);

        // Assert
        Assert.Equal(itemName, result.Name);
        _mockRepository.Verify(r => r.CreatePantryItemAsync(It.IsAny<PantryItemsInsert>()), Times.Once);
    }

    [Fact]
    public async Task CreatePantryItemAsync_DefaultFavoriteValue_IsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createDto = new PantryItemCreateDto("Test Item");
        var expectedCreatedItem = new PantryItemsSelect
        {
            Id = "test-id",
            Name = "Test Item",
            IsFavorite = false,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.CreatePantryItemAsync(It.Is<PantryItemsInsert>(p => p.IsFavorite == false)))
            .ReturnsAsync(expectedCreatedItem);

        // Act
        var result = await _service.CreatePantryItemAsync(userId, createDto);

        // Assert
        Assert.False(result.IsFavorite);
        _mockRepository.Verify(r => r.CreatePantryItemAsync(It.IsAny<PantryItemsInsert>()), Times.Once);
    }

    // ================================
    // UpdatePantryItemAsync Tests
    // ================================

    [Fact]
    public async Task UpdatePantryItemAsync_ValidData_ReturnsUpdatedItem()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateDto = new PantryItemUpdateDto(Name: "Updated Name", IsFavorite: true);
        var expectedUpdatedItem = new PantryItemsSelect
        {
            Id = itemId.ToString(),
            Name = "Updated Name",
            IsFavorite = true,
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-15T10:30:00Z",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.UpdatePantryItemAsync(It.Is<PantryItemsUpdate>(u =>
                u.Id == itemId.ToString() &&
                u.UserId == userId.ToString() &&
                u.Name == "Updated Name" &&
                u.IsFavorite == true)))
            .ReturnsAsync(expectedUpdatedItem);

        // Act
        var result = await _service.UpdatePantryItemAsync(itemId, userId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(itemId.ToString(), result.Id);
        Assert.Equal("Updated Name", result.Name);
        Assert.True(result.IsFavorite);
        Assert.Equal("2024-01-01T00:00:00Z", result.CreatedAt);
        Assert.Equal("2024-01-15T10:30:00Z", result.UpdatedAt);

        _mockRepository.Verify(r => r.UpdatePantryItemAsync(It.IsAny<PantryItemsUpdate>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateDto = new PantryItemUpdateDto(Name: "Updated Name");

        _mockRepository
            .Setup(r => r.UpdatePantryItemAsync(It.IsAny<PantryItemsUpdate>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.UpdatePantryItemAsync(itemId, userId, updateDto));

        _mockRepository.Verify(r => r.UpdatePantryItemAsync(It.IsAny<PantryItemsUpdate>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_NameOnly_UpdatesNameOnly()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateDto = new PantryItemUpdateDto(Name: "New Name");
        var expectedUpdatedItem = new PantryItemsSelect
        {
            Id = itemId.ToString(),
            Name = "New Name",
            IsFavorite = false, // Original value
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-02",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.UpdatePantryItemAsync(It.Is<PantryItemsUpdate>(u =>
                u.Name == "New Name" && u.IsFavorite == null)))
            .ReturnsAsync(expectedUpdatedItem);

        // Act
        var result = await _service.UpdatePantryItemAsync(itemId, userId, updateDto);

        // Assert
        Assert.Equal("New Name", result.Name);
        Assert.False(result.IsFavorite); // Should remain unchanged
        _mockRepository.Verify(r => r.UpdatePantryItemAsync(It.IsAny<PantryItemsUpdate>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_IsFavoriteOnly_UpdatesFavoriteOnly()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateDto = new PantryItemUpdateDto(IsFavorite: true);
        var expectedUpdatedItem = new PantryItemsSelect
        {
            Id = itemId.ToString(),
            Name = "Original Name", // Original value
            IsFavorite = true,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-02",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.UpdatePantryItemAsync(It.Is<PantryItemsUpdate>(u =>
                u.IsFavorite == true && u.Name == null)))
            .ReturnsAsync(expectedUpdatedItem);

        // Act
        var result = await _service.UpdatePantryItemAsync(itemId, userId, updateDto);

        // Assert
        Assert.Equal("Original Name", result.Name); // Should remain unchanged
        Assert.True(result.IsFavorite);
        _mockRepository.Verify(r => r.UpdatePantryItemAsync(It.IsAny<PantryItemsUpdate>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_EmptyUpdateDto_DoesNotChangeValues()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateDto = new PantryItemUpdateDto(); // Empty update
        var expectedUpdatedItem = new PantryItemsSelect
        {
            Id = itemId.ToString(),
            Name = "Original Name",
            IsFavorite = false,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-02",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.UpdatePantryItemAsync(It.Is<PantryItemsUpdate>(u =>
                u.Name == null && u.IsFavorite == null)))
            .ReturnsAsync(expectedUpdatedItem);

        // Act
        var result = await _service.UpdatePantryItemAsync(itemId, userId, updateDto);

        // Assert
        Assert.Equal("Original Name", result.Name);
        Assert.False(result.IsFavorite);
        _mockRepository.Verify(r => r.UpdatePantryItemAsync(It.IsAny<PantryItemsUpdate>()), Times.Once);
    }

    // ================================
    // DeletePantryItemAsync Tests
    // ================================

    [Fact]
    public async Task DeletePantryItemAsync_ValidItem_DeletesSuccessfully()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.DeletePantryItemAsync(itemId, userId))
            .ReturnsAsync(1); // One row affected

        // Act
        await _service.DeletePantryItemAsync(itemId, userId);

        // Assert
        _mockRepository.Verify(r => r.DeletePantryItemAsync(itemId, userId), Times.Once);
    }

    [Fact]
    public async Task DeletePantryItemAsync_ItemNotFound_ThrowsArgumentException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.DeletePantryItemAsync(itemId, userId))
            .ReturnsAsync(0); // No rows affected

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.DeletePantryItemAsync(itemId, userId));

        Assert.Contains("not found", exception.Message.ToLower());
        _mockRepository.Verify(r => r.DeletePantryItemAsync(itemId, userId), Times.Once);
    }

    [Fact]
    public async Task DeletePantryItemAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.DeletePantryItemAsync(itemId, userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.DeletePantryItemAsync(itemId, userId));

        _mockRepository.Verify(r => r.DeletePantryItemAsync(itemId, userId), Times.Once);
    }

    [Fact]
    public async Task DeletePantryItemAsync_ZeroRowsAffected_ThrowsArgumentException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.DeletePantryItemAsync(itemId, userId))
            .ReturnsAsync(0); // Zero rows affected means item not found

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.DeletePantryItemAsync(itemId, userId));

        Assert.Contains("not found", exception.Message.ToLower());
        _mockRepository.Verify(r => r.DeletePantryItemAsync(itemId, userId), Times.Once);
    }

    [Fact]
    public async Task DeletePantryItemAsync_ValidDeletion_NoExceptionThrown()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.DeletePantryItemAsync(itemId, userId))
            .ReturnsAsync(1);

        // Act & Assert - Should not throw any exception
        await _service.DeletePantryItemAsync(itemId, userId);

        _mockRepository.Verify(r => r.DeletePantryItemAsync(itemId, userId), Times.Once);
    }

    // ================================
    // Input Validation Tests
    // ================================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreatePantryItemAsync_InvalidNames_StillProcesses(string invalidName)
    {
        // Arrange - Service doesn't validate input, validation is at endpoint level
        var userId = Guid.NewGuid();
        var createDto = new PantryItemCreateDto(invalidName!);
        var expectedCreatedItem = new PantryItemsSelect
        {
            Id = "test-id",
            Name = invalidName!,
            IsFavorite = false,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.CreatePantryItemAsync(It.Is<PantryItemsInsert>(p =>
                p.UserId == userId.ToString() && p.Name == invalidName)))
            .ReturnsAsync(expectedCreatedItem);

        // Act
        var result = await _service.CreatePantryItemAsync(userId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(invalidName, result.Name);
        _mockRepository.Verify(r => r.CreatePantryItemAsync(It.IsAny<PantryItemsInsert>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetPantryItemsAsync_InvalidPageNumbers_StillProcesses(int invalidPage)
    {
        // Arrange - Service doesn't validate pagination, validation is at endpoint level
        var userId = Guid.NewGuid();
        var pageSize = 20;
        var sortField = "created_at";

        var mockItems = new List<PantryItemsSelect>();
        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, invalidPage, pageSize, sortField))
            .ReturnsAsync((mockItems, 0));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, invalidPage, pageSize, sortField);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(invalidPage, result.Page);
        Assert.Equal(pageSize, result.PageSize);
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, invalidPage, pageSize, sortField), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(-100)]
    public async Task GetPantryItemsAsync_InvalidPageSizes_StillProcesses(int invalidPageSize)
    {
        // Arrange - Service doesn't validate pagination, validation is at endpoint level
        var userId = Guid.NewGuid();
        var page = 1;
        var sortField = "created_at";

        var mockItems = new List<PantryItemsSelect>();
        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, invalidPageSize, sortField))
            .ReturnsAsync((mockItems, 0));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, invalidPageSize, sortField);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(page, result.Page);
        Assert.Equal(invalidPageSize, result.PageSize);
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, invalidPageSize, sortField), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid_field")]
    [InlineData("nonexistent_column")]
    public async Task GetPantryItemsAsync_InvalidSortFields_StillProcesses(string invalidSortField)
    {
        // Arrange - Service doesn't validate sort field, validation is at endpoint level
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 20;

        var mockItems = new List<PantryItemsSelect>();
        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, pageSize, invalidSortField!))
            .ReturnsAsync((mockItems, 0));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, pageSize, invalidSortField!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, pageSize, invalidSortField!), Times.Once);
    }

    [Fact]
    public async Task GetPantryItemsAsync_EmptyGuid_StillProcesses()
    {
        // Arrange
        var userId = Guid.Empty; // Empty GUID
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
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, pageSize, sortField), Times.Once);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_EmptyGuids_StillProcesses()
    {
        // Arrange
        var itemId = Guid.Empty;
        var userId = Guid.Empty;
        var updateDto = new PantryItemUpdateDto(Name: "Updated Name");

        var expectedUpdatedItem = new PantryItemsSelect
        {
            Id = itemId.ToString(),
            Name = "Updated Name",
            IsFavorite = false,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-02",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.UpdatePantryItemAsync(It.Is<PantryItemsUpdate>(u =>
                u.Id == itemId.ToString() && u.UserId == userId.ToString())))
            .ReturnsAsync(expectedUpdatedItem);

        // Act
        var result = await _service.UpdatePantryItemAsync(itemId, userId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        _mockRepository.Verify(r => r.UpdatePantryItemAsync(It.IsAny<PantryItemsUpdate>()), Times.Once);
    }

    [Fact]
    public async Task DeletePantryItemAsync_EmptyGuids_StillProcesses()
    {
        // Arrange
        var itemId = Guid.Empty;
        var userId = Guid.Empty;

        _mockRepository
            .Setup(r => r.DeletePantryItemAsync(itemId, userId))
            .ReturnsAsync(1);

        // Act
        await _service.DeletePantryItemAsync(itemId, userId);

        // Assert
        _mockRepository.Verify(r => r.DeletePantryItemAsync(itemId, userId), Times.Once);
    }

    [Fact]
    public async Task CreatePantryItemAsync_SpecialCharactersInName_HandlesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var specialName = "Apples & Bananas (Fresh!) - 2 lbs";
        var createDto = new PantryItemCreateDto(specialName);

        var expectedCreatedItem = new PantryItemsSelect
        {
            Id = "test-id",
            Name = specialName,
            IsFavorite = false,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.CreatePantryItemAsync(It.Is<PantryItemsInsert>(p => p.Name == specialName)))
            .ReturnsAsync(expectedCreatedItem);

        // Act
        var result = await _service.CreatePantryItemAsync(userId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(specialName, result.Name);
        _mockRepository.Verify(r => r.CreatePantryItemAsync(It.IsAny<PantryItemsInsert>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePantryItemAsync_AllNullValues_NoChangesApplied()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var updateDto = new PantryItemUpdateDto(); // All null values

        var expectedUpdatedItem = new PantryItemsSelect
        {
            Id = itemId.ToString(),
            Name = "Original Name",
            IsFavorite = false,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
            UserId = userId.ToString()
        };

        _mockRepository
            .Setup(r => r.UpdatePantryItemAsync(It.Is<PantryItemsUpdate>(u =>
                u.Name == null && u.IsFavorite == null)))
            .ReturnsAsync(expectedUpdatedItem);

        // Act
        var result = await _service.UpdatePantryItemAsync(itemId, userId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Original Name", result.Name);
        Assert.False(result.IsFavorite);
        _mockRepository.Verify(r => r.UpdatePantryItemAsync(It.IsAny<PantryItemsUpdate>()), Times.Once);
    }

    [Theory]
    [InlineData(1000)] // Very large page size
    [InlineData(10000)]
    public async Task GetPantryItemsAsync_LargePageSizes_HandlesCorrectly(int largePageSize)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var sortField = "created_at";

        var mockItems = new List<PantryItemsSelect>();
        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, page, largePageSize, sortField))
            .ReturnsAsync((mockItems, 0));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, page, largePageSize, sortField);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(largePageSize, result.PageSize);
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, page, largePageSize, sortField), Times.Once);
    }

    [Theory]
    [InlineData(1000)] // Very large page number
    [InlineData(10000)]
    public async Task GetPantryItemsAsync_LargePageNumbers_HandlesCorrectly(int largePageNumber)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pageSize = 20;
        var sortField = "created_at";

        var mockItems = new List<PantryItemsSelect>();
        _mockRepository
            .Setup(r => r.GetPantryItemsAsync(userId, largePageNumber, pageSize, sortField))
            .ReturnsAsync((mockItems, 0));

        // Act
        var result = await _service.GetPantryItemsAsync(userId, largePageNumber, pageSize, sortField);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(largePageNumber, result.Page);
        _mockRepository.Verify(r => r.GetPantryItemsAsync(userId, largePageNumber, pageSize, sortField), Times.Once);
    }
}

