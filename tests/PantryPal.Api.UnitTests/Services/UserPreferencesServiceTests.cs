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
    public async Task GetUserPreferencesAsync_ExistingUser_ReturnsPreferencesDto()
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

    }

    [Fact]
    public async Task GetUserPreferencesAsync_NonexistentUser_ReturnsNull()
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
    }

    [Fact]
    public async Task GetUserPreferencesAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.GetUserPreferencesAsync(userId));

    }

    [Fact]
    public async Task UpsertPreferencesAsync_ValidData_ReturnsPreferencesDto()
    {
        // Arrange
        var userId = "550e8400-e29b-41d4-a716-446655440000";
        var userGuid = Guid.Parse(userId);
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
            .Setup(r => r.GetUserPreferencesAsync(userGuid))
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

    }

    [Fact]
    public async Task UpsertPreferencesAsync_NullDislikedIngredients_ReturnsPreferencesDto()
    {
        // Arrange
        var userId = "550e8400-e29b-41d4-a716-446655440001";
        var userGuid = Guid.Parse(userId);
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
            .Setup(r => r.GetUserPreferencesAsync(userGuid))
            .ReturnsAsync(upsertedRecord);

        // Act
        var result = await _service.UpsertPreferencesAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Null(result.DislikedIngredients);

    }

    [Fact]
    public async Task UpsertPreferencesAsync_InvalidDietTypeId_ThrowsArgumentException()
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
        Assert.Equal("DietTypeId", exception.ParamName);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_InvalidPreferredCuisineId_ThrowsArgumentException()
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
        Assert.Equal("PreferredCuisineId", exception.ParamName);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    public async Task UpsertPreferencesAsync_InvalidUserId_ThrowsArgumentException(string? invalidUserId)
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(1, 2, "nuts");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpsertPreferencesAsync(dto, invalidUserId));

        Assert.Contains("User ID cannot be null or empty", exception.Message);
        Assert.Equal("userId", exception.ParamName);

    }

    [Fact]
    public async Task UpsertPreferencesAsync_NullDto_ThrowsArgumentNullException()
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
    public async Task UpsertPreferencesAsync_UpsertRepositoryThrowsException_PropagatesException()
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

    }

    [Fact]
    public async Task UpsertPreferencesAsync_GetPreferencesRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var userId = "550e8400-e29b-41d4-a716-446655440002";
        var userGuid = Guid.Parse(userId);
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
            .Setup(r => r.GetUserPreferencesAsync(userGuid))
            .ThrowsAsync(new Exception("Failed to retrieve preferences"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.UpsertPreferencesAsync(dto, userId));

    }

    // ================================
    // Edge Cases and Input Validation Tests
    // ================================

    [Fact]
    public async Task GetUserPreferencesAsync_EmptyGuid_ReturnsNull()
    {
        // Arrange
        var userId = Guid.Empty;

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
    public async Task GetUserPreferencesAsync_NullDietTypes_ReturnsEmptyDietTypeName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypeId = 1,
            PreferredCuisineId = 2,
            DislikedIngredients = "nuts",
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
            DietTypes = null, // Null diet types
            PreferredCuisines = new PreferredCuisinesSelect { Id = 2, Name = "Italian" }
        };

        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockPreferences);

        // Act
        var result = await _service.GetUserPreferencesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.DietTypeName);
        Assert.Equal("Italian", result.PreferredCuisineName);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_NullPreferredCuisines_ReturnsEmptyCuisineName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypeId = 1,
            PreferredCuisineId = 2,
            DislikedIngredients = "nuts",
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegetarian" },
            PreferredCuisines = null // Null preferred cuisines
        };

        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockPreferences);

        // Act
        var result = await _service.GetUserPreferencesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Vegetarian", result.DietTypeName);
        Assert.Equal(string.Empty, result.PreferredCuisineName);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_BothJoinedEntitiesNull_ReturnsEmptyNames()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypeId = 1,
            PreferredCuisineId = 2,
            DislikedIngredients = "nuts",
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
            DietTypes = null,
            PreferredCuisines = null
        };

        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockPreferences);

        // Act
        var result = await _service.GetUserPreferencesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.DietTypeName);
        Assert.Equal(string.Empty, result.PreferredCuisineName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpsertPreferencesAsync_EmptyDislikedIngredients_HandlesCorrectly(string emptyIngredients)
    {
        // Arrange
        var userId = "550e8400-e29b-41d4-a716-446655440003";
        var userGuid = Guid.Parse(userId);
        var dto = new UserPreferencesCreateDto(1, 2, emptyIngredients);

        var upsertedRecord = new UserPreferencesSelect
        {
            UserId = userId,
            DietTypeId = 1,
            PreferredCuisineId = 2,
            DislikedIngredients = emptyIngredients,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
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
            .Setup(r => r.UpsertUserPreferencesAsync(userId, 1, 2, emptyIngredients))
            .ReturnsAsync(upsertedRecord);
        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(userGuid))
            .ReturnsAsync(upsertedRecord);

        // Act
        var result = await _service.UpsertPreferencesAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(emptyIngredients, result.DislikedIngredients);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_VeryLongDislikedIngredients_HandlesCorrectly()
    {
        // Arrange
        var longIngredients = string.Join(", ", Enumerable.Range(1, 100).Select(i => $"ingredient{i}"));
        var userId = "550e8400-e29b-41d4-a716-446655440004";
        var userGuid = Guid.Parse(userId);
        var dto = new UserPreferencesCreateDto(1, 2, longIngredients);

        var upsertedRecord = new UserPreferencesSelect
        {
            UserId = userId,
            DietTypeId = 1,
            PreferredCuisineId = 2,
            DislikedIngredients = longIngredients,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
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
            .Setup(r => r.UpsertUserPreferencesAsync(userId, 1, 2, longIngredients))
            .ReturnsAsync(upsertedRecord);
        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(userGuid))
            .ReturnsAsync(upsertedRecord);

        // Act
        var result = await _service.UpsertPreferencesAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(longIngredients, result.DislikedIngredients);
        Assert.Contains("ingredient1", result.DislikedIngredients);
        Assert.Contains("ingredient100", result.DislikedIngredients);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)-1)]
    [InlineData((short)-32768)] // Min short value
    public async Task UpsertPreferencesAsync_InvalidDietTypeIds_ThrowsArgumentException(short invalidDietTypeId)
    {
        // Arrange
        var userId = "test-user-123";
        var dto = new UserPreferencesCreateDto(invalidDietTypeId, 2, "nuts");

        _mockRepository
            .Setup(r => r.DietTypeExistsAsync(invalidDietTypeId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpsertPreferencesAsync(dto, userId));

        Assert.Contains($"Diet type with ID {invalidDietTypeId} does not exist", exception.Message);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)-1)]
    [InlineData((short)-32768)] // Min short value
    public async Task UpsertPreferencesAsync_InvalidPreferredCuisineIds_ThrowsArgumentException(short invalidCuisineId)
    {
        // Arrange
        var userId = "test-user-123";
        var dto = new UserPreferencesCreateDto(1, invalidCuisineId, "nuts");

        _mockRepository
            .Setup(r => r.DietTypeExistsAsync(1))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.PreferredCuisineExistsAsync(invalidCuisineId))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpsertPreferencesAsync(dto, userId));

        Assert.Contains($"Preferred cuisine with ID {invalidCuisineId} does not exist", exception.Message);
    }

    [Theory]
    [InlineData((short)32767)] // Max short value
    [InlineData((short)1000)]
    public async Task UpsertPreferencesAsync_LargeValidIds_HandlesCorrectly(short largeId)
    {
        // Arrange
        var userId = "550e8400-e29b-41d4-a716-446655440005";
        var userGuid = Guid.Parse(userId);
        var dto = new UserPreferencesCreateDto(largeId, largeId, "nuts");

        var upsertedRecord = new UserPreferencesSelect
        {
            UserId = userId,
            DietTypeId = largeId,
            PreferredCuisineId = largeId,
            DislikedIngredients = "nuts",
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
            DietTypes = new DietTypesSelect { Id = largeId, Name = "Test Diet" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = largeId, Name = "Test Cuisine" }
        };

        _mockRepository
            .Setup(r => r.DietTypeExistsAsync(largeId))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.PreferredCuisineExistsAsync(largeId))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.UpsertUserPreferencesAsync(userId, largeId, largeId, "nuts"))
            .ReturnsAsync(upsertedRecord);
        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(userGuid))
            .ReturnsAsync(upsertedRecord);

        // Act
        var result = await _service.UpsertPreferencesAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(largeId, result.DietTypeId);
        Assert.Equal(largeId, result.PreferredCuisineId);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_SpecialCharactersInDislikedIngredients_HandlesCorrectly()
    {
        // Arrange
        var specialIngredients = "nuts & seeds, dairy (milk, cheese), shellfish, gluten-free items";
        var userId = "550e8400-e29b-41d4-a716-446655440006";
        var userGuid = Guid.Parse(userId);
        var dto = new UserPreferencesCreateDto(1, 2, specialIngredients);

        var upsertedRecord = new UserPreferencesSelect
        {
            UserId = userId,
            DietTypeId = 1,
            PreferredCuisineId = 2,
            DislikedIngredients = specialIngredients,
            CreatedAt = "2024-01-01",
            UpdatedAt = "2024-01-01",
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
            .Setup(r => r.UpsertUserPreferencesAsync(userId, 1, 2, specialIngredients))
            .ReturnsAsync(upsertedRecord);
        _mockRepository
            .Setup(r => r.GetUserPreferencesAsync(userGuid))
            .ReturnsAsync(upsertedRecord);

        // Act
        var result = await _service.UpsertPreferencesAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(specialIngredients, result.DislikedIngredients);
        Assert.Contains("&", result.DislikedIngredients);
        Assert.Contains("(", result.DislikedIngredients);
        Assert.Contains(")", result.DislikedIngredients);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_UpsertReturnsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = "550e8400-e29b-41d4-a716-446655440007";
        var userGuid = Guid.Parse(userId);
        var dto = new UserPreferencesCreateDto(1, 2, "nuts");

        _mockRepository
            .Setup(r => r.DietTypeExistsAsync(1))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.PreferredCuisineExistsAsync(2))
            .ReturnsAsync(true);
        _mockRepository
            .Setup(r => r.UpsertUserPreferencesAsync(userId, 1, 2, "nuts"))
            .ReturnsAsync((UserPreferencesSelect?)null); // Repository returns null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpsertPreferencesAsync(dto, userId));

        Assert.Contains("Failed to retrieve upserted user preferences", exception.Message);
    }

    [Fact]
    public async Task UpsertPreferencesAsync_GetPreferencesReturnsNullAfterUpsert_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = "550e8400-e29b-41d4-a716-446655440008";
        var userGuid = Guid.Parse(userId);
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
            .Setup(r => r.GetUserPreferencesAsync(userGuid))
            .ReturnsAsync((UserPreferencesSelect?)null); // Get returns null

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpsertPreferencesAsync(dto, userId));

        Assert.Contains("Failed to retrieve upserted user preferences", exception.Message);
    }
}
