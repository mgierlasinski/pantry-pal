using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryPal.Data;
using PantryPal.Mobile.Models;
using PantryPal.Mobile.Services;
using System.Collections.ObjectModel;

namespace PantryPal.Mobile.ViewModels;

/// <summary>
/// ViewModel for the Saved Recipes page
/// Manages the state and business logic for displaying and managing saved recipes
/// </summary>
public partial class SavedRecipesViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;

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
    public SavedRecipesViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
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
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            await Toast.Make($"Failed to load recipes: {ex.Message}").Show();
        }
        catch (Exception ex)
        {
            await Toast.Make($"An error occurred: {ex.Message}").Show();
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
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            // Reset page counter on failure to allow retry
            _currentPage--;
            await Toast.Make($"Failed to load more recipes: {ex.Message}").Show();
        }
        catch (Exception ex)
        {
            // Reset page counter on failure to allow retry
            _currentPage--;
            await Toast.Make($"An error occurred: {ex.Message}").Show();
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

        var confirm = await Shell.Current.DisplayAlert(
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
            await Toast.Make("Recipe was already deleted.").Show();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            await Toast.Make($"Failed to delete recipe: {ex.Message}").Show();
        }
        catch (Exception ex)
        {
            await Toast.Make($"An error occurred: {ex.Message}").Show();
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

        await Shell.Current.GoToAsync("RecipeDetailPage", new Dictionary<string, object>
        {
            { "Recipe", selectedRecipe }
        });
    }

    /// <summary>
    /// Determines if a recipe can be deleted
    /// </summary>
    private bool CanDeleteRecipe(string recipeId) => !IsBusy && !string.IsNullOrEmpty(recipeId);
}
