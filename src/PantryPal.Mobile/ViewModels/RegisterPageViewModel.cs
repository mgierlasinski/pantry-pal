using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Core;
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

        ValidateAllProperties();

        // Additional validation for password matching
        if (Password != ConfirmPassword)
        {
            await _displayService.ShowToast("Passwords do not match.");
            return;
        }

        if (HasErrors)
        {
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
                await _displayService.ShowToast(result.ErrorMessage ?? "An unexpected error occurred.", ToastDuration.Long);
            }
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"Registration failed: {ex.Message}", ToastDuration.Long);
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
}
