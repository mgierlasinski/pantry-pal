using CommunityToolkit.Maui.Alerts;
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

    // Collections for pickers
    [ObservableProperty]
    private ObservableCollection<DietTypeDto> _dietTypes = new();

    [ObservableProperty]
    private ObservableCollection<PreferredCuisineDto> _preferredCuisines = new();

    // Selected items
    [ObservableProperty]
    private DietTypeDto? _selectedDietType;

    [ObservableProperty]
    private PreferredCuisineDto? _selectedPreferredCuisine;

    // Form input with validation
    [ObservableProperty]
    [MaxLength(1000, ErrorMessage = "Disliked ingredients cannot exceed 1000 characters.")]
    private string _dislikedIngredients = string.Empty;

    // State properties
    [ObservableProperty]
    private bool _isLoading;

    public ProfileViewModel(
        IUserPreferencesService userPreferencesService,
        IDietTypesService dietTypesService,
        IPreferredCuisinesService preferredCuisinesService)
    {
        _userPreferencesService = userPreferencesService;
        _dietTypesService = dietTypesService;
        _preferredCuisinesService = preferredCuisinesService;
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
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            await Toast.Make($"Network error: {ex.Message}").Show();
        }
        catch (Exception ex)
        {
            await Toast.Make($"Failed to load preferences: {ex.Message}").Show();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SavePreferencesAsync()
    {
        // Validate the model
        ValidateAllProperties();
        if (HasErrors)
        {
            await Shell.Current.DisplayAlert("Validation Error", string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage)), "OK");
            return;
        }

        // Ensure required selections are made
        if (SelectedDietType == null)
        {
            await Shell.Current.DisplayAlert("Validation Error", "Please select a diet type.", "OK");
            return;
        }

        if (SelectedPreferredCuisine == null)
        {
            await Shell.Current.DisplayAlert("Validation Error", "Please select a preferred cuisine.", "OK");
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
            await Toast.Make("Preferences saved successfully!").Show();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException ex)
        {
            await Toast.Make($"Network error: {ex.Message}").Show();
        }
        catch (Exception ex)
        {
            await Toast.Make($"Failed to save preferences: {ex.Message}").Show();
        }
    }
}
