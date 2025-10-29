using Microsoft.Extensions.Logging;
using Moq;
using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Services;

/// <summary>
/// Unit tests for UserPreferencesService
/// </summary>
public class UserPreferencesServiceTests
{
    private readonly Mock<IUserPreferencesRepository> _mockRepository;
    private readonly Mock<ILogger<UserPreferencesService>> _mockLogger;
    private readonly UserPreferencesService _service;

    public UserPreferencesServiceTests()
    {
        _mockRepository = new Mock<IUserPreferencesRepository>();
        _mockLogger = new Mock<ILogger<UserPreferencesService>>();
        _service = new UserPreferencesService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_WithValidUser_ReturnsPreferencesDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypeId = 1,
            PreferredCuisineId = 2,
            DislikedIngredients = "nuts, shellfish",
            CreatedAt = "2024-10-29T10:00:00Z",
            UpdatedAt = "2024-10-29T10:00:00Z",
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegetarian" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = 2, Name = "Italian" }
        };

        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockPreferences);

        // Act
        var result = await _service.GetUserPreferencesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId.ToString(), result.UserId);
        Assert.Equal(1, result.DietTypeId);
        Assert.Equal("Vegetarian", result.DietTypeName);
        Assert.Equal(2, result.PreferredCuisineId);
        Assert.Equal("Italian", result.PreferredCuisineName);
        Assert.Equal("nuts, shellfish", result.DislikedIngredients);
        Assert.Equal("2024-10-29T10:00:00Z", result.CreatedAt);
        Assert.Equal("2024-10-29T10:00:00Z", result.UpdatedAt);

        _mockRepository.Verify(r => r.GetUserPreferencesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_WithNonexistentUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync((UserPreferencesSelect?)null);

        // Act
        var result = await _service.GetUserPreferencesAsync(userId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetUserPreferencesAsync(userId), Times.Once);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_WithValidData_ReturnsPreferencesDto()
    {
        // Arrange
        var userId = "test-user-123";
        var dto = new UserPreferencesCreateDto(1, 2, "nuts, shellfish");

        var upsertedRecord = new UserPreferencesSelect
        {
            UserId = userId,
            DietTypeId = 1,
            PreferredCuisineId = 2,
            DislikedIngredients = "nuts, shellfish",
            CreatedAt = "2024-10-29T10:00:00Z",
            UpdatedAt = "2024-10-29T10:00:00Z",
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegetarian" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = 2, Name = "Italian" }
        };

        _mockRepository
            .Setup(r => r.DietTypeExistsAsync(1))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.PreferredCuisineExistsAsync(2))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.UpsertUserPreferencesAsync(userId, 1, 2, "nuts, shellfish"))
            .ReturnsAsync(upsertedRecord);
        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(Guid.Parse(userId)))
            .ReturnsAsync(upsertedRecord);

        // Act
        var result = await _service.UpsertPreferencesAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(1, result.DietTypeId);
        Assert.Equal("Vegetarian", result.DietTypeName);
        Assert.Equal(2, result.PreferredCuisineId);
        Assert.Equal("Italian", result.PreferredCuisineName);
        Assert.Equal("nuts, shellfish", result.DislikedIngredients);

        _mockRepository.Verify(r => r.DietTypeExistsAsync(1), Times.Once);
        _mockRepository.Verify(r => r.PreferredCuisineExistsAsync(2), Times.Once);
        _mockRepository.Verify(r => r.UpsertUserPreferencesAsync(userId, 1, 2, "nuts, shellfish"), Times.Once);
        _mockRepository.Verify(r => r.GetUserPreferencesAsync(Guid.Parse(userId)), Times.Once);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_WithNullDislikedIngredients_ReturnsPreferencesDto()
    {
        // Arrange
        var userId = "test-user-123";
        var dto = new UserPreferencesCreateDto(1, 2, null);

        var upsertedRecord = new UserPreferencesSelect
        {
            UserId = userId,
            DietTypeId = 1,
            PreferredCuisineId = 2,
            DislikedIngredients = null,
            CreatedAt = "2024-10-29T10:00:00Z",
            UpdatedAt = "2024-10-29T10:00:00Z",
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegetarian" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = 2, Name = "Italian" }
        };

        _mockRepository
            .Setup(r => r.DietTypeExistsAsync(1))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.PreferredCuisineExistsAsync(2))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.UpsertUserPreferencesAsync(userId, 1, 2, null))
            .ReturnsAsync(upsertedRecord);
        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(Guid.Parse(userId)))
            .ReturnsAsync(upsertedRecord);

        // Act
        var result = await _service.UpsertPreferencesAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Null(result.DislikedIngredients);

        _mockRepository.Verify(r => r.UpsertUserPreferencesAsync(userId, 1, 2, null), Times.Once);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_WithInvalidDietTypeId_ThrowsArgumentException()
    {
        // Arrange
        var userId = "test-user-123";
        var dto = new UserPreferencesCreateDto(999, 2, "nuts");

        _mockRepository
            .Setup(r => r.DietTypeExistsAsync(999))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpsertPreferencesAsync(dto, userId));

        Assert.Contains("Diet type with ID 999 does not exist", exception.Message);
        Assert.Equal("dto.DietTypeId", exception.ParamName);

        _mockRepository.Verify(r => r.DietTypeExistsAsync(999), Times.Once);
        _mockRepository.Verify(r => r.PreferredCuisineExistsAsync(It.IsAny<short>()), Times.Never);
        _mockRepository.Verify(r => r.UpsertUserPreferencesAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<short>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_WithInvalidPreferredCuisineId_ThrowsArgumentException()
    {
        // Arrange
        var userId = "test-user-123";
        var dto = new UserPreferencesCreateDto(1, 999, "nuts");

        _mockRepository
            .Setup(r => r.DietTypeExistsAsync(1))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.PreferredCuisineExistsAsync(999))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpsertPreferencesAsync(dto, userId));

        Assert.Contains("Preferred cuisine with ID 999 does not exist", exception.Message);
        Assert.Equal("dto.PreferredCuisineId", exception.ParamName);

        _mockRepository.Verify(r => r.DietTypeExistsAsync(1), Times.Once);
        _mockRepository.Verify(r => r.PreferredCuisineExistsAsync(999), Times.Once);
        _mockRepository.Verify(r => r.UpsertUserPreferencesAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<short>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_WithNullUserId_ThrowsArgumentException()
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(1, 2, "nuts");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpsertPreferencesAsync(dto, null!));

        Assert.Contains("User ID cannot be null or empty", exception.Message);
        Assert.Equal("userId", exception.ParamName);

        _mockRepository.Verify(r => r.DietTypeExistsAsync(It.IsAny<short>()), Times.Never);
        _mockRepository.Verify(r => r.PreferredCuisineExistsAsync(It.IsAny<short>()), Times.Never);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(1, 2, "nuts");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpsertPreferencesAsync(dto, ""));

        Assert.Contains("User ID cannot be null or empty", exception.Message);
        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_WithNullDto_ThrowsArgumentNullException()
    {
        // Arrange
        var userId = "test-user-123";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.UpsertPreferencesAsync(null!, userId));

        Assert.Contains("User preferences DTO cannot be null", exception.Message);
        Assert.Equal("dto", exception.ParamName);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var userId = "test-user-123";
        var dto = new UserPreferencesCreateDto(1, 2, "nuts");

        _mockRepository
            .Setup(r => r.DietTypeExistsAsync(1))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.PreferredCuisineExistsAsync(2))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.UpsertUserPreferencesAsync(userId, 1, 2, "nuts"))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.UpsertPreferencesAsync(dto, userId));

        _mockRepository.Verify(r => r.UpsertUserPreferencesAsync(userId, 1, 2, "nuts"), Times.Once);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_WhenGetUserPreferencesThrowsException_PropagatesException()
    {
        // Arrange
        var userId = "test-user-123";
        var dto = new UserPreferencesCreateDto(1, 2, "nuts");

        var upsertedRecord = new UserPreferencesSelect
        {
            UserId = userId,
            DietTypeId = 1,
            PreferredCuisineId = 2,
            DislikedIngredients = "nuts"
        };

        _mockRepository
            .Setup(r => r.DietTypeExistsAsync(1))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.PreferredCuisineExistsAsync(2))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.UpsertUserPreferencesAsync(userId, 1, 2, "nuts"))
            .ReturnsAsync(upsertedRecord);
        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(Guid.Parse(userId)))
            .ThrowsAsync(new Exception("Failed to retrieve preferences"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.UpsertPreferencesAsync(dto, userId));

        _mockRepository.Verify(r => r.GetUserPreferencesAsync(Guid.Parse(userId)), Times.Once);
    }
}
