using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Core;
using PantryPal.Data;
using PantryPal.Mobile.Models;
using PantryPal.Mobile.Services;
using System.Collections.ObjectModel;

namespace PantryPal.Mobile.ViewModels;

public partial class PantryPageViewModel : ObservableObject
{
    private readonly IPantryService _pantryService;
    private readonly IDisplayService _displayService;
    private readonly INavigationService _navigationService;

    // Pagination properties
    [ObservableProperty]
    private int _page = 1;

    [ObservableProperty]
    private int _pageSize = 100;

    [ObservableProperty]
    private string _sortField = "name";

    [ObservableProperty]
    private int _total;

    // State properties
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotEmpty))]
    private bool _isEmpty = true;

    // Collection
    [ObservableProperty]
    private ObservableCollection<PantryItemViewModel> _items = new();

    public bool IsNotEmpty => !IsEmpty;

    public PantryPageViewModel(IPantryService pantryService, IDisplayService displayService, INavigationService navigationService)
    {
        _pantryService = pantryService;
        _displayService = displayService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task LoadItemsAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            var response = await _pantryService.GetPantryItemsAsync(Page, PageSize, SortField);
            
            Total = response.Total;
            Items.Clear();

            foreach (var item in response.Items)
            {
                var viewModel = CreateItemViewModel(item);
                Items.Add(viewModel);
            }

            IsEmpty = Items.Count == 0;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Navigation to login should be handled by global error handler
            throw;
        }
        catch (HttpRequestException ex)
        {
            await _displayService.ShowToast($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"Failed to load items: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    public async Task AddItemAsync(string itemName)
    {
        if (!await ValidateItemName(itemName))
            return;

        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            var createDto = new PantryItemCreateDto(itemName.Trim());
            var newItem = await _pantryService.CreatePantryItemAsync(createDto);

            var viewModel = CreateItemViewModel(newItem);
            Items.Add(viewModel);

            IsEmpty = false;
            Total++;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            await _displayService.ShowToast("An item with this name already exists.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Navigation to login should be handled by global error handler
            throw;
        }
        catch (HttpRequestException ex)
        {
            await _displayService.ShowToast($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"Failed to add item: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task EditItemAsync(string itemId, string newName)
    {
         
        if (!await ValidateItemName(newName))
            return;

        if (IsLoading)
            return;

        var existingItem = Items.FirstOrDefault(i => i.Id == itemId);
        if (existingItem == null)
            return;

        var originalName = existingItem.Name;

        try
        {
            IsLoading = true;

            var updateDto = new PantryItemUpdateDto(Name: newName.Trim());
            var updatedItem = await _pantryService.UpdatePantryItemAsync(itemId, updateDto);

            existingItem.Name = updatedItem.Name;
            existingItem.IsFavorite = updatedItem.IsFavorite;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            existingItem.Name = originalName;
            await _displayService.ShowToast("An item with this name already exists.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            existingItem.Name = originalName;
            await _navigationService.GoToAsync(AppShell.LoginRoute);
        }
        catch (HttpRequestException ex)
        {
            existingItem.Name = originalName;
            await _displayService.ShowToast($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            existingItem.Name = originalName;
            await _displayService.ShowToast($"Failed to update item: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ConfirmDeleteAsync(string itemId)
    {
        if (IsLoading)
            return;

        var itemToRemove = Items.FirstOrDefault(i => i.Id == itemId);
        if (itemToRemove == null)
            return;

        try
        {
            IsLoading = true;

            await _pantryService.DeletePantryItemAsync(itemId);

            Items.Remove(itemToRemove);
            Total--;
            IsEmpty = Items.Count == 0;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Navigation to login should be handled by global error handler
            throw;
        }
        catch (HttpRequestException ex)
        {
            await _displayService.ShowToast($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"Failed to delete item: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ToggleFavoriteAsync(string itemId)
    {
        if (IsLoading)
            return;

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return;

        var originalState = item.IsFavorite;
        item.IsFavorite = !originalState;

        try
        {
            IsLoading = true;

            var updateDto = new PantryItemUpdateDto(IsFavorite: item.IsFavorite);
            var updatedItem = await _pantryService.UpdatePantryItemAsync(itemId, updateDto);

            item.IsFavorite = updatedItem.IsFavorite;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            item.IsFavorite = originalState;
            // Navigation to login should be handled by global error handler
            throw;
        }
        catch (HttpRequestException ex)
        {
            item.IsFavorite = originalState;
            await _displayService.ShowToast($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            item.IsFavorite = originalState;
            await _displayService.ShowToast($"Failed to update favorite status: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ShowAddDialogAsync()
    {
        var itemName = await _displayService.DisplayPromptAsync(
            "Add Item",
            "Enter item name:",
            accept: "Add",
            cancel: "Cancel",
            placeholder: "Item name",
            maxLength: 100,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(itemName))
            return;

        await AddItemAsync(itemName);
    }

    [RelayCommand]
    public async Task ShowEditDialogAsync(string itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return;

        var itemName = await _displayService.DisplayPromptAsync(
            "Edit Item",
            "Enter item name:",
            accept: "Save",
            cancel: "Cancel",
            placeholder: "Item name",
            maxLength: 100,
            keyboard: Keyboard.Text,
            initialValue: item.Name);

        if (string.IsNullOrWhiteSpace(itemName))
            return;

        await EditItemAsync(itemId, itemName);
    }

    [RelayCommand]
    public async Task ShowDeleteDialogAsync(string itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return;

        var confirm = await _displayService.DisplayAlert(
            "Delete Item",
            $"Are you sure you want to delete '{item.Name}'?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        await ConfirmDeleteAsync(itemId);
    }

    [RelayCommand]
    public async Task GenerateRecipeAsync()
    {
        if (IsEmpty)
        {
            await _displayService.ShowToast("Add items to your pantry before generating a recipe.");
            return;
        }

        await _navigationService.GoToAsync(nameof(Views.RecipeGenerationPage), true);
    }

    private async Task<bool> ValidateItemName(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            await _displayService.ShowToast("Item name is required.");
            return false;
        }

        if (itemName.Length > 100)
        {
            await _displayService.ShowToast("Item name must be 100 characters or less.");
            return false;
        }

        return true;
    }

    private PantryItemViewModel CreateItemViewModel(PantryItemDto dto)
    {
        var viewModel = new PantryItemViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            IsFavorite = dto.IsFavorite
        };

        viewModel.ToggleFavoriteCommand = new RelayCommand<string>(async (id) =>
        {
            if (!string.IsNullOrEmpty(id))
                await ToggleFavoriteAsync(id);
        });

        viewModel.DeleteItemCommand = new RelayCommand<string>(async (id) =>
        {
            if (!string.IsNullOrEmpty(id))
                await ShowDeleteDialogAsync(id);
        });

        viewModel.EditItemCommand = new RelayCommand<string>(async (id) =>
        {
            if (!string.IsNullOrEmpty(id))
                await ShowEditDialogAsync(id);
        });

        return viewModel;
    }
}
