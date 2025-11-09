using Microsoft.Extensions.Logging;
using Moq;
using PantryPal.Api.Services;
using PantryPal.Api.Services.OpenRouter;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Services;

/// <summary>
/// Unit tests for AIRecipeGeneratorService
/// </summary>
public class AIRecipeGeneratorServiceTests
{
    private readonly Mock<IOpenRouterService> _mockOpenRouterService;
    private readonly Mock<ILogger<AIRecipeGeneratorService>> _mockLogger;
    private readonly AIRecipeGeneratorService _service;

    public AIRecipeGeneratorServiceTests()
    {
        _mockOpenRouterService = new Mock<IOpenRouterService>();
        _mockLogger = new Mock<ILogger<AIRecipeGeneratorService>>();
        _service = new AIRecipeGeneratorService(_mockOpenRouterService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateAsync_ValidPrompt_ReturnsMarkdownRecipe()
    {
        // Arrange
        var prompt = "Ingredients: chicken, rice, vegetables. Preferences: healthy, quick";
        var mockRecipe = new AIRecipeDto(
            RecipeName: "Quick Chicken Stir-Fry",
            Description: "A healthy and quick stir-fry dish perfect for busy weeknights",
            PrepTimeMinutes: 15,
            CookTimeMinutes: 20,
            Servings: 4,
            Ingredients: new List<string>
            {
                "400g chicken breast, sliced",
                "2 cups rice",
                "2 cups mixed vegetables",
                "2 tbsp soy sauce"
            },
            Instructions: new List<string>
            {
                "Cook rice according to package instructions",
                "Heat oil in a large pan over medium-high heat",
                "Add chicken and cook for 5-7 minutes until browned",
                "Add vegetables and cook for 3-4 minutes",
                "Add soy sauce and cooked rice, stir well",
                "Cook for another 2-3 minutes until everything is heated through"
            }
        );

        _mockOpenRouterService
            .Setup(s => s.GetStructuredResponseAsync<AIRecipeDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockRecipe);

        // Act
        var result = await _service.GenerateAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("# Quick Chicken Stir-Fry", result);
        Assert.Contains("A healthy and quick stir-fry dish", result);
        Assert.Contains("## Ingredients", result);
        Assert.Contains("## Instructions", result);
        Assert.Contains("## Notes", result);
        Assert.Contains("400g chicken breast, sliced", result);
        Assert.Contains("Cook rice according to package instructions", result);
        Assert.Contains("**Prep Time:** 15 minutes", result);
        Assert.Contains("**Cook Time:** 20 minutes", result);
        Assert.Contains("**Total Time:** 35 minutes", result);
        Assert.Contains("**Servings:** 4", result);

        _mockOpenRouterService.Verify(s =>
            s.GetStructuredResponseAsync<AIRecipeDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_OpenRouterReturnsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var prompt = "Ingredients: pasta, tomatoes";

        _mockOpenRouterService
            .Setup(s => s.GetStructuredResponseAsync<AIRecipeDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AIRecipeDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GenerateAsync(prompt));

        Assert.Contains("Failed to generate recipe", exception.Message);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task GenerateAsync_OpenRouterThrowsException_WrapsAndRethrows()
    {
        // Arrange
        var prompt = "Ingredients: fish, herbs";
        var originalException = new Exception("OpenRouter API error");

        _mockOpenRouterService
            .Setup(s => s.GetStructuredResponseAsync<AIRecipeDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(originalException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GenerateAsync(prompt));

        Assert.Contains("Failed to generate recipe", exception.Message);
        Assert.Equal(originalException, exception.InnerException);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ingredients: ")]
    [InlineData("Short")]
    public async Task GenerateAsync_VariousPromptLengths_HandlesCorrectly(string prompt)
    {
        // Arrange
        var mockRecipe = new AIRecipeDto(
            RecipeName: "Simple Recipe",
            Description: "A simple dish",
            PrepTimeMinutes: 5,
            CookTimeMinutes: 10,
            Servings: 2,
            Ingredients: new List<string> { "1 ingredient" },
            Instructions: new List<string> { "Mix and serve" }
        );

        _mockOpenRouterService
            .Setup(s => s.GetStructuredResponseAsync<AIRecipeDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockRecipe);

        // Act
        var result = await _service.GenerateAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("# Simple Recipe", result);
        _mockOpenRouterService.Verify(s =>
            s.GetStructuredResponseAsync<AIRecipeDto>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(RecipeTestData))]
    public async Task GenerateAsync_RecipeFormatsCorrectly_FormatsAsExpected(
        string testName, string prompt, AIRecipeDto mockRecipe, string expectedTitle,
        string expectedDescription, string expectedIngredient, string expectedInstruction,
        string expectedPrepTime, string expectedCookTime, string expectedTotalTime, string expectedServings)
    {
        // Arrange
        _mockOpenRouterService
            .Setup(s => s.GetStructuredResponseAsync<AIRecipeDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockRecipe);

        // Act
        var result = await _service.GenerateAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(expectedTitle, result);
        Assert.Contains(expectedDescription, result);
        Assert.Contains(expectedIngredient, result);
        Assert.Contains(expectedInstruction, result);
        Assert.Contains(expectedPrepTime, result);
        Assert.Contains(expectedCookTime, result);
        Assert.Contains(expectedTotalTime, result);
        Assert.Contains(expectedServings, result);
    }

    public static IEnumerable<object[]> RecipeTestData =>
        new List<object[]>
        {
            new object[]
            {
                "ComplexRecipe",
                "Complex recipe with many ingredients",
                new AIRecipeDto(
                    RecipeName: "Gourmet Lasagna",
                    Description: "A rich and flavorful Italian classic",
                    PrepTimeMinutes: 45,
                    CookTimeMinutes: 60,
                    Servings: 8,
                    Ingredients: new List<string>
                    {
                        "1 lb ground beef",
                        "1 lb Italian sausage",
                        "2 cups ricotta cheese",
                        "2 cups mozzarella cheese",
                        "1 cup parmesan cheese",
                        "12 lasagna noodles",
                        "2 jars marinara sauce",
                        "2 eggs",
                        "1/4 cup fresh basil",
                        "1/4 cup fresh parsley"
                    },
                    Instructions: new List<string>
                    {
                        "Preheat oven to 375°F",
                        "Brown ground beef and sausage in a large skillet",
                        "Cook lasagna noodles according to package directions",
                        "Mix ricotta, eggs, and herbs in a bowl",
                        "Layer sauce, noodles, meat, cheese mixture in baking dish",
                        "Repeat layers ending with cheese on top",
                        "Bake for 45-50 minutes until bubbly",
                        "Let rest for 15 minutes before serving"
                    }
                ),
                "# Gourmet Lasagna",
                "A rich and flavorful Italian classic",
                "1 lb ground beef",
                "1. Preheat oven to 375°F",
                "**Prep Time:** 45 minutes",
                "**Cook Time:** 60 minutes",
                "**Total Time:** 105 minutes",
                "**Servings:** 8"
            },
            new object[]
            {
                "MinimalRecipe",
                "Simple ingredients",
                new AIRecipeDto(
                    RecipeName: "Simple Salad",
                    Description: "Fresh and simple",
                    PrepTimeMinutes: 5,
                    CookTimeMinutes: 0,
                    Servings: 1,
                    Ingredients: new List<string> { "lettuce", "tomato" },
                    Instructions: new List<string> { "Mix together" }
                ),
                "# Simple Salad",
                "Fresh and simple",
                "lettuce",
                "1. Mix together",
                "**Prep Time:** 5 minutes",
                "**Cook Time:** 0 minutes",
                "**Total Time:** 5 minutes",
                "**Servings:** 1"
            }
        };

    [Fact]
    public async Task GenerateAsync_NullConstructorArguments_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new AIRecipeGeneratorService(null!, _mockLogger.Object));
        Assert.Throws<ArgumentNullException>(() =>
            new AIRecipeGeneratorService(_mockOpenRouterService.Object, null!));
    }

    [Fact]
    public async Task GenerateAsync_LogsPromptLength_LogsInformation()
    {
        // Arrange
        var prompt = "Test prompt for logging";
        var mockRecipe = new AIRecipeDto(
            RecipeName: "Test Recipe",
            Description: "Test description",
            PrepTimeMinutes: 10,
            CookTimeMinutes: 15,
            Servings: 2,
            Ingredients: new List<string> { "ingredient1", "ingredient2" },
            Instructions: new List<string> { "step1", "step2" }
        );

        _mockOpenRouterService
            .Setup(s => s.GetStructuredResponseAsync<AIRecipeDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockRecipe);

        // Act
        await _service.GenerateAsync(prompt);

        // Assert - Verify logging calls
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Generating recipe with prompt length")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Successfully generated recipe")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_ExceptionDuringGeneration_LogsError()
    {
        // Arrange
        var prompt = "Test prompt";
        var testException = new Exception("Test error");

        _mockOpenRouterService
            .Setup(s => s.GetStructuredResponseAsync<AIRecipeDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(testException);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GenerateAsync(prompt));

        // Assert - Verify error logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error generating recipe")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_CancellationTokenCancelled_WrapsOperationCanceledException()
    {
        // Arrange
        var prompt = "Test prompt";

        _mockOpenRouterService
            .Setup(s => s.GetStructuredResponseAsync<AIRecipeDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("Operation was cancelled"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GenerateAsync(prompt));

        Assert.Contains("Failed to generate recipe", exception.Message);
        Assert.IsType<OperationCanceledException>(exception.InnerException);
        Assert.Contains("Operation was cancelled", exception.InnerException?.Message);
    }

    [Fact]
    public async Task GenerateAsync_CorrectParametersPassedToOpenRouter_PassesExpectedValues()
    {
        // Arrange
        var prompt = "Ingredients: chicken, rice. Preferences: healthy";
        var expectedUserMessage = $"Please create a recipe using the following information: {prompt}";
        var mockRecipe = new AIRecipeDto(
            RecipeName: "Test Recipe",
            Description: "Test description",
            PrepTimeMinutes: 10,
            CookTimeMinutes: 20,
            Servings: 2,
            Ingredients: new List<string> { "chicken", "rice" },
            Instructions: new List<string> { "Cook chicken", "Cook rice" }
        );

        string? capturedSystemMessage = null;
        string? capturedUserMessage = null;
        object? capturedSchema = null;

        _mockOpenRouterService
            .Setup(s => s.GetStructuredResponseAsync<AIRecipeDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, object, CancellationToken>((sysMsg, userMsg, schema, token) =>
            {
                capturedSystemMessage = sysMsg;
                capturedUserMessage = userMsg;
                capturedSchema = schema;
            })
            .ReturnsAsync(mockRecipe);

        // Act
        await _service.GenerateAsync(prompt);

        // Assert
        Assert.NotNull(capturedSystemMessage);
        Assert.Contains("You are a master chef", capturedSystemMessage);
        Assert.Contains("respond only with a JSON object", capturedSystemMessage);
        Assert.Contains("Use only the ingredients mentioned", capturedSystemMessage);

        Assert.Equal(expectedUserMessage, capturedUserMessage);

        Assert.NotNull(capturedSchema);
        // Verify that schema was passed (detailed structure validation would require JSON parsing)
        // The schema is an anonymous object created in AIRecipeGeneratorService.GenerateAsync
    }

    [Fact]
    public async Task GenerateAsync_VeryLongRecipeData_FormatsCorrectly()
    {
        // Arrange
        var prompt = "Complex recipe with many ingredients and steps";
        var longDescription = string.Join(" ", Enumerable.Repeat("This is a very detailed description of the dish that goes on and on.", 10));
        var manyIngredients = Enumerable.Range(1, 50).Select(i => $"Ingredient number {i} with a very long name that describes exactly what it is and how it should be prepared").ToList();
        var manyInstructions = Enumerable.Range(1, 30).Select(i => $"Step {i}: This is a very detailed instruction that explains exactly how to perform this cooking step with precise measurements and timing.").ToList();

        var mockRecipe = new AIRecipeDto(
            RecipeName: "Extremely Complex Gourmet Recipe with Very Long Name",
            Description: longDescription,
            PrepTimeMinutes: 120,
            CookTimeMinutes: 180,
            Servings: 12,
            Ingredients: manyIngredients,
            Instructions: manyInstructions
        );

        _mockOpenRouterService
            .Setup(s => s.GetStructuredResponseAsync<AIRecipeDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockRecipe);

        // Act
        var result = await _service.GenerateAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("# Extremely Complex Gourmet Recipe with Very Long Name", result);
        Assert.Contains(longDescription, result);
        Assert.Contains("## Ingredients", result);
        Assert.Contains("## Instructions", result);
        Assert.Contains("## Notes", result);

        // Verify all ingredients are present
        foreach (var ingredient in manyIngredients)
        {
            Assert.Contains(ingredient, result);
        }

        // Verify all instructions are numbered correctly
        for (int i = 0; i < manyInstructions.Count; i++)
        {
            Assert.Contains($"{i + 1}. {manyInstructions[i]}", result);
        }

        // Verify timing information
        Assert.Contains("**Prep Time:** 120 minutes", result);
        Assert.Contains("**Cook Time:** 180 minutes", result);
        Assert.Contains("**Total Time:** 300 minutes", result);
        Assert.Contains("**Servings:** 12", result);
    }
}
