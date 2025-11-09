using Moq;
using PantryPal.Api.Db;
using PantryPal.Api.Services.OpenRouter;

namespace PantryPal.Api.UnitTests.Services;

/// <summary>
/// Unit tests for RecipeService
/// </summary>
public class RecipeServiceTests
{
    private readonly RecipeServiceTestFixture _fixture;

    public RecipeServiceTests()
    {
        _fixture = new RecipeServiceTestFixture();
    }

    // ================================
    // GetRecipesAsync Tests
    // ================================

    [Fact]
    public async Task GetRecipesAsync_ValidParameters_ReturnsPaginatedResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;
        var totalItems = 25;

        var mockRecipes = new List<RecipesSelect>
        {
            new()
            {
                Id = "recipe-1",
                UserId = userId.ToString(),
                RecipeText = "# Test Recipe 1\n\nIngredients: Test",
                CreatedAt = "2024-01-01T10:00:00Z",
                UpdatedAt = "2024-01-01T10:00:00Z"
            },
            new()
            {
                Id = "recipe-2",
                UserId = userId.ToString(),
                RecipeText = "# Test Recipe 2\n\nIngredients: Test",
                CreatedAt = "2024-01-02T10:00:00Z",
                UpdatedAt = "2024-01-02T10:00:00Z"
            }
        };

        _fixture.MockRecipeRepository
            .Setup(r => r.GetRecipesAsync(userId, page, pageSize))
            .ReturnsAsync((mockRecipes, totalItems));

