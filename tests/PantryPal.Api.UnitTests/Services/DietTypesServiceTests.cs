using Microsoft.Extensions.Logging;
using Moq;
using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Services;

/// <summary>
/// Unit tests for DietTypesService
/// </summary>
public class DietTypesServiceTests
{
    private readonly Mock<IDietTypesRepository> _mockRepository;
    private readonly Mock<ILogger<DietTypesService>> _mockLogger;
    private readonly DietTypesService _service;

    public DietTypesServiceTests()
    {
        _mockRepository = new Mock<IDietTypesRepository>();
        _mockLogger = new Mock<ILogger<DietTypesService>>();
        _service = new DietTypesService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithValidDietTypes_ReturnsCorrectResponse()
    {
        // Arrange
        var mockDietTypes = new List<DietTypesSelect>
        {
            new() { Id = 1, Name = "standard" },
            new() { Id = 2, Name = "vegetarian" },
            new() { Id = 3, Name = "vegan" },
            new() { Id = 4, Name = "gluten-free" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockDietTypes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.DietTypes.Count());

        var dietTypesList = result.DietTypes.ToList();
        Assert.Equal(1, dietTypesList[0].Id);
        Assert.Equal("standard", dietTypesList[0].Name);
        Assert.Equal(2, dietTypesList[1].Id);
        Assert.Equal("vegetarian", dietTypesList[1].Name);
        Assert.Equal(3, dietTypesList[2].Id);
        Assert.Equal("vegan", dietTypesList[2].Name);
        Assert.Equal(4, dietTypesList[3].Id);
        Assert.Equal("gluten-free", dietTypesList[3].Name);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var mockDietTypes = new List<DietTypesSelect>();

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockDietTypes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.DietTypes);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithInvalidDietTypes_FiltersOutInvalidEntries()
    {
        // Arrange
        var mockDietTypes = new List<DietTypesSelect>
        {
            new() { Id = 1, Name = "standard" },
            new() { Id = 3, Name = "" }, // Invalid: empty name
            new() { Id = 4, Name = "   " }, // Invalid: whitespace only
            new() { Id = 5, Name = "vegetarian" } // Valid
        };

        // Add an item with null name using reflection or by setting it after creation
        var nullNameItem = new DietTypesSelect { Id = 2 };
        typeof(DietTypesSelect).GetProperty("Name")!.SetValue(nullNameItem, null);
        mockDietTypes.Insert(1, nullNameItem);

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockDietTypes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.DietTypes.Count()); // Only 2 valid entries

        var dietTypesList = result.DietTypes.ToList();
        Assert.Equal(1, dietTypesList[0].Id);
        Assert.Equal("standard", dietTypesList[0].Name);
        Assert.Equal(5, dietTypesList[1].Id);
        Assert.Equal("vegetarian", dietTypesList[1].Name);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithWhitespaceNames_TrimsNames()
    {
        // Arrange
        var mockDietTypes = new List<DietTypesSelect>
        {
            new() { Id = 1, Name = "  standard  " },
            new() { Id = 2, Name = "\tvegetarian\n" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockDietTypes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.DietTypes.Count());

        var dietTypesList = result.DietTypes.ToList();
        Assert.Equal("standard", dietTypesList[0].Name);
        Assert.Equal("vegetarian", dietTypesList[1].Name);

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
        var mockDietTypes = new List<DietTypesSelect>
        {
            new() { Id = 100, Name = "test-diet-type" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockDietTypes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        var dietType = result.DietTypes.First();
        Assert.Equal(100, dietType.Id);
        Assert.Equal("test-diet-type", dietType.Name);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithLargeDataset_HandlesCorrectly()
    {
        // Arrange
        var mockDietTypes = Enumerable.Range(1, 50).Select(i => new DietTypesSelect
        {
            Id = (short)i,
            Name = $"diet-type-{i}"
        }).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockDietTypes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.DietTypes.Count());

        for (int i = 0; i < 50; i++)
        {
            var dietType = result.DietTypes.ElementAt(i);
            Assert.Equal(i + 1, dietType.Id);
            Assert.Equal($"diet-type-{i + 1}", dietType.Name);
        }

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithMixedValidInvalidData_FiltersCorrectly()
    {
        // Arrange
        var mockDietTypes = new List<DietTypesSelect>
        {
            new() { Id = 1, Name = "valid1" },
            new() { Id = 3, Name = "valid2" },
            new() { Id = 4, Name = "" },
            new() { Id = 5, Name = "   " },
            new() { Id = 6, Name = "valid3" },
            new() { Id = 7, Name = "also valid" }
        };

        // Add an item with null name
        var nullNameItem = new DietTypesSelect { Id = 2 };
        typeof(DietTypesSelect).GetProperty("Name")!.SetValue(nullNameItem, null);
        mockDietTypes.Insert(1, nullNameItem);

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockDietTypes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.DietTypes.Count()); // 4 valid entries

        var validIds = result.DietTypes.Select(dt => dt.Id).ToList();
        Assert.Contains((short)1, validIds);
        Assert.Contains((short)3, validIds);
        Assert.Contains((short)6, validIds);
        Assert.Contains((short)7, validIds);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
