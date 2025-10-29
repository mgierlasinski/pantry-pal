using Microsoft.Extensions.Logging;
using Moq;
using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;

namespace PantryPal.Api.UnitTests.Services;

/// <summary>
/// Unit tests for RecipeService.AcceptGeneratedRecipeAsync method
/// </summary>
public class RecipeServiceTests
{
    private readonly Mock<IRecipeRepository> _mockRecipeRepository;
    private readonly Mock<IPantryRepository> _mockPantryRepository;
    private readonly Mock<IUserPreferencesRepository> _mockUserPreferencesRepository;
    private readonly Mock<IRecipesGenerationsRepository> _mockRecipesGenerationsRepository;
    private readonly Mock<IAIRecipeGeneratorService> _mockAIService;
    private readonly Mock<ILogger<RecipeService>> _mockLogger;
    private readonly RecipeService _service;

    public RecipeServiceTests()
    {
        _mockRecipeRepository = new Mock<IRecipeRepository>();
        _mockPantryRepository = new Mock<IPantryRepository>();
        _mockUserPreferencesRepository = new Mock<IUserPreferencesRepository>();
        _mockRecipesGenerationsRepository = new Mock<IRecipesGenerationsRepository>();
        _mockAIService = new Mock<IAIRecipeGeneratorService>();
        _mockLogger = new Mock<ILogger<RecipeService>>();

        _service = new RecipeService(
            _mockRecipeRepository.Object,
            _mockPantryRepository.Object,
            _mockUserPreferencesRepository.Object,
            _mockRecipesGenerationsRepository.Object,
            _mockAIService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_WithValidGeneration_ReturnsSuccessResponse()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipeId = Guid.NewGuid().ToString();
        var createdAt = "2024-10-29T12:00:00Z";

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "# Test Recipe\n\nIngredients:\n- Test ingredient\n\nInstructions:\n1. Test step",
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = null
        };

        var mockCreatedRecipe = new RecipesSelect
        {
            Id = recipeId,
            UserId = userId.ToString(),
            RecipeText = mockGeneration.GeneratedRecipeText,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _mockRecipeRepository
            .Setup(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()))
            .ReturnsAsync(mockCreatedRecipe);

        _mockRecipesGenerationsRepository
            .Setup(r => r.MarkAsAcceptedAsync(generationId, Guid.Parse(recipeId)))
            .ReturnsAsync(mockGeneration);

        // Act
        var result = await _service.AcceptGeneratedRecipeAsync(generationId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recipeId, result.RecipeId);
        Assert.Equal(createdAt, result.SavedAt);

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _mockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.Is<RecipesInsert>(
            insert => insert.UserId == userId.ToString() && 
                     insert.RecipeText == mockGeneration.GeneratedRecipeText
        )), Times.Once);
        _mockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(generationId, Guid.Parse(recipeId)), Times.Once);
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_WithNonExistentGeneration_ThrowsArgumentException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync((RecipesGenerationsSelect?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AcceptGeneratedRecipeAsync(generationId, userId));

        Assert.Equal("Generation not found", exception.Message);

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _mockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Never);
        _mockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_WithAlreadyAcceptedGeneration_ThrowsInvalidOperationException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingRecipeId = Guid.NewGuid().ToString();

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "# Test Recipe\n\nIngredients:\n- Test ingredient",
            GeneratedRecipeId = existingRecipeId, // Already has a recipe ID
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = null
        };

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AcceptGeneratedRecipeAsync(generationId, userId));

        Assert.Equal("Already accepted", exception.Message);

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _mockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Never);
        _mockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_WithNullRecipeText_ThrowsInvalidOperationException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = null, // No recipe text
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = "AI_SERVICE_ERROR",
            ErrorMessage = "Failed to generate",
            RejectReasonId = null
        };

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AcceptGeneratedRecipeAsync(generationId, userId));

        Assert.Equal("No recipe text available", exception.Message);

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _mockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Never);
        _mockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_WithEmptyRecipeText_ThrowsInvalidOperationException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "   ", // Whitespace only
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = null
        };

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AcceptGeneratedRecipeAsync(generationId, userId));

        Assert.Equal("No recipe text available", exception.Message);

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _mockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Never);
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_WhenRecipeCreationFails_PropagatesException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "# Test Recipe",
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = null
        };

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _mockRecipeRepository
            .Setup(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.AcceptGeneratedRecipeAsync(generationId, userId));

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _mockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Once);
        _mockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_WhenMarkAsAcceptedFails_PropagatesException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipeId = Guid.NewGuid().ToString();

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "# Test Recipe",
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = null
        };

        var mockCreatedRecipe = new RecipesSelect
        {
            Id = recipeId,
            UserId = userId.ToString(),
            RecipeText = mockGeneration.GeneratedRecipeText,
            CreatedAt = "2024-10-29T12:00:00Z",
            UpdatedAt = "2024-10-29T12:00:00Z"
        };

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _mockRecipeRepository
            .Setup(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()))
            .ReturnsAsync(mockCreatedRecipe);

        _mockRecipesGenerationsRepository
            .Setup(r => r.MarkAsAcceptedAsync(generationId, Guid.Parse(recipeId)))
            .ThrowsAsync(new Exception("Update failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _service.AcceptGeneratedRecipeAsync(generationId, userId));

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _mockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Once);
        _mockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(generationId, Guid.Parse(recipeId)), Times.Once);
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_CreatesRecipeWithCorrectData()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipeText = "# Delicious Recipe\n\nIngredients:\n- Tomato\n- Onion\n\nInstructions:\n1. Chop vegetables\n2. Cook";

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1500,
            GeneratedRecipeText = recipeText,
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = null
        };

        var mockCreatedRecipe = new RecipesSelect
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId.ToString(),
            RecipeText = recipeText,
            CreatedAt = "2024-10-29T12:00:00Z",
            UpdatedAt = "2024-10-29T12:00:00Z"
        };

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _mockRecipeRepository
            .Setup(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()))
            .ReturnsAsync(mockCreatedRecipe);

        _mockRecipesGenerationsRepository
            .Setup(r => r.MarkAsAcceptedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(mockGeneration);

        // Act
        var result = await _service.AcceptGeneratedRecipeAsync(generationId, userId);

        // Assert
        _mockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.Is<RecipesInsert>(
            insert => 
                insert.UserId == userId.ToString() && 
                insert.RecipeText == recipeText
        )), Times.Once);

        Assert.Equal(mockCreatedRecipe.Id, result.RecipeId);
        Assert.Equal(mockCreatedRecipe.CreatedAt, result.SavedAt);
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_MarksGenerationAsAcceptedWithCorrectRecipeId()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recipeId = Guid.NewGuid();

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "# Recipe",
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = null
        };

        var mockCreatedRecipe = new RecipesSelect
        {
            Id = recipeId.ToString(),
            UserId = userId.ToString(),
            RecipeText = mockGeneration.GeneratedRecipeText,
            CreatedAt = "2024-10-29T12:00:00Z",
            UpdatedAt = "2024-10-29T12:00:00Z"
        };

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _mockRecipeRepository
            .Setup(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()))
            .ReturnsAsync(mockCreatedRecipe);

        _mockRecipesGenerationsRepository
            .Setup(r => r.MarkAsAcceptedAsync(generationId, recipeId))
            .ReturnsAsync(mockGeneration);

        // Act
        await _service.AcceptGeneratedRecipeAsync(generationId, userId);

        // Assert
        _mockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(generationId, recipeId), Times.Once);
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_WithDifferentUsers_IsolatesData()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        // Generation belongs to user1
        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = user1.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "# Recipe",
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = null
        };

        // Setup returns generation for user1
        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, user1))
            .ReturnsAsync(mockGeneration);

        // Setup returns null for user2 (not their generation)
        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, user2))
            .ReturnsAsync((RecipesGenerationsSelect?)null);

        // Act & Assert - user2 cannot access user1's generation
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AcceptGeneratedRecipeAsync(generationId, user2));

        Assert.Equal("Generation not found", exception.Message);

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, user2), Times.Once);
        _mockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Never);
    }

    [Fact]
    public async Task RejectGeneratedRecipeAsync_WithValidGeneration_ReturnsSuccessfully()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var rejectReasonId = (short)1;

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "# Test Recipe\n\nIngredients:\n- Test ingredient",
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = null // Not rejected yet
        };

        var updatedGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "# Test Recipe\n\nIngredients:\n- Test ingredient",
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = rejectReasonId // Now rejected
        };

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _mockRecipesGenerationsRepository
            .Setup(r => r.UpdateRejectReasonAsync(generationId, rejectReasonId))
            .ReturnsAsync(updatedGeneration);

        // Act
        await _service.RejectGeneratedRecipeAsync(generationId, rejectReasonId, userId);

        // Assert
        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _mockRecipesGenerationsRepository.Verify(r => r.UpdateRejectReasonAsync(generationId, rejectReasonId), Times.Once);
    }

    [Fact]
    public async Task RejectGeneratedRecipeAsync_WithGenerationNotFound_ThrowsArgumentException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var rejectReasonId = (short)1;

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync((RecipesGenerationsSelect?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RejectGeneratedRecipeAsync(generationId, rejectReasonId, userId));

        Assert.Equal("Generation not found", exception.Message);

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _mockRecipesGenerationsRepository.Verify(r => r.UpdateRejectReasonAsync(It.IsAny<Guid>(), It.IsAny<short>()), Times.Never);
    }

    [Fact]
    public async Task RejectGeneratedRecipeAsync_WithAlreadyRejectedGeneration_ThrowsInvalidOperationException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var rejectReasonId = (short)1;
        var existingRejectReasonId = (short)2;

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = userId.ToString(),
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "# Test Recipe\n\nIngredients:\n- Test ingredient",
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = existingRejectReasonId // Already rejected
        };

        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RejectGeneratedRecipeAsync(generationId, rejectReasonId, userId));

        Assert.Equal("Already rejected", exception.Message);

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _mockRecipesGenerationsRepository.Verify(r => r.UpdateRejectReasonAsync(It.IsAny<Guid>(), It.IsAny<short>()), Times.Never);
    }

    [Fact]
    public async Task RejectGeneratedRecipeAsync_WithOtherUsersGeneration_ThrowsArgumentException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var user1 = Guid.NewGuid(); // Owner of the generation
        var user2 = Guid.NewGuid(); // Trying to reject user1's generation
        var rejectReasonId = (short)1;

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId.ToString(),
            UserId = user1.ToString(), // Belongs to user1
            Model = "mock-gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = "# Test Recipe\n\nIngredients:\n- Test ingredient",
            GeneratedRecipeId = null,
            CreatedAt = "2024-10-29T11:00:00Z",
            ErrorCode = null,
            ErrorMessage = null,
            RejectReasonId = null
        };

        // Setup returns generation for user1
        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, user1))
            .ReturnsAsync(mockGeneration);

        // Setup returns null for user2 (not their generation)
        _mockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, user2))
            .ReturnsAsync((RecipesGenerationsSelect?)null);

        // Act & Assert - user2 cannot access user1's generation
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RejectGeneratedRecipeAsync(generationId, rejectReasonId, user2));

        Assert.Equal("Generation not found", exception.Message);

        _mockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, user2), Times.Once);
        _mockRecipesGenerationsRepository.Verify(r => r.UpdateRejectReasonAsync(It.IsAny<Guid>(), It.IsAny<short>()), Times.Never);
    }
}

