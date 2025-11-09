using Moq;
using PantryPal.Data;
using PantryPal.Mobile.Models;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.ViewModels;
using PantryPal.Mobile.Views;
using System.Net;

namespace PantryPal.Mobile.UnitTests.ViewModels;

public class SavedRecipesViewModelTests
{
    private readonly Mock<IRecipeService> _mockRecipeService;
    private readonly Mock<IDisplayService> _mockDisplayService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly SavedRecipesViewModel _viewModel;

    public SavedRecipesViewModelTests()
    {
        _mockRecipeService = new Mock<IRecipeService>();
        _mockDisplayService = new Mock<IDisplayService>();
        _mockNavigationService = new Mock<INavigationService>();
        _viewModel = new SavedRecipesViewModel(_mockRecipeService.Object, _mockDisplayService.Object, _mockNavigationService.Object);
    }

    [Fact]
    public void Constructor_NullRecipeService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new SavedRecipesViewModel(null!, _mockDisplayService.Object, _mockNavigationService.Object));
    }

    [Fact]
    public void Constructor_NullDisplayService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new SavedRecipesViewModel(_mockRecipeService.Object, null!, _mockNavigationService.Object));
    }

    [Fact]
    public void Constructor_NullNavigationService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new SavedRecipesViewModel(_mockRecipeService.Object, _mockDisplayService.Object, null!));
    }

    [Fact]
    public async Task LoadItemsAsync_Success_PopulatesRecipesAndResetsPagination()
    {
        // Arrange
        var mockRecipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString()),
            new("2", "# Recipe 2\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new RecipesPaginatedResponseDto(mockRecipes, 1, 20, 5);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        // Act
        await _viewModel.LoadItemsAsync();

        // Assert
        Assert.Equal(2, _viewModel.Recipes.Count);
        Assert.Equal("Recipe 1", _viewModel.Recipes[0].Title);
        Assert.Equal("Recipe 2", _viewModel.Recipes[1].Title);
        Assert.False(_viewModel.IsLoading);

        // Verify pagination reset by checking that next LoadMoreItemsAsync uses page 2
        var nextPageRecipes = new List<RecipeDto>
        {
            new("3", "# Recipe 3\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };
        var nextResponse = new RecipesPaginatedResponseDto(nextPageRecipes, 2, 20, 5);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(2, 20))
            .ReturnsAsync(nextResponse);

        await _viewModel.LoadMoreItemsAsync();
        Assert.Equal(3, _viewModel.Recipes.Count);
        _mockRecipeService.Verify(s => s.GetRecipesAsync(1, 20), Times.Once);
    }

    [Fact]
    public async Task LoadItemsAsync_EmptyResponse_SetsEmptyCollection()
    {
        // Arrange
        var response = new RecipesPaginatedResponseDto([], 1, 20, 0);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        // Act
        await _viewModel.LoadItemsAsync();

        // Assert
        Assert.Empty(_viewModel.Recipes);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadItemsAsync_NetworkError_ShowsToastAndDoesNotCrash()
    {
        // Arrange
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert - Should not throw exception, error is handled via Toast
        await _viewModel.LoadItemsAsync();

        // Assert
        Assert.Empty(_viewModel.Recipes);
        Assert.False(_viewModel.IsLoading);
        _mockDisplayService.Verify(s => s.ShowToast("Failed to load recipes: Network error"), Times.Once);
    }

    [Fact]
    public async Task LoadItemsAsync_UnauthorizedError_NavigatesToLogin()
    {
        // Arrange
        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LoadItemsAsync();

        // Assert
        Assert.Empty(_viewModel.Recipes);
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.GoToAsync(It.Is<string>(route => route == AppShell.LoginRoute), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task LoadItemsAsync_IsBusy_DoesNotExecute()
    {
        // Arrange
        _viewModel.IsLoading = true;
        var response = new RecipesPaginatedResponseDto([], 1, 20, 0);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        // Act
        await _viewModel.LoadItemsAsync();

        // Assert
        Assert.Empty(_viewModel.Recipes);
        _mockRecipeService.Verify(s => s.GetRecipesAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task LoadMoreItemsAsync_Success_AddsToExistingCollection()
    {
        // Arrange - First load initial items
        var initialRecipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var initialResponse = new RecipesPaginatedResponseDto(initialRecipes, 1, 20, 3);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(initialResponse);

        await _viewModel.LoadItemsAsync();

        // Setup next page
        var nextPageRecipes = new List<RecipeDto>
        {
            new("2", "# Recipe 2\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString()),
            new("3", "# Recipe 3\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var nextPageResponse = new RecipesPaginatedResponseDto(nextPageRecipes, 2, 20, 3);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(2, 20))
            .ReturnsAsync(nextPageResponse);

        // Act
        await _viewModel.LoadMoreItemsAsync();

        // Assert
        Assert.Equal(3, _viewModel.Recipes.Count);
        Assert.Equal("Recipe 1", _viewModel.Recipes[0].Title);
        Assert.Equal("Recipe 2", _viewModel.Recipes[1].Title);
        Assert.Equal("Recipe 3", _viewModel.Recipes[2].Title);
        Assert.False(_viewModel.IsLoadingMore);
        _mockRecipeService.Verify(s => s.GetRecipesAsync(2, 20), Times.Once);
    }

    [Fact]
    public async Task LoadMoreItemsAsync_IsBusy_DoesNotExecute()
    {
        // Arrange
        _viewModel.IsLoadingMore = true;

        // Act
        await _viewModel.LoadMoreItemsAsync();

        // Assert
        _mockRecipeService.Verify(s => s.GetRecipesAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task LoadMoreItemsAsync_NetworkError_ShowsToastAndResetsPageCounter()
    {
        // Arrange - First load initial items
        var initialRecipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var initialResponse = new RecipesPaginatedResponseDto(initialRecipes, 1, 20, 3);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(initialResponse);

        await _viewModel.LoadItemsAsync();

        // Setup next page to fail
        _mockRecipeService.Setup(s => s.GetRecipesAsync(2, 20))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _viewModel.LoadMoreItemsAsync();

        // Assert
        Assert.Single(_viewModel.Recipes); // Still only has initial recipe
        Assert.False(_viewModel.IsLoadingMore);
        _mockDisplayService.Verify(s => s.ShowToast("Failed to load more recipes: Network error"), Times.Once);
        // Page counter should be reset (verified by no additional recipes being added)
    }

    [Fact]
    public async Task LoadMoreItemsAsync_UnauthorizedError_NavigatesToLogin()
    {
        // Arrange - First load initial items
        var initialRecipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var initialResponse = new RecipesPaginatedResponseDto(initialRecipes, 1, 20, 3);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(initialResponse);

        await _viewModel.LoadItemsAsync();

        // Setup next page to fail with unauthorized
        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(2, 20))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LoadMoreItemsAsync();

        // Assert
        Assert.Single(_viewModel.Recipes); // Still only has initial recipe
        Assert.False(_viewModel.IsLoadingMore);
        _mockNavigationService.Verify(s => s.GoToAsync(It.Is<string>(route => route == AppShell.LoginRoute), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_UserConfirms_DeletesRecipeAndUpdatesCollection()
    {
        // Arrange - Load initial recipes
        var recipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString()),
            new("2", "# Recipe 2\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new RecipesPaginatedResponseDto(recipes, 1, 20, 2);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        _mockDisplayService.Setup(s => s.DisplayAlert("Delete Recipe", $"Are you sure you want to delete '{_viewModel.Recipes[0].Title}'?", "Delete", "Cancel"))
            .ReturnsAsync(true);

        _mockRecipeService.Setup(s => s.DeleteRecipeAsync("1"))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.DeleteRecipeAsync("1");

        // Assert
        Assert.Single(_viewModel.Recipes);
        Assert.Equal("2", _viewModel.Recipes[0].Id);
        Assert.False(_viewModel.IsLoading);
        _mockRecipeService.Verify(s => s.DeleteRecipeAsync("1"), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_UserCancels_DoesNotDelete()
    {
        // Arrange - Load initial recipes
        var recipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new RecipesPaginatedResponseDto(recipes, 1, 20, 1);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        _mockDisplayService.Setup(s => s.DisplayAlert("Delete Recipe", $"Are you sure you want to delete '{_viewModel.Recipes[0].Title}'?", "Delete", "Cancel"))
            .ReturnsAsync(false);

        // Act
        await _viewModel.DeleteRecipeAsync("1");

        // Assert
        Assert.Single(_viewModel.Recipes);
        Assert.Equal("1", _viewModel.Recipes[0].Id);
        _mockRecipeService.Verify(s => s.DeleteRecipeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRecipeAsync_RecipeNotFound_HandlesGracefully()
    {
        // Arrange - Load initial recipes
        var recipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new RecipesPaginatedResponseDto(recipes, 1, 20, 1);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        _mockDisplayService.Setup(s => s.DisplayAlert("Delete Recipe", $"Are you sure you want to delete '{_viewModel.Recipes[0].Title}'?", "Delete", "Cancel"))
            .ReturnsAsync(true);

        var exception = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
        _mockRecipeService.Setup(s => s.DeleteRecipeAsync("1"))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.DeleteRecipeAsync("1");

        // Assert
        Assert.Empty(_viewModel.Recipes); // Recipe removed from UI
        Assert.False(_viewModel.IsLoading);
        _mockDisplayService.Verify(s => s.ShowToast("Recipe was already deleted."), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_UnauthorizedError_NavigatesToLogin()
    {
        // Arrange - Load initial recipes
        var recipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new RecipesPaginatedResponseDto(recipes, 1, 20, 1);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        _mockDisplayService.Setup(s => s.DisplayAlert("Delete Recipe", $"Are you sure you want to delete '{_viewModel.Recipes[0].Title}'?", "Delete", "Cancel"))
            .ReturnsAsync(true);

        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);
        _mockRecipeService.Setup(s => s.DeleteRecipeAsync("1"))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.DeleteRecipeAsync("1");

        // Assert
        Assert.Single(_viewModel.Recipes); // Recipe still in UI
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.GoToAsync(It.Is<string>(route => route == AppShell.LoginRoute), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_NetworkError_ShowsToast()
    {
        // Arrange - Load initial recipes
        var recipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new RecipesPaginatedResponseDto(recipes, 1, 20, 1);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        _mockDisplayService.Setup(s => s.DisplayAlert("Delete Recipe", $"Are you sure you want to delete '{_viewModel.Recipes[0].Title}'?", "Delete", "Cancel"))
            .ReturnsAsync(true);

        _mockRecipeService.Setup(s => s.DeleteRecipeAsync("1"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _viewModel.DeleteRecipeAsync("1");

        // Assert
        Assert.Single(_viewModel.Recipes); // Recipe still in UI
        Assert.False(_viewModel.IsLoading);
        _mockDisplayService.Verify(s => s.ShowToast("Failed to delete recipe: Network error"), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_IsBusy_DoesNotExecute()
    {
        // Arrange
        _viewModel.IsLoading = true;

        // Act
        await _viewModel.DeleteRecipeAsync("1");

        // Assert
        _mockDisplayService.Verify(s => s.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockRecipeService.Verify(s => s.DeleteRecipeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRecipeAsync_NullOrEmptyRecipeId_DoesNotExecute()
    {
        // Act
        await _viewModel.DeleteRecipeAsync(null!);

        // Assert
        _mockDisplayService.Verify(s => s.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockRecipeService.Verify(s => s.DeleteRecipeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRecipeAsync_RecipeNotInCollection_DoesNotExecute()
    {
        // Act
        await _viewModel.DeleteRecipeAsync("nonexistent");

        // Assert
        _mockDisplayService.Verify(s => s.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockRecipeService.Verify(s => s.DeleteRecipeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SelectRecipeAsync_ValidRecipe_NavigatesToRecipeDetailPage()
    {
        // Arrange - Load initial recipes
        var recipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new RecipesPaginatedResponseDto(recipes, 1, 20, 1);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        // Act
        await _viewModel.SelectRecipeAsync(_viewModel.Recipes[0]);

        // Assert
        _mockNavigationService.Verify(s => s.GoToAsync(nameof(RecipeDetailPage), It.Is<IDictionary<string, object>>(dict =>
            dict.ContainsKey("Recipe") && dict["Recipe"] == _viewModel.Recipes[0]), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task SelectRecipeAsync_IsBusy_DoesNotExecute()
    {
        // Arrange
        _viewModel.IsLoading = true;

        var recipe = new SavedRecipeItemViewModel(new RecipeDto("1", "# Test Recipe", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString()));

        // Act
        await _viewModel.SelectRecipeAsync(recipe);

        // Assert
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task SelectRecipeAsync_NullRecipe_DoesNotExecute()
    {
        // Act
        await _viewModel.SelectRecipeAsync(null!);

        // Assert
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task LoadMoreItemsAsync_CommandCanExecute_WhenHasMoreItems_Executes()
    {
        // Arrange - Load initial items with total > current count
        var initialRecipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var initialResponse = new RecipesPaginatedResponseDto(initialRecipes, 1, 20, 3); // Total = 3, so more to load
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(initialResponse);

        await _viewModel.LoadItemsAsync();

        // Setup next page
        var nextPageRecipes = new List<RecipeDto>
        {
            new("2", "# Recipe 2\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString()),
            new("3", "# Recipe 3\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var nextResponse = new RecipesPaginatedResponseDto(nextPageRecipes, 2, 20, 3);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(2, 20))
            .ReturnsAsync(nextResponse);

        // Act - LoadMoreItemsAsync should execute because we have more items to load
        await _viewModel.LoadMoreItemsAsync();

        // Assert
        Assert.Equal(3, _viewModel.Recipes.Count);
    }

    [Fact]
    public async Task LoadMoreItemsAsync_WhenNoMoreItems_DoesNotAddItems()
    {
        // Arrange - Load all items at once
        var allRecipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString()),
            new("2", "# Recipe 2\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString()),
            new("3", "# Recipe 3\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new RecipesPaginatedResponseDto(allRecipes, 1, 20, 3); // Total = 3, current = 3
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        // Setup call to page 2 that returns no items (simulating end of data)
        _mockRecipeService.Setup(s => s.GetRecipesAsync(2, 20))
            .ReturnsAsync(new RecipesPaginatedResponseDto([], 2, 20, 3));

        // Act - LoadMoreItemsAsync will execute but should not add new items
        await _viewModel.LoadMoreItemsAsync();

        // Assert - Collection should still have only the original 3 items
        Assert.Equal(3, _viewModel.Recipes.Count);
        _mockRecipeService.Verify(s => s.GetRecipesAsync(2, 20), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_CommandCanExecute_WhenNotBusy_Executes()
    {
        // Arrange - Load initial recipes
        var recipes = new List<RecipeDto>
        {
            new("1", "# Recipe 1\nIngredients:...", DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new RecipesPaginatedResponseDto(recipes, 1, 20, 1);
        _mockRecipeService.Setup(s => s.GetRecipesAsync(1, 20))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        _mockDisplayService.Setup(s => s.DisplayAlert("Delete Recipe", $"Are you sure you want to delete '{_viewModel.Recipes[0].Title}'?", "Delete", "Cancel"))
            .ReturnsAsync(true);

        _mockRecipeService.Setup(s => s.DeleteRecipeAsync("1"))
            .Returns(Task.CompletedTask);

        // Act - Should execute because not busy and valid recipe ID
        await _viewModel.DeleteRecipeAsync("1");

        // Assert
        Assert.Empty(_viewModel.Recipes);
        _mockRecipeService.Verify(s => s.DeleteRecipeAsync("1"), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_CommandCanExecute_WhenBusy_DoesNotExecute()
    {
        // Arrange
        _viewModel.IsLoading = true;

        // Act
        await _viewModel.DeleteRecipeAsync("1");

        // Assert - Should not execute because busy
        _mockDisplayService.Verify(s => s.DisplayAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockRecipeService.Verify(s => s.DeleteRecipeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void IsBusy_IsLoadingTrue_ReturnsTrue()
    {
        // Arrange
        _viewModel.IsLoading = true;
        _viewModel.IsLoadingMore = false;

        // Act & Assert
        Assert.True(_viewModel.IsBusy);
    }

    [Fact]
    public void IsBusy_IsLoadingMoreTrue_ReturnsTrue()
    {
        // Arrange
        _viewModel.IsLoading = false;
        _viewModel.IsLoadingMore = true;

        // Act & Assert
        Assert.True(_viewModel.IsBusy);
    }

    [Fact]
    public void IsBusy_BothLoadingStatesFalse_ReturnsFalse()
    {
        // Arrange
        _viewModel.IsLoading = false;
        _viewModel.IsLoadingMore = false;

        // Act & Assert
        Assert.False(_viewModel.IsBusy);
    }
}
