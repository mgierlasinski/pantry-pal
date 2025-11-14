using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryPal.Data;
using PantryPal.Mobile.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace PantryPal.Mobile.ViewModels;

public partial class ProfileViewModel : ObservableValidator
{
    private readonly IUserPreferencesService _userPreferencesService;
    private readonly IDietTypesService _dietTypesService;
    private readonly IPreferredCuisinesService _preferredCuisinesService;
    private readonly IAuthService _authService;
    private readonly IDisplayService _displayService;
    private readonly INavigationService _navigationService;

    // Collections for pickers
    [ObservableProperty]
    private ObservableCollection<DietTypeDto> _dietTypes = new();

    [ObservableProperty]
    private ObservableCollection<PreferredCuisineDto> _preferredCuisines = new();

    // Selected items
    [ObservableProperty]
    [Required(ErrorMessage = "Please select a diet type.")]
    private DietTypeDto? _selectedDietType;

    [ObservableProperty]
    [Required(ErrorMessage = "Please select a preferred cuisine.")]
    private PreferredCuisineDto? _selectedPreferredCuisine;

    // Form input with validation
    [ObservableProperty]
    [MaxLength(1000, ErrorMessage = "Disliked ingredients cannot exceed 1000 characters.")]
    private string _dislikedIngredients = string.Empty;

    // Password change fields
    [ObservableProperty]
    [Required(ErrorMessage = "New password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    private string? _newPassword;

    [ObservableProperty]
    [Required(ErrorMessage = "Please confirm your new password.")]
    private string? _confirmNewPassword;

    // State properties
    [ObservableProperty]
    private bool _isLoading;

    public ProfileViewModel(
        IUserPreferencesService userPreferencesService,
        IDietTypesService dietTypesService,
        IPreferredCuisinesService preferredCuisinesService,
        IAuthService authService,
        IDisplayService displayService,
        INavigationService navigationService)
    {
        _userPreferencesService = userPreferencesService;
        _dietTypesService = dietTypesService;
        _preferredCuisinesService = preferredCuisinesService;
        _authService = authService;
        _displayService = displayService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task LoadPreferencesAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;

            // Load diet types, cuisines, and user preferences in parallel
            var dietTypesTask = _dietTypesService.GetDietTypesAsync();
            var cuisinesTask = _preferredCuisinesService.GetPreferredCuisinesAsync();
            var preferencesTask = _userPreferencesService.GetUserPreferencesAsync();

            await Task.WhenAll(dietTypesTask, cuisinesTask, preferencesTask);

            // Update collections
            var dietTypesResponse = await dietTypesTask;
            DietTypes.Clear();
            foreach (var dietType in dietTypesResponse.DietTypes)
            {
                DietTypes.Add(dietType);
            }

            var cuisinesResponse = await cuisinesTask;
            PreferredCuisines.Clear();
            foreach (var cuisine in cuisinesResponse.PreferredCuisines)
            {
                PreferredCuisines.Add(cuisine);
            }

            // Handle user preferences (may be null for new users)
            var preferences = await preferencesTask;
            if (preferences != null)
            {
                SelectedDietType = DietTypes.FirstOrDefault(dt => dt.Id == preferences.DietTypeId);
                SelectedPreferredCuisine = PreferredCuisines.FirstOrDefault(pc => pc.Id == preferences.PreferredCuisineId);
                DislikedIngredients = preferences.DislikedIngredients ?? string.Empty;
            }
            else
            {
                // New user - leave selections empty
                SelectedDietType = null;
                SelectedPreferredCuisine = null;
                DislikedIngredients = string.Empty;
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _navigationService.GoToAsync(AppShell.LoginRoute);
        }
        catch (HttpRequestException ex)
        {
            await _displayService.ShowToast($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"Failed to load preferences: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool HasValidationErrors(params string[] propertyNames)
    {
        return propertyNames.Any(propertyName => GetErrors(propertyName).Any());
    }

    [RelayCommand]
    public async Task SavePreferencesAsync()
    {
        // Validate only preferences-related properties
        ValidateProperty(SelectedDietType, nameof(SelectedDietType));
        ValidateProperty(SelectedPreferredCuisine, nameof(SelectedPreferredCuisine));
        ValidateProperty(DislikedIngredients, nameof(DislikedIngredients));

        // Check if preferences properties have errors
        if (HasValidationErrors(nameof(SelectedDietType), nameof(SelectedPreferredCuisine), nameof(DislikedIngredients)))
        {
            return;
        }

        try
        {
            var preferencesDto = new UserPreferencesCreateDto(
                DietTypeId: SelectedDietType.Id,
                PreferredCuisineId: SelectedPreferredCuisine.Id,
                DislikedIngredients: string.IsNullOrWhiteSpace(DislikedIngredients) ? null : DislikedIngredients.Trim()
            );

            var result = await _userPreferencesService.UpsertUserPreferencesAsync(preferencesDto);
            await _displayService.ShowToast("Preferences saved successfully!");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _navigationService.GoToAsync(AppShell.LoginRoute);
        }
        catch (HttpRequestException ex)
        {
            await _displayService.ShowToast($"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"Failed to save preferences: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        try
        {
            var result = await _authService.LogoutAsync();
            if (result.IsSuccess)
            {
                await _displayService.ShowToast("Logged out successfully");
                // Navigation will be handled by the auth state change message in AppShell
            }
            else
            {
                await _displayService.DisplayAlert("Logout Error", result.ErrorMessage ?? "An error occurred during logout.");
            }
        }
        catch (Exception ex)
        {
            await _displayService.DisplayAlert("Error", $"Logout failed: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task ChangePasswordAsync()
    {
        // Validate only password-related properties
        ValidateProperty(NewPassword, nameof(NewPassword));
        ValidateProperty(ConfirmNewPassword, nameof(ConfirmNewPassword));

        // Additional validation for password matching
        if (NewPassword != ConfirmNewPassword)
        {
            await _displayService.DisplayAlert("Validation Error", "Passwords do not match.");
            return;
        }

        // Check if password properties have errors
        if (HasValidationErrors(nameof(NewPassword), nameof(ConfirmNewPassword)))
        {
            return;
        }

        try
        {
            IsLoading = true;

            var result = await _authService.ChangePasswordAsync(NewPassword!);

            if (result.IsSuccess)
            {
                await _displayService.ShowToast("Password changed successfully!");
                // Clear the password fields
                NewPassword = null;
                ConfirmNewPassword = null;
            }
            else
            {
                await _displayService.DisplayAlert("Error", result.ErrorMessage ?? "Failed to change password.");
            }
        }
        catch (Exception ex)
        {
            await _displayService.DisplayAlert("Error", $"Failed to change password: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
