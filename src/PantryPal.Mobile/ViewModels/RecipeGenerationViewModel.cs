using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryPal.Data;
using PantryPal.Mobile.Services;

namespace PantryPal.Mobile.ViewModels;

public partial class RecipeGenerationViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;

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

    public RecipeGenerationViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
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
            await Shell.Current.DisplayAlert(
                "Cannot Generate Recipe", 
                errorMessage ?? "Please ensure your pantry has items and your preferences are set.", 
                "OK");
            await Shell.Current.Navigation.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await Shell.Current.DisplayAlert("Session Expired", "Please log in again.", "OK");
            await Shell.Current.Navigation.PopModalAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            await Shell.Current.DisplayAlert(
                "Network Error", 
                $"Failed to generate recipe. Please check your connection and try again.\n{ex.Message}", 
                "OK");
            await Shell.Current.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Error", 
                $"An unexpected error occurred. Please try again later.\n{ex.Message}", 
                "OK");
            await Shell.Current.Navigation.PopModalAsync();
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

            await Shell.Current.DisplayAlert(
                "Recipe Saved", 
                "Your recipe has been saved to your collection!", 
                "OK");
            
            await Shell.Current.Navigation.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await Shell.Current.DisplayAlert(
                "Recipe Expired", 
                "This recipe session has expired or is no longer valid.", 
                "OK");
            await Shell.Current.Navigation.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            await Shell.Current.DisplayAlert(
                "Recipe Already Processed", 
                "This recipe has already been accepted or rejected.", 
                "OK");
            await Shell.Current.Navigation.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await Shell.Current.DisplayAlert("Session Expired", "Please log in again.", "OK");
            await Shell.Current.Navigation.PopModalAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            await Shell.Current.DisplayAlert(
                "Network Error", 
                $"Failed to accept recipe. Please check your connection and try again.\n{ex.Message}", 
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Error", 
                $"An unexpected error occurred. Please try again later.\n{ex.Message}", 
                "OK");
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
            await Shell.Current.DisplayAlert(
                "Error", 
                "No rejection reasons available. Please try again.", 
                "OK");
            return;
        }

        // Display action sheet with rejection reasons
        var reasonDescriptions = _rejectReasons.Select(r => r.Description).ToArray();
        var selectedReason = await Shell.Current.DisplayActionSheet(
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

            await Shell.Current.Navigation.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await Shell.Current.DisplayAlert(
                "Recipe Expired", 
                "This recipe session has expired or is no longer valid.", 
                "OK");
            await Shell.Current.Navigation.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            await Shell.Current.DisplayAlert(
                "Recipe Already Processed", 
                "This recipe has already been accepted or rejected.", 
                "OK");
            await Shell.Current.Navigation.PopModalAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await Shell.Current.DisplayAlert("Session Expired", "Please log in again.", "OK");
            await Shell.Current.Navigation.PopModalAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            await Shell.Current.DisplayAlert(
                "Network Error", 
                $"Failed to reject recipe. Please check your connection and try again.\n{ex.Message}", 
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                "Error", 
                $"An unexpected error occurred. Please try again later.\n{ex.Message}", 
                "OK");
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

