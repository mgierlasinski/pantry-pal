using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryPal.Data;
using PantryPal.Mobile.Models;
using PantryPal.Mobile.Services;
using System.Collections.ObjectModel;

namespace PantryPal.Mobile.ViewModels;

public partial class PantryPageViewModel : ObservableObject
{
    private readonly IPantryService _pantryService;

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
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotEmpty))]
    private bool _isEmpty = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _showErrorSnackbar;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // Dialog properties
    [ObservableProperty]
    private string _dialogItemName = string.Empty;

    [ObservableProperty]
    private bool _dialogIsEdit;

    [ObservableProperty]
    private string _selectedItemId = string.Empty;

    // Collection
    [ObservableProperty]
    private ObservableCollection<PantryItemViewModel> _items = new();

    public bool IsNotEmpty => !IsEmpty;

    public PantryPageViewModel(IPantryService pantryService)
    {
        _pantryService = pantryService;
    }

    [RelayCommand]
    public async Task LoadItemsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

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
            ErrorMessage = "Session expired. Please log in again.";
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Network error: {ex.Message}";
            ShowErrorSnackbar = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load items: {ex.Message}";
            ShowErrorSnackbar = true;
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task AddItemAsync()
    {
        if (string.IsNullOrWhiteSpace(DialogItemName))
        {
            await Shell.Current.DisplayAlert("Validation Error", "Item name is required.", "OK");
            return;
        }

        if (DialogItemName.Length > 100)
        {
            await Shell.Current.DisplayAlert("Validation Error", "Item name must be 100 characters or less.", "OK");
            return;
        }

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var createDto = new PantryItemCreateDto(DialogItemName.Trim());
            var newItem = await _pantryService.CreatePantryItemAsync(createDto);

            var viewModel = CreateItemViewModel(newItem);
            Items.Add(viewModel);
            
            IsEmpty = false;
            Total++;

            DialogItemName = string.Empty;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            ErrorMessage = "An item with this name already exists.";
            await Shell.Current.DisplayAlert("Duplicate Item", ErrorMessage, "OK");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ErrorMessage = "Session expired. Please log in again.";
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Network error: {ex.Message}";
            ShowErrorSnackbar = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to add item: {ex.Message}";
            ShowErrorSnackbar = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task EditItemAsync()
    {
        if (string.IsNullOrWhiteSpace(DialogItemName))
        {
            await Shell.Current.DisplayAlert("Validation Error", "Item name is required.", "OK");
            return;
        }

        if (DialogItemName.Length > 100)
        {
            await Shell.Current.DisplayAlert("Validation Error", "Item name must be 100 characters or less.", "OK");
            return;
        }

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var updateDto = new PantryItemUpdateDto(Name: DialogItemName.Trim());
            var updatedItem = await _pantryService.UpdatePantryItemAsync(SelectedItemId, updateDto);

            var existingItem = Items.FirstOrDefault(i => i.Id == SelectedItemId);
            if (existingItem != null)
            {
                existingItem.Name = updatedItem.Name;
                existingItem.IsFavorite = updatedItem.IsFavorite;
            }

            DialogItemName = string.Empty;
            SelectedItemId = string.Empty;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            ErrorMessage = "An item with this name already exists.";
            await Shell.Current.DisplayAlert("Duplicate Item", ErrorMessage, "OK");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ErrorMessage = "Session expired. Please log in again.";
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Network error: {ex.Message}";
            ShowErrorSnackbar = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to update item: {ex.Message}";
            ShowErrorSnackbar = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ConfirmDeleteAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            await _pantryService.DeletePantryItemAsync(SelectedItemId);

            var itemToRemove = Items.FirstOrDefault(i => i.Id == SelectedItemId);
            if (itemToRemove != null)
            {
                Items.Remove(itemToRemove);
                Total--;
                IsEmpty = Items.Count == 0;
            }

            SelectedItemId = string.Empty;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            ErrorMessage = "Session expired. Please log in again.";
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Network error: {ex.Message}";
            ShowErrorSnackbar = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete item: {ex.Message}";
            ShowErrorSnackbar = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ToggleFavoriteAsync(string itemId)
    {
        if (IsBusy)
            return;

        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return;

        var originalState = item.IsFavorite;
        item.IsFavorite = !originalState;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var updateDto = new PantryItemUpdateDto(IsFavorite: item.IsFavorite);
            var updatedItem = await _pantryService.UpdatePantryItemAsync(itemId, updateDto);

            item.IsFavorite = updatedItem.IsFavorite;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            item.IsFavorite = originalState;
            ErrorMessage = "Session expired. Please log in again.";
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            item.IsFavorite = originalState;
            ErrorMessage = $"Network error: {ex.Message}";
            ShowErrorSnackbar = true;
        }
        catch (Exception ex)
        {
            item.IsFavorite = originalState;
            ErrorMessage = $"Failed to update favorite status: {ex.Message}";
            ShowErrorSnackbar = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ShowAddDialogAsync()
    {
        var itemName = await Shell.Current.DisplayPromptAsync(
            "Add Item",
            "Enter item name:",
            accept: "Add",
            cancel: "Cancel",
            placeholder: "Item name",
            maxLength: 100,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(itemName))
            return;

        DialogItemName = itemName;
        await AddItemAsync();
    }

    [RelayCommand]
    public async Task ShowEditDialogAsync(string itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return;

        var itemName = await Shell.Current.DisplayPromptAsync(
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

        DialogItemName = itemName;
        SelectedItemId = itemId;
        await EditItemAsync();
    }

    [RelayCommand]
    public async Task ShowDeleteDialogAsync(string itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return;

        var confirm = await Shell.Current.DisplayAlert(
            "Delete Item",
            $"Are you sure you want to delete '{item.Name}'?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        SelectedItemId = itemId;
        await ConfirmDeleteAsync();
    }

    [RelayCommand]
    public async Task GenerateRecipeAsync()
    {
        if (IsEmpty)
        {
            await Shell.Current.DisplayAlert("No Items", "Add items to your pantry before generating a recipe.", "OK");
            return;
        }

        // TODO: Navigate to recipe generation page when implemented
        await Shell.Current.DisplayAlert("Coming Soon", "Recipe generation will be implemented in a future version.", "OK");
    }

    [RelayCommand]
    public void DismissError()
    {
        ErrorMessage = string.Empty;
        ShowErrorSnackbar = false;
    }

    [RelayCommand]
    public async Task RetryAsync()
    {
        DismissError();
        await LoadItemsAsync();
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

    partial void OnErrorMessageChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var toast = Toast.Make(value);
            toast.Show();
        }
    }
}