        // Act
        var result = await _fixture.Service.GetRecipesAsync(userId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(page, result.Page);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(totalItems, result.Total);

        var firstItem = result.Items.First();
        Assert.Equal("recipe-1", firstItem.Id);
        Assert.Equal("# Test Recipe 1\n\nIngredients: Test", firstItem.RecipeText);
        Assert.Equal("2024-01-01T10:00:00Z", firstItem.CreatedAt);
        Assert.Equal("2024-01-01T10:00:00Z", firstItem.UpdatedAt);

        _fixture.MockRecipeRepository.Verify(r => r.GetRecipesAsync(userId, page, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetRecipesAsync_EmptyResult_ReturnsEmptyResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;

        var mockRecipes = new List<RecipesSelect>();
        var totalItems = 0;

        _fixture.MockRecipeRepository
            .Setup(r => r.GetRecipesAsync(userId, page, pageSize))
            .ReturnsAsync((mockRecipes, totalItems));

        // Act
        var result = await _fixture.Service.GetRecipesAsync(userId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(page, result.Page);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(0, result.Total);

        _fixture.MockRecipeRepository.Verify(r => r.GetRecipesAsync(userId, page, pageSize), Times.Once);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(3, 20)]
    public async Task GetRecipesAsync_PaginationParameters_PassedCorrectly(int page, int pageSize)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockRecipes = new List<RecipesSelect>();
        var totalItems = 0;

        _fixture.MockRecipeRepository
            .Setup(r => r.GetRecipesAsync(userId, page, pageSize))
            .ReturnsAsync((mockRecipes, totalItems));

        // Act
        var result = await _fixture.Service.GetRecipesAsync(userId, page, pageSize);

        // Assert
        Assert.Equal(page, result.Page);
        Assert.Equal(pageSize, result.PageSize);

        _fixture.MockRecipeRepository.Verify(r => r.GetRecipesAsync(userId, page, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetRecipesAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;

        _fixture.MockRecipeRepository
            .Setup(r => r.GetRecipesAsync(userId, page, pageSize))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _fixture.Service.GetRecipesAsync(userId, page, pageSize));

        _fixture.MockRecipeRepository.Verify(r => r.GetRecipesAsync(userId, page, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetRecipesAsync_MapsAllRecipeFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 1;

        var mockRecipes = new List<RecipesSelect>
        {
            new()
            {
                Id = "test-recipe-id-123",
                UserId = userId.ToString(),
                RecipeText = "# Detailed Recipe\n\nIngredients:\n- Tomato\n- Onion\n\nInstructions:\n1. Chop\n2. Cook",
                CreatedAt = "2024-10-29T15:30:45Z",
                UpdatedAt = "2024-10-30T09:15:22Z"
            }
        };

        _fixture.MockRecipeRepository
            .Setup(r => r.GetRecipesAsync(userId, page, pageSize))
            .ReturnsAsync((mockRecipes, 1));

        // Act
        var result = await _fixture.Service.GetRecipesAsync(userId, page, pageSize);

        // Assert
        var recipe = result.Items.First();
        Assert.Equal("test-recipe-id-123", recipe.Id);
        Assert.Equal("# Detailed Recipe\n\nIngredients:\n- Tomato\n- Onion\n\nInstructions:\n1. Chop\n2. Cook", recipe.RecipeText);
        Assert.Equal("2024-10-29T15:30:45Z", recipe.CreatedAt);
        Assert.Equal("2024-10-30T09:15:22Z", recipe.UpdatedAt);
    }

    // ================================
    // GenerateRecipeAsync Tests
    // ================================

    [Fact]
    public async Task GenerateRecipeAsync_ValidData_ReturnsGeneratedRecipe()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid().ToString();
        var expectedRecipeText = "# Generated Recipe\n\nIngredients:\n- Tomato\n- Pasta\n\nInstructions:\n1. Cook pasta\n2. Add tomatoes";

        var mockUserPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegetarian" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = 1, Name = "Italian" },
            DislikedIngredients = "mushrooms, olives"
        };

        var mockPantryItems = new List<PantryItemsSelect>
        {
            new() { Id = "1", Name = "Tomato", UserId = userId.ToString() },
            new() { Id = "2", Name = "Pasta", UserId = userId.ToString() },
            new() { Id = "3", Name = "Cheese", UserId = userId.ToString() }
        };

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId,
            UserId = userId.ToString(),
            Model = "gpt-4",
            DurationMs = 1000,
            GeneratedRecipeText = expectedRecipeText,
            CreatedAt = "2024-10-29T10:00:00Z"
        };

        _fixture.MockOptions.Setup(o => o.Value).Returns(new OpenRouterOptions { ApiKey = "test-key", BaseUrl = "https://test.com", SiteName = "Test", Model = "gpt-4" });
        _fixture.MockUserPreferencesRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockUserPreferences);
        _fixture.MockPantryRepository
            .Setup(r => r.GetAllPantryItemsAsync(userId))
            .ReturnsAsync(mockPantryItems);
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()))
            .ReturnsAsync(mockGeneration);
        _fixture.MockAIService
            .Setup(s => s.GenerateAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedRecipeText);
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.UpdateGenerationAsync(It.IsAny<RecipesGenerationsUpdate>()))
            .ReturnsAsync(mockGeneration);

        // Act
        var result = await _fixture.Service.GenerateRecipeAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(generationId, result.GenerationId);
        Assert.Equal(expectedRecipeText, result.RecipeText);

        _fixture.MockUserPreferencesRepository.Verify(r => r.GetUserPreferencesAsync(userId), Times.Once);
        _fixture.MockPantryRepository.Verify(r => r.GetAllPantryItemsAsync(userId), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()), Times.Once);
        _fixture.MockAIService.Verify(s => s.GenerateAsync(It.IsAny<string>()), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.UpdateGenerationAsync(It.IsAny<RecipesGenerationsUpdate>()), Times.Once);
    }

    [Fact]
    public async Task GenerateRecipeAsync_MissingUserPreferences_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _fixture.MockUserPreferencesRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync((UserPreferencesSelect?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _fixture.Service.GenerateRecipeAsync(userId));

        Assert.Equal("User preferences not set.", exception.Message);

        _fixture.MockUserPreferencesRepository.Verify(r => r.GetUserPreferencesAsync(userId), Times.Once);
        _fixture.MockPantryRepository.Verify(r => r.GetAllPantryItemsAsync(It.IsAny<Guid>()), Times.Never);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()), Times.Never);
        _fixture.MockAIService.Verify(s => s.GenerateAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GenerateRecipeAsync_EmptyPantry_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var mockUserPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegetarian" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = 1, Name = "Italian" },
            DislikedIngredients = null
        };

        var mockPantryItems = new List<PantryItemsSelect>(); // Empty pantry

        _fixture.MockUserPreferencesRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockUserPreferences);
        _fixture.MockPantryRepository
            .Setup(r => r.GetAllPantryItemsAsync(userId))
            .ReturnsAsync(mockPantryItems);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _fixture.Service.GenerateRecipeAsync(userId));

        Assert.Equal("Pantry is empty.", exception.Message);

        _fixture.MockUserPreferencesRepository.Verify(r => r.GetUserPreferencesAsync(userId), Times.Once);
        _fixture.MockPantryRepository.Verify(r => r.GetAllPantryItemsAsync(userId), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()), Times.Never);
        _fixture.MockAIService.Verify(s => s.GenerateAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GenerateRecipeAsync_AIServiceFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid().ToString();

        var mockUserPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegetarian" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = 1, Name = "Italian" },
            DislikedIngredients = "mushrooms"
        };

        var mockPantryItems = new List<PantryItemsSelect>
        {
            new() { Id = "1", Name = "Tomato", UserId = userId.ToString() },
            new() { Id = "2", Name = "Pasta", UserId = userId.ToString() }
        };

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId,
            UserId = userId.ToString(),
            Model = "gpt-4",
            DurationMs = 0,
            CreatedAt = "2024-10-29T10:00:00Z"
        };

        _fixture.MockOptions.Setup(o => o.Value).Returns(new OpenRouterOptions { ApiKey = "test-key", BaseUrl = "https://test.com", SiteName = "Test", Model = "gpt-4" });
        _fixture.MockUserPreferencesRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockUserPreferences);
        _fixture.MockPantryRepository
            .Setup(r => r.GetAllPantryItemsAsync(userId))
            .ReturnsAsync(mockPantryItems);
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()))
            .ReturnsAsync(mockGeneration);
        _fixture.MockAIService
            .Setup(s => s.GenerateAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("AI service timeout"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _fixture.Service.GenerateRecipeAsync(userId));

        Assert.Contains("Failed to generate recipe", exception.Message);

        _fixture.MockUserPreferencesRepository.Verify(r => r.GetUserPreferencesAsync(userId), Times.Once);
        _fixture.MockPantryRepository.Verify(r => r.GetAllPantryItemsAsync(userId), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()), Times.Once);
        _fixture.MockAIService.Verify(s => s.GenerateAsync(It.IsAny<string>()), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.UpdateGenerationAsync(It.Is<RecipesGenerationsUpdate>(
            u => u.Id == generationId && u.ErrorCode == "AI_SERVICE_ERROR")), Times.Once);
    }

    [Fact]
    public async Task GenerateRecipeAsync_CreationRecordFailure_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var mockUserPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegetarian" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = 1, Name = "Italian" },
            DislikedIngredients = null
        };

        var mockPantryItems = new List<PantryItemsSelect>
        {
            new() { Id = "1", Name = "Tomato", UserId = userId.ToString() }
        };

        _fixture.MockUserPreferencesRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockUserPreferences);
        _fixture.MockPantryRepository
            .Setup(r => r.GetAllPantryItemsAsync(userId))
            .ReturnsAsync(mockPantryItems);
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _fixture.Service.GenerateRecipeAsync(userId));

        _fixture.MockUserPreferencesRepository.Verify(r => r.GetUserPreferencesAsync(userId), Times.Once);
        _fixture.MockPantryRepository.Verify(r => r.GetAllPantryItemsAsync(userId), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()), Times.Once);
        _fixture.MockAIService.Verify(s => s.GenerateAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GenerateRecipeAsync_UpdateRecordFailure_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid().ToString();
        var expectedRecipeText = "# Generated Recipe\n\nIngredients: Tomato";

        var mockUserPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegetarian" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = 1, Name = "Italian" },
            DislikedIngredients = null
        };

        var mockPantryItems = new List<PantryItemsSelect>
        {
            new() { Id = "1", Name = "Tomato", UserId = userId.ToString() }
        };

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId,
            UserId = userId.ToString(),
            Model = "gpt-4",
            DurationMs = 0,
            CreatedAt = "2024-10-29T10:00:00Z"
        };

        _fixture.MockOptions.Setup(o => o.Value).Returns(new OpenRouterOptions { ApiKey = "test-key", BaseUrl = "https://test.com", SiteName = "Test", Model = "gpt-4" });
        _fixture.MockUserPreferencesRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockUserPreferences);
        _fixture.MockPantryRepository
            .Setup(r => r.GetAllPantryItemsAsync(userId))
            .ReturnsAsync(mockPantryItems);
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()))
            .ReturnsAsync(mockGeneration);
        _fixture.MockAIService
            .Setup(s => s.GenerateAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedRecipeText);
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.UpdateGenerationAsync(It.IsAny<RecipesGenerationsUpdate>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _fixture.Service.GenerateRecipeAsync(userId));

        _fixture.MockUserPreferencesRepository.Verify(r => r.GetUserPreferencesAsync(userId), Times.Once);
        _fixture.MockPantryRepository.Verify(r => r.GetAllPantryItemsAsync(userId), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()), Times.Once);
        _fixture.MockAIService.Verify(s => s.GenerateAsync(It.IsAny<string>()), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.UpdateGenerationAsync(It.IsAny<RecipesGenerationsUpdate>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GenerateRecipeAsync_BuildsCorrectPrompt_IncludesAllIngredientsAndPreferences()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid().ToString();
        var expectedRecipeText = "# Recipe";

        var mockUserPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegan" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = 2, Name = "Mexican" },
            DislikedIngredients = "onions, garlic"
        };

        var mockPantryItems = new List<PantryItemsSelect>
        {
            new() { Id = "1", Name = "Black Beans", UserId = userId.ToString() },
            new() { Id = "2", Name = "Corn", UserId = userId.ToString() },
            new() { Id = "3", Name = "Tomatoes", UserId = userId.ToString() }
        };

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId,
            UserId = userId.ToString(),
            Model = "gpt-4",
            DurationMs = 0,
            CreatedAt = "2024-10-29T10:00:00Z"
        };

        string? capturedPrompt = null;
        _fixture.MockOptions.Setup(o => o.Value).Returns(new OpenRouterOptions { ApiKey = "test-key", BaseUrl = "https://test.com", SiteName = "Test", Model = "gpt-4" });
        _fixture.MockUserPreferencesRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockUserPreferences);
        _fixture.MockPantryRepository
            .Setup(r => r.GetAllPantryItemsAsync(userId))
            .ReturnsAsync(mockPantryItems);
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()))
            .ReturnsAsync(mockGeneration);
        _fixture.MockAIService
            .Setup(s => s.GenerateAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedRecipeText)
            .Callback<string>(prompt => capturedPrompt = prompt);
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.UpdateGenerationAsync(It.IsAny<RecipesGenerationsUpdate>()))
            .ReturnsAsync(mockGeneration);

        // Act
        await _fixture.Service.GenerateRecipeAsync(userId);

        // Assert
        Assert.NotNull(capturedPrompt);
        Assert.Contains("Black Beans, Corn, Tomatoes", capturedPrompt);
        Assert.Contains("Diet Type: Vegan", capturedPrompt);
        Assert.Contains("Preferred Cuisine: Mexican", capturedPrompt);
        Assert.Contains("Disliked Ingredients: onions, garlic", capturedPrompt);
        Assert.Contains("Please create a detailed recipe in markdown format", capturedPrompt);
    }

    [Fact]
    public async Task GenerateRecipeAsync_NoDislikedIngredients_UsesNoneInPrompt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var generationId = Guid.NewGuid().ToString();
        var expectedRecipeText = "# Recipe";

        var mockUserPreferences = new UserPreferencesSelect
        {
            UserId = userId.ToString(),
            DietTypes = new DietTypesSelect { Id = 1, Name = "Vegetarian" },
            PreferredCuisines = new PreferredCuisinesSelect { Id = 1, Name = "Italian" },
            DislikedIngredients = null // No disliked ingredients
        };

        var mockPantryItems = new List<PantryItemsSelect>
        {
            new() { Id = "1", Name = "Pasta", UserId = userId.ToString() }
        };

        var mockGeneration = new RecipesGenerationsSelect
        {
            Id = generationId,
            UserId = userId.ToString(),
            Model = "gpt-4",
            DurationMs = 0,
            CreatedAt = "2024-10-29T10:00:00Z"
        };

        string? capturedPrompt = null;
        _fixture.MockOptions.Setup(o => o.Value).Returns(new OpenRouterOptions { ApiKey = "test-key", BaseUrl = "https://test.com", SiteName = "Test", Model = "gpt-4" });
        _fixture.MockUserPreferencesRepository
            .Setup(r => r.GetUserPreferencesAsync(userId))
            .ReturnsAsync(mockUserPreferences);
        _fixture.MockPantryRepository
            .Setup(r => r.GetAllPantryItemsAsync(userId))
            .ReturnsAsync(mockPantryItems);
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.CreateGenerationAsync(It.IsAny<RecipesGenerationsInsert>()))
            .ReturnsAsync(mockGeneration);
        _fixture.MockAIService
            .Setup(s => s.GenerateAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedRecipeText)
            .Callback<string>(prompt => capturedPrompt = prompt);
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.UpdateGenerationAsync(It.IsAny<RecipesGenerationsUpdate>()))
            .ReturnsAsync(mockGeneration);

        // Act
        await _fixture.Service.GenerateRecipeAsync(userId);

        // Assert
        Assert.NotNull(capturedPrompt);
        Assert.Contains("Disliked Ingredients: none", capturedPrompt);
    }

    // ================================
    // DeleteRecipeAsync Tests
    // ================================

    [Fact]
    public async Task DeleteRecipeAsync_ValidRecipeAndOwner_DeletesSuccessfully()
    {
        // Arrange
        var recipeId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var mockRecipe = new RecipesSelect
        {
            Id = recipeId,
            UserId = userId.ToString(),
            RecipeText = "# Test Recipe",
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-01T00:00:00Z"
        };

        _fixture.MockRecipeRepository
            .Setup(r => r.GetByIdAsync(recipeId))
            .ReturnsAsync(mockRecipe);
        _fixture.MockRecipeRepository
            .Setup(r => r.DeleteAsync(recipeId))
            .Returns(Task.CompletedTask);

        // Act
        await _fixture.Service.DeleteRecipeAsync(recipeId, userId);

        // Assert
        _fixture.MockRecipeRepository.Verify(r => r.GetByIdAsync(recipeId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.DeleteAsync(recipeId), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_InvalidRecipeIdFormat_ThrowsArgumentException()
    {
        // Arrange
        var invalidRecipeId = "not-a-guid";
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _fixture.Service.DeleteRecipeAsync(invalidRecipeId, userId));

        Assert.Equal("Invalid recipe ID format. (Parameter 'recipeId')", exception.Message);
        Assert.Equal("recipeId", exception.ParamName);

        _fixture.MockRecipeRepository.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
        _fixture.MockRecipeRepository.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRecipeAsync_RecipeNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var recipeId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        _fixture.MockRecipeRepository
            .Setup(r => r.GetByIdAsync(recipeId))
            .ReturnsAsync((RecipesSelect?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _fixture.Service.DeleteRecipeAsync(recipeId, userId));

        Assert.Equal("Recipe not found.", exception.Message);

        _fixture.MockRecipeRepository.Verify(r => r.GetByIdAsync(recipeId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRecipeAsync_UserNotOwner_ThrowsKeyNotFoundException()
    {
        // Arrange
        var recipeId = Guid.NewGuid().ToString();
        var requestingUserId = Guid.NewGuid();
        var actualOwnerId = Guid.NewGuid();

        var mockRecipe = new RecipesSelect
        {
            Id = recipeId,
            UserId = actualOwnerId.ToString(), // Different owner
            RecipeText = "# Test Recipe",
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-01T00:00:00Z"
        };

        _fixture.MockRecipeRepository
            .Setup(r => r.GetByIdAsync(recipeId))
            .ReturnsAsync(mockRecipe);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _fixture.Service.DeleteRecipeAsync(recipeId, requestingUserId));

        Assert.Equal("Recipe not found.", exception.Message); // Masked as "not found" for security

        _fixture.MockRecipeRepository.Verify(r => r.GetByIdAsync(recipeId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRecipeAsync_RepositoryDeleteThrowsException_PropagatesException()
    {
        // Arrange
        var recipeId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var mockRecipe = new RecipesSelect
        {
            Id = recipeId,
            UserId = userId.ToString(),
            RecipeText = "# Test Recipe",
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-01T00:00:00Z"
        };

        _fixture.MockRecipeRepository
            .Setup(r => r.GetByIdAsync(recipeId))
            .ReturnsAsync(mockRecipe);
        _fixture.MockRecipeRepository
            .Setup(r => r.DeleteAsync(recipeId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _fixture.Service.DeleteRecipeAsync(recipeId, userId));

        _fixture.MockRecipeRepository.Verify(r => r.GetByIdAsync(recipeId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.DeleteAsync(recipeId), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_RepositoryGetByIdThrowsException_PropagatesException()
    {
        // Arrange
        var recipeId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        _fixture.MockRecipeRepository
            .Setup(r => r.GetByIdAsync(recipeId))
            .ThrowsAsync(new Exception("Database query failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _fixture.Service.DeleteRecipeAsync(recipeId, userId));

        _fixture.MockRecipeRepository.Verify(r => r.GetByIdAsync(recipeId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // Valid GUID format but zero
    [InlineData("12345678-1234-1234-1234-123456789012")] // Valid GUID format
    [InlineData("a1b2c3d4-e5f6-1234-abcd-1234567890ab")] // Valid GUID format with letters
    public async Task DeleteRecipeAsync_ValidGuidFormats_Accepted(string recipeId)
    {
        // Arrange
        var userId = Guid.NewGuid();

        var mockRecipe = new RecipesSelect
        {
            Id = recipeId,
            UserId = userId.ToString(),
            RecipeText = "# Test Recipe",
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-01T00:00:00Z"
        };

        _fixture.MockRecipeRepository
            .Setup(r => r.GetByIdAsync(recipeId))
            .ReturnsAsync(mockRecipe);
        _fixture.MockRecipeRepository
            .Setup(r => r.DeleteAsync(recipeId))
            .Returns(Task.CompletedTask);

        // Act
        await _fixture.Service.DeleteRecipeAsync(recipeId, userId);

        // Assert
        _fixture.MockRecipeRepository.Verify(r => r.GetByIdAsync(recipeId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.DeleteAsync(recipeId), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-guid")]
    [InlineData("123-456-789")]
    [InlineData("12345678-1234-1234-1234-1234567890123")] // Too long
    public async Task DeleteRecipeAsync_InvalidGuidFormats_ThrowsArgumentException(string invalidRecipeId)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _fixture.Service.DeleteRecipeAsync(invalidRecipeId, userId));

        Assert.Equal("Invalid recipe ID format. (Parameter 'recipeId')", exception.Message);
        Assert.Equal("recipeId", exception.ParamName);

        _fixture.MockRecipeRepository.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
        _fixture.MockRecipeRepository.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRecipeAsync_OwnershipCheckIsCaseSensitive_ThrowsKeyNotFoundException()
    {
        // Arrange
        var recipeId = Guid.NewGuid().ToString();
        var requestingUserId = Guid.NewGuid();
        var actualOwnerId = Guid.NewGuid();

        var mockRecipe = new RecipesSelect
        {
            Id = recipeId,
            UserId = actualOwnerId.ToString().ToUpper(), // Upper case
            RecipeText = "# Test Recipe",
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-01T00:00:00Z"
        };

        _fixture.MockRecipeRepository
            .Setup(r => r.GetByIdAsync(recipeId))
            .ReturnsAsync(mockRecipe);

        // Act & Assert - User ID comparison should be exact
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _fixture.Service.DeleteRecipeAsync(recipeId, requestingUserId));

        Assert.Equal("Recipe not found.", exception.Message);

        _fixture.MockRecipeRepository.Verify(r => r.GetByIdAsync(recipeId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
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

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _fixture.MockRecipeRepository
            .Setup(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()))
            .ReturnsAsync(mockCreatedRecipe);

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.MarkAsAcceptedAsync(generationId, Guid.Parse(recipeId)))
            .ReturnsAsync(mockGeneration);

        // Act
        var result = await _fixture.Service.AcceptGeneratedRecipeAsync(generationId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recipeId, result.RecipeId);
        Assert.Equal(createdAt, result.SavedAt);

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.Is<RecipesInsert>(
            insert => insert.UserId == userId.ToString() && 
                     insert.RecipeText == mockGeneration.GeneratedRecipeText
        )), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(generationId, Guid.Parse(recipeId)), Times.Once);
    }

    [Fact]
    public async Task AcceptGeneratedRecipeAsync_WithNonExistentGeneration_ThrowsArgumentException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync((RecipesGenerationsSelect?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _fixture.Service.AcceptGeneratedRecipeAsync(generationId, userId));

        Assert.Equal("Generation not found", exception.Message);

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Never);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
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

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _fixture.Service.AcceptGeneratedRecipeAsync(generationId, userId));

        Assert.Equal("Already accepted", exception.Message);

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Never);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
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

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _fixture.Service.AcceptGeneratedRecipeAsync(generationId, userId));

        Assert.Equal("No recipe text available", exception.Message);

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Never);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
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

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _fixture.Service.AcceptGeneratedRecipeAsync(generationId, userId));

        Assert.Equal("No recipe text available", exception.Message);

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Never);
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

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _fixture.MockRecipeRepository
            .Setup(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _fixture.Service.AcceptGeneratedRecipeAsync(generationId, userId));

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
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

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _fixture.MockRecipeRepository
            .Setup(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()))
            .ReturnsAsync(mockCreatedRecipe);

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.MarkAsAcceptedAsync(generationId, Guid.Parse(recipeId)))
            .ThrowsAsync(new Exception("Update failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _fixture.Service.AcceptGeneratedRecipeAsync(generationId, userId));

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(generationId, Guid.Parse(recipeId)), Times.Once);
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

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _fixture.MockRecipeRepository
            .Setup(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()))
            .ReturnsAsync(mockCreatedRecipe);

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.MarkAsAcceptedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(mockGeneration);

        // Act
        var result = await _fixture.Service.AcceptGeneratedRecipeAsync(generationId, userId);

        // Assert
        _fixture.MockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.Is<RecipesInsert>(
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

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _fixture.MockRecipeRepository
            .Setup(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()))
            .ReturnsAsync(mockCreatedRecipe);

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.MarkAsAcceptedAsync(generationId, recipeId))
            .ReturnsAsync(mockGeneration);

        // Act
        await _fixture.Service.AcceptGeneratedRecipeAsync(generationId, userId);

        // Assert
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.MarkAsAcceptedAsync(generationId, recipeId), Times.Once);
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
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, user1))
            .ReturnsAsync(mockGeneration);

        // Setup returns null for user2 (not their generation)
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, user2))
            .ReturnsAsync((RecipesGenerationsSelect?)null);

        // Act & Assert - user2 cannot access user1's generation
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _fixture.Service.AcceptGeneratedRecipeAsync(generationId, user2));

        Assert.Equal("Generation not found", exception.Message);

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, user2), Times.Once);
        _fixture.MockRecipeRepository.Verify(r => r.CreateRecipeAsync(It.IsAny<RecipesInsert>()), Times.Never);
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

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.UpdateRejectReasonAsync(generationId, rejectReasonId))
            .ReturnsAsync(updatedGeneration);

        // Act
        await _fixture.Service.RejectGeneratedRecipeAsync(generationId, rejectReasonId, userId);

        // Assert
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.UpdateRejectReasonAsync(generationId, rejectReasonId), Times.Once);
    }

    [Fact]
    public async Task RejectGeneratedRecipeAsync_WithGenerationNotFound_ThrowsArgumentException()
    {
        // Arrange
        var generationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var rejectReasonId = (short)1;

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync((RecipesGenerationsSelect?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _fixture.Service.RejectGeneratedRecipeAsync(generationId, rejectReasonId, userId));

        Assert.Equal("Generation not found", exception.Message);

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.UpdateRejectReasonAsync(It.IsAny<Guid>(), It.IsAny<short>()), Times.Never);
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

        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, userId))
            .ReturnsAsync(mockGeneration);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _fixture.Service.RejectGeneratedRecipeAsync(generationId, rejectReasonId, userId));

        Assert.Equal("Already rejected", exception.Message);

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, userId), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.UpdateRejectReasonAsync(It.IsAny<Guid>(), It.IsAny<short>()), Times.Never);
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
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, user1))
            .ReturnsAsync(mockGeneration);

        // Setup returns null for user2 (not their generation)
        _fixture.MockRecipesGenerationsRepository
            .Setup(r => r.GetByIdAsync(generationId, user2))
            .ReturnsAsync((RecipesGenerationsSelect?)null);

        // Act & Assert - user2 cannot access user1's generation
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _fixture.Service.RejectGeneratedRecipeAsync(generationId, rejectReasonId, user2));

        Assert.Equal("Generation not found", exception.Message);

        _fixture.MockRecipesGenerationsRepository.Verify(r => r.GetByIdAsync(generationId, user2), Times.Once);
        _fixture.MockRecipesGenerationsRepository.Verify(r => r.UpdateRejectReasonAsync(It.IsAny<Guid>(), It.IsAny<short>()), Times.Never);
    }

}

