using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryPal.Mobile.Services;
using System.ComponentModel.DataAnnotations;

namespace PantryPal.Mobile.ViewModels;

public partial class RegisterPageViewModel : ObservableValidator
{
    private readonly IAuthService _authService;
    private readonly IDisplayService _displayService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    private string _email = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    private string _password = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Please confirm your password.")]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private bool _isConfirmPasswordVisible;

    public RegisterPageViewModel(IAuthService authService, IDisplayService displayService, INavigationService navigationService)
    {
        _authService = authService;
        _displayService = displayService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task RegisterAsync()
    {
        if (IsLoading)
            return;

        // Validate input
        ValidateAllProperties();

        // Additional validation for password matching
        if (Password != ConfirmPassword)
        {
            await _displayService.DisplayAlert("Validation Error", "Passwords do not match.");
            return;
        }

        if (HasErrors)
        {
            await _displayService.DisplayAlert("Validation Error",
                string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage)));
            return;
        }

        try
        {
            IsLoading = true;

            var result = await _authService.RegisterAsync(Email.Trim(), Password);

            if (result.IsSuccess)
            {
                await _displayService.ShowToast("Registration successful! Please check your email for verification.");
                // Navigate back to login page
                await _navigationService.GoToAsync(AppShell.LoginRoute);
            }
            else
            {
                await _displayService.DisplayAlert("Registration Failed", result.ErrorMessage ?? "An unexpected error occurred.");
            }
        }
        catch (Exception ex)
        {
            await _displayService.DisplayAlert("Error", $"Registration failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task NavigateToLoginAsync()
    {
        await _navigationService.GoToAsync(AppShell.LoginRoute);
    }

    [RelayCommand]
    public void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    [RelayCommand]
    public void ToggleConfirmPasswordVisibility()
    {
        IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
    }
}
