using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryPal.Mobile.Models;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.Views;
using System.Collections.ObjectModel;

namespace PantryPal.Mobile.ViewModels;

/// <summary>
/// ViewModel for the Saved Recipes page
/// Manages the state and business logic for displaying and managing saved recipes
/// </summary>
public partial class SavedRecipesViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly IDisplayService _displayService;
    private readonly INavigationService _navigationService;

    // Pagination state
    private int _currentPage = 1;
    private int _totalItems;

    // Observable properties for UI binding
    [ObservableProperty]
    private ObservableCollection<SavedRecipeItemViewModel> _recipes = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    private bool _isLoadingMore;

    public bool IsBusy => IsLoading || IsLoadingMore;

    /// <summary>
    /// Constructor that injects the recipe service
    /// </summary>
    public SavedRecipesViewModel(IRecipeService recipeService, IDisplayService displayService, INavigationService navigationService)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    /// <summary>
    /// Command executed when the page appears - loads the first page of recipes
    /// </summary>
    [RelayCommand]
    public async Task LoadItemsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsLoading = true;

            _currentPage = 1;
            var response = await _recipeService.GetRecipesAsync(_currentPage, 20);
            _totalItems = response.Total;

            Recipes.Clear();

            foreach (var recipeDto in response.Items)
            {
                var recipeViewModel = new SavedRecipeItemViewModel(recipeDto);
                Recipes.Add(recipeViewModel);
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _navigationService.GoToAsync(AppShell.LoginRoute);
        }
        catch (HttpRequestException ex)
        {
            await _displayService.ShowToast($"Failed to load recipes: {ex.Message}");
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"An error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command executed when the user scrolls to the end of the list - loads more recipes
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanLoadMoreItems))]
    public async Task LoadMoreItemsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsLoadingMore = true;

            _currentPage++;
            var response = await _recipeService.GetRecipesAsync(_currentPage, 20);

            foreach (var recipeDto in response.Items)
            {
                var recipeViewModel = new SavedRecipeItemViewModel(recipeDto);
                Recipes.Add(recipeViewModel);
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _navigationService.GoToAsync(AppShell.LoginRoute);
        }
        catch (HttpRequestException ex)
        {
            // Reset page counter on failure to allow retry
            _currentPage--;
            await _displayService.ShowToast($"Failed to load more recipes: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Reset page counter on failure to allow retry
            _currentPage--;
            await _displayService.ShowToast($"An error occurred: {ex.Message}");
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    /// <summary>
    /// Command executed when the user confirms deletion of a recipe
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteRecipe))]
    public async Task DeleteRecipeAsync(string recipeId)
    {
        if (string.IsNullOrEmpty(recipeId) || IsBusy)
            return;

        var recipe = Recipes.FirstOrDefault(r => r.Id == recipeId);
        if (recipe == null)
            return;

        var confirm = await _displayService.DisplayAlert(
            "Delete Recipe",
            $"Are you sure you want to delete '{recipe.Title}'?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        try
        {
            IsLoading = true;

            await _recipeService.DeleteRecipeAsync(recipeId);

            // Remove from the collection
            Recipes.Remove(recipe);
            _totalItems--;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Recipe was already deleted - remove from UI
            Recipes.Remove(recipe);
            _totalItems--;
            await _displayService.ShowToast("Recipe was already deleted.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _navigationService.GoToAsync(AppShell.LoginRoute);
        }
        catch (HttpRequestException ex)
        {
            await _displayService.ShowToast($"Failed to delete recipe: {ex.Message}");
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"An error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Determines if more items can be loaded
    /// </summary>
    private bool CanLoadMoreItems() => !IsBusy && Recipes.Count < _totalItems;

    /// <summary>
    /// Command executed when a recipe is selected for viewing details
    /// </summary>
    [RelayCommand]
    public async Task SelectRecipeAsync(SavedRecipeItemViewModel selectedRecipe)
    {
        if (selectedRecipe == null || IsBusy)
            return;

        await _navigationService.GoToAsync(nameof(RecipeDetailPage), new Dictionary<string, object>
        {
            { "Recipe", selectedRecipe }
        });
    }

    /// <summary>
    /// Determines if a recipe can be deleted
    /// </summary>
    private bool CanDeleteRecipe(string recipeId) => !IsBusy && !string.IsNullOrEmpty(recipeId);
}
