using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Core;
using PantryPal.Data;
using PantryPal.Mobile.Services;

namespace PantryPal.Mobile.ViewModels;

public partial class RecipeGenerationViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly IDisplayService _displayService;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Controls the visibility of the loading spinner
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowRecipeContent))]
    private bool _isLoading = true;

    /// <summary>
    /// Stores the Markdown recipe content
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowRecipeContent))]
    private string _recipeText = string.Empty;

    /// <summary>
    /// Computed property to control visibility of recipe content and buttons
    /// True when IsLoading is false and RecipeText is not null or empty
    /// </summary>
    public bool ShowRecipeContent => !IsLoading && !string.IsNullOrEmpty(RecipeText);

    private string _generationId = string.Empty;
    private List<RecipeRejectReasonDto> _rejectReasons = new();

    public RecipeGenerationViewModel(IRecipeService recipeService, IDisplayService displayService, INavigationService navigationService)
    {
        _recipeService = recipeService;
        _displayService = displayService;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Triggered when the page appears. Loads reject reasons and generates a recipe
    /// </summary>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (!IsLoading)
        {
            IsLoading = true;
        }

        try
        {
            // Load reject reasons first (needed for potential rejection)
            _rejectReasons = await _recipeService.GetRejectReasonsAsync();

            // Generate the recipe
            var response = await _recipeService.GenerateRecipeAsync();
            
            _generationId = response.GenerationId;
            RecipeText = response.RecipeText;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            // Handle validation errors (empty pantry, no preferences)
            var errorMessage = await TryExtractErrorMessageAsync(ex);
            await _displayService.ShowToast(errorMessage ?? "Please ensure your pantry has items and your preferences are set.", ToastDuration.Long);
            await _navigationService.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _displayService.ShowToast("Please log in again.");
            await _navigationService.PopModalAsync();
            await _navigationService.GoToAsync(AppShell.LoginRoute);
        }
        catch (HttpRequestException ex)
        {
            await _displayService.ShowToast($"Failed to generate recipe. Please check your connection and try again.\n{ex.Message}", ToastDuration.Long);
            await _navigationService.PopModalAsync();
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"An unexpected error occurred. Please try again later.\n{ex.Message}", ToastDuration.Long);
            await _navigationService.PopModalAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Accepts the generated recipe and saves it to the user's collection
    /// </summary>
    [RelayCommand]
    public async Task AcceptAsync()
    {
        if (string.IsNullOrEmpty(_generationId))
        {
            return;
        }

        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var response = await _recipeService.AcceptRecipeAsync(_generationId);

            await _displayService.ShowToast("Your recipe has been saved to your collection!");

            await _navigationService.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await _displayService.ShowToast("This recipe session has expired or is no longer valid.");
            await _navigationService.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            await _displayService.ShowToast("This recipe has already been accepted or rejected.");
            await _navigationService.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _displayService.ShowToast("Please log in again.");
            await _navigationService.PopModalAsync();
            await _navigationService.GoToAsync(AppShell.LoginRoute);
        }
        catch (HttpRequestException ex)
        {
            await _displayService.ShowToast($"Failed to accept recipe. Please check your connection and try again.\n{ex.Message}", ToastDuration.Long);
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"An unexpected error occurred. Please try again later.\n{ex.Message}", ToastDuration.Long);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Shows a dialog to select a rejection reason, then rejects the recipe
    /// </summary>
    [RelayCommand]
    public async Task RejectAsync()
    {
        if (string.IsNullOrEmpty(_generationId))
        {
            return;
        }

        if (IsLoading)
        {
            return;
        }

        if (_rejectReasons.Count == 0)
        {
            await _displayService.ShowToast("No rejection reasons available. Please try again.");
            return;
        }

        // Display action sheet with rejection reasons
        var reasonDescriptions = _rejectReasons.Select(r => r.Description).ToArray();
        var selectedReason = await _displayService.DisplayActionSheet(
            "Why are you rejecting this recipe?",
            "Cancel",
            null,
            reasonDescriptions);

        // User cancelled
        if (selectedReason == "Cancel" || string.IsNullOrEmpty(selectedReason))
        {
            return;
        }

        // Find the selected reason
        var reason = _rejectReasons.FirstOrDefault(r => r.Description == selectedReason);
        if (reason == null)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var payload = new RecipeRejectRequestDto(reason.Id);
            await _recipeService.RejectRecipeAsync(_generationId, payload);

            await _navigationService.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await _displayService.ShowToast("This recipe session has expired or is no longer valid.");
            await _navigationService.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            await _displayService.ShowToast("This recipe has already been accepted or rejected.");
            await _navigationService.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _displayService.ShowToast("Please log in again.");
            await _navigationService.PopModalAsync();
            await _navigationService.GoToAsync(AppShell.LoginRoute);
        }
        catch (HttpRequestException ex)
        {
            await _displayService.ShowToast($"Failed to reject recipe. Please check your connection and try again.\n{ex.Message}", ToastDuration.Long);
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"An unexpected error occurred. Please try again later.\n{ex.Message}", ToastDuration.Long);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Helper method to extract error message from HTTP response
    /// </summary>
    private async Task<string?> TryExtractErrorMessageAsync(HttpRequestException ex)
    {
        try
        {
            if (ex.Data.Contains("ResponseContent") && ex.Data["ResponseContent"] is string content)
            {
                return content;
            }
        }
        catch
        {
            // If we can't extract the message, return null
        }

        return null;
    }
}

