using Microsoft.Extensions.Logging;
using Moq;
using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Services;

/// <summary>
/// Unit tests for PreferredCuisinesService
/// </summary>
public class PreferredCuisinesServiceTests
{
    private readonly Mock<IPreferredCuisinesRepository> _mockRepository;
    private readonly Mock<ILogger<PreferredCuisinesService>> _mockLogger;
    private readonly PreferredCuisinesService _service;

    public PreferredCuisinesServiceTests()
    {
        _mockRepository = new Mock<IPreferredCuisinesRepository>();
        _mockLogger = new Mock<ILogger<PreferredCuisinesService>>();
        _service = new PreferredCuisinesService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithValidPreferredCuisines_ReturnsCorrectResponse()
    {
        // Arrange
        var mockPreferredCuisines = new List<PreferredCuisinesSelect>
        {
            new() { Id = 1, Name = "Polish" },
            new() { Id = 2, Name = "Italian" },
            new() { Id = 3, Name = "Asian" },
            new() { Id = 4, Name = "Mexican" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockPreferredCuisines);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.PreferredCuisines.Count());

        var preferredCuisinesList = result.PreferredCuisines.ToList();
        Assert.Equal(1, preferredCuisinesList[0].Id);
        Assert.Equal("Polish", preferredCuisinesList[0].Name);
        Assert.Equal(2, preferredCuisinesList[1].Id);
        Assert.Equal("Italian", preferredCuisinesList[1].Name);
        Assert.Equal(3, preferredCuisinesList[2].Id);
        Assert.Equal("Asian", preferredCuisinesList[2].Name);
        Assert.Equal(4, preferredCuisinesList[3].Id);
        Assert.Equal("Mexican", preferredCuisinesList[3].Name);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var mockPreferredCuisines = new List<PreferredCuisinesSelect>();

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockPreferredCuisines);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.PreferredCuisines);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithInvalidPreferredCuisines_FiltersOutInvalidEntries()
    {
        // Arrange
        var mockPreferredCuisines = new List<PreferredCuisinesSelect>
        {
            new() { Id = 1, Name = "Polish" },
            new() { Id = 2, Name = "" }, // Invalid: empty name
            new() { Id = 3, Name = "   " }, // Invalid: whitespace only
            new() { Id = 4, Name = "Italian" } // Valid
        };

        // Add an item with null name using reflection
        var nullNameItem = new PreferredCuisinesSelect { Id = 5 };
        typeof(PreferredCuisinesSelect).GetProperty("Name")!.SetValue(nullNameItem, null);
        mockPreferredCuisines.Add(nullNameItem);

        mockPreferredCuisines.Add(new() { Id = 6, Name = "Asian" }); // Valid
        mockPreferredCuisines.Add(new() { Id = 7, Name = "Mexican" }); // Valid

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockPreferredCuisines);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.PreferredCuisines.Count()); // 4 valid entries

        var validIds = result.PreferredCuisines.Select(pc => pc.Id).ToList();
        Assert.Contains((short)1, validIds);
        Assert.Contains((short)4, validIds);
        Assert.Contains((short)6, validIds);
        Assert.Contains((short)7, validIds);

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
    public async Task GetAllAsync_WithWhitespaceNames_TrimsNames()
    {
        // Arrange
        var mockPreferredCuisines = new List<PreferredCuisinesSelect>
        {
            new() { Id = 1, Name = "  Polish  " },
            new() { Id = 2, Name = "\tItalian\n" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockPreferredCuisines);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.PreferredCuisines.Count());

        var preferredCuisinesList = result.PreferredCuisines.ToList();
        Assert.Equal("Polish", preferredCuisinesList[0].Name);
        Assert.Equal("Italian", preferredCuisinesList[1].Name);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_MapsAllFieldsCorrectly()
    {
        // Arrange
        var mockPreferredCuisines = new List<PreferredCuisinesSelect>
        {
            new() { Id = 100, Name = "test-cuisine" }
        };

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockPreferredCuisines);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        var preferredCuisine = result.PreferredCuisines.First();
        Assert.Equal(100, preferredCuisine.Id);
        Assert.Equal("test-cuisine", preferredCuisine.Name);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithLargeDataset_HandlesCorrectly()
    {
        // Arrange
        var mockPreferredCuisines = Enumerable.Range(1, 50).Select(i => new PreferredCuisinesSelect
        {
            Id = (short)i,
            Name = $"cuisine-{i}"
        }).ToList();

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockPreferredCuisines);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.PreferredCuisines.Count());

        for (int i = 0; i < 50; i++)
        {
            var preferredCuisine = result.PreferredCuisines.ElementAt(i);
            Assert.Equal(i + 1, preferredCuisine.Id);
            Assert.Equal($"cuisine-{i + 1}", preferredCuisine.Name);
        }

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithMixedValidInvalidData_FiltersCorrectly()
    {
        // Arrange
        var mockPreferredCuisines = new List<PreferredCuisinesSelect>
        {
            new() { Id = 1, Name = "valid1" },
            new() { Id = 3, Name = "valid2" },
            new() { Id = 4, Name = "" },
            new() { Id = 5, Name = "   " },
            new() { Id = 6, Name = "valid3" },
            new() { Id = 7, Name = "also valid" }
        };

        // Add an item with null name
        var nullNameItem = new PreferredCuisinesSelect { Id = 2 };
        typeof(PreferredCuisinesSelect).GetProperty("Name")!.SetValue(nullNameItem, null);
        mockPreferredCuisines.Insert(1, nullNameItem);

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(mockPreferredCuisines);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.PreferredCuisines.Count()); // 4 valid entries

        var validIds = result.PreferredCuisines.Select(pc => pc.Id).ToList();
        Assert.Contains((short)1, validIds);
        Assert.Contains((short)3, validIds);
        Assert.Contains((short)6, validIds);
        Assert.Contains((short)7, validIds);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
