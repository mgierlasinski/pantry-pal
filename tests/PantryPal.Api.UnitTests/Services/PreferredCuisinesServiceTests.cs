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
    public async Task GetAllAsync_RepositoryThrowsException_ThrowsException()
    {
        // Arrange
        var expectedException = new Exception("Database connection failed");

        _mockRepository
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _service.GetAllAsync());
        Assert.Equal(expectedException, exception);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
