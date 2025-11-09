using Moq;
using PantryPal.Data;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.ViewModels;
using PantryPal.Mobile.Models;

namespace PantryPal.Mobile.UnitTests.ViewModels;

public class RecipeDetailViewModelTests
{
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly RecipeDetailViewModel _viewModel;

    public RecipeDetailViewModelTests()
    {
        _mockNavigationService = new Mock<INavigationService>();
        _viewModel = new RecipeDetailViewModel(_mockNavigationService.Object);
    }

    [Fact]
    public void Constructor_WithNavigationService_InitializesCorrectly()
    {
        // Arrange & Act
        var viewModel = new RecipeDetailViewModel(_mockNavigationService.Object);

        // Assert
        Assert.NotNull(viewModel);
        Assert.Equal("Recipe", viewModel.RecipeTitle);
        Assert.Equal("Loading recipe...", viewModel.RecipeMarkdownContent);
        Assert.Null(viewModel.Recipe);
    }

    [Fact]
    public void OnRecipeChanged_WithValidRecipe_SetsPropertiesCorrectly()
    {
        // Arrange
        var recipeDto = new RecipeDto("1", "# Spaghetti Carbonara\n\nA delicious pasta dish...", "2023-01-01T00:00:00Z", "2023-01-01T00:00:00Z");
        var recipe = new SavedRecipeItemViewModel(recipeDto);

        // Act
        _viewModel.Recipe = recipe;

        // Assert
        Assert.Equal(recipe, _viewModel.Recipe);
        Assert.Equal("Spaghetti Carbonara", _viewModel.RecipeTitle);
        Assert.Equal(recipe.RecipeText, _viewModel.RecipeMarkdownContent);
    }

    [Fact]
    public void OnRecipeChanged_WithNullRecipe_SetsErrorMessage()
    {
        // Arrange - Create a fresh view model instance and set a recipe first
        var viewModel = new RecipeDetailViewModel(_mockNavigationService.Object);
        var recipeDto = new RecipeDto("1", "# Test Recipe\nContent", "2023-01-01T00:00:00Z", "2023-01-01T00:00:00Z");
        var recipe = new SavedRecipeItemViewModel(recipeDto);

        // Act - Set to valid recipe first, then to null
        viewModel.Recipe = recipe;
        viewModel.Recipe = null!;

        // Assert
        Assert.Null(viewModel.Recipe);
        Assert.Equal("Recipe", viewModel.RecipeTitle); // Default title
        Assert.Equal("Error: Could not load recipe.", viewModel.RecipeMarkdownContent);
    }

    [Theory]
    [InlineData("# Simple Title\nContent", "Simple Title")]
    [InlineData("## Second Level Title\nContent", "Second Level Title")]
    [InlineData("### Third Level Title\nContent", "Third Level Title")]
    [InlineData("#   Spaced Title   \nContent", "Spaced Title")]
    [InlineData("No hash prefix\nContent", "No hash prefix")]
    [InlineData("   \n# Valid Title\nContent", "Valid Title")]
    public void OnRecipeChanged_VariousMarkdownTitles_SetsCorrectTitle(string recipeText, string expectedTitle)
    {
        // Arrange
        var recipeDto = new RecipeDto("1", recipeText, "2023-01-01T00:00:00Z", "2023-01-01T00:00:00Z");
        var recipe = new SavedRecipeItemViewModel(recipeDto);

        // Act
        _viewModel.Recipe = recipe;

        // Assert
        Assert.Equal(expectedTitle, _viewModel.RecipeTitle);
        Assert.Equal(recipeText, _viewModel.RecipeMarkdownContent);
    }

    [Theory]
    [InlineData("", "Recipe")] // Empty markdown, should use default title
    [InlineData("   ", "Recipe")] // Whitespace only, should use default title
    [InlineData("\n\n\n", "Recipe")] // Only newlines, should use default title
    public void OnRecipeChanged_EmptyOrWhitespaceMarkdown_SetsDefaultTitle(string recipeText, string expectedTitle)
    {
        // Arrange
        var recipeDto = new RecipeDto("1", recipeText, "2023-01-01T00:00:00Z", "2023-01-01T00:00:00Z");
        var recipe = new SavedRecipeItemViewModel(recipeDto);

        // Act
        _viewModel.Recipe = recipe;

        // Assert
        Assert.Equal(expectedTitle, _viewModel.RecipeTitle);
        Assert.Equal(recipeText, _viewModel.RecipeMarkdownContent);
    }

    [Fact]
    public async Task CloseCommand_CallsNavigationService()
    {
        // Arrange

        // Act
        await _viewModel.CloseCommand.ExecuteAsync(null);

        // Assert
        _mockNavigationService.Verify(s => s.GoToAsync("..", It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public void Recipe_Property_GetSet_WorksCorrectly()
    {
        // Arrange
        var recipeDto = new RecipeDto("1", "Test content", "2023-01-01T00:00:00Z", "2023-01-01T00:00:00Z");
        var recipe = new SavedRecipeItemViewModel(recipeDto);

        // Act
        _viewModel.Recipe = recipe;

        // Assert
        Assert.Equal(recipe, _viewModel.Recipe);
    }

    [Fact]
    public void RecipeTitle_Property_GetSet_WorksCorrectly()
    {
        // Arrange
        var expectedTitle = "Test Recipe Title";

        // Act
        _viewModel.RecipeTitle = expectedTitle;

        // Assert
        Assert.Equal(expectedTitle, _viewModel.RecipeTitle);
    }

    [Fact]
    public void RecipeMarkdownContent_Property_GetSet_WorksCorrectly()
    {
        // Arrange
        var expectedContent = "# Test Recipe\n\nContent here...";

        // Act
        _viewModel.RecipeMarkdownContent = expectedContent;

        // Assert
        Assert.Equal(expectedContent, _viewModel.RecipeMarkdownContent);
    }

    [Fact]
    public void RecipeDetailViewModel_InheritsFromObservableObject()
    {
        // Arrange & Act & Assert
        Assert.IsAssignableFrom<CommunityToolkit.Mvvm.ComponentModel.ObservableObject>(_viewModel);
    }
}
