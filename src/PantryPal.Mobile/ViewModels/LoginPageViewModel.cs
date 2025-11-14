using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Core;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.Views;
using System.ComponentModel.DataAnnotations;

namespace PantryPal.Mobile.ViewModels;

public partial class LoginPageViewModel : ObservableValidator
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
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public LoginPageViewModel(IAuthService authService, IDisplayService displayService, INavigationService navigationService)
    {
        _authService = authService;
        _displayService = displayService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (IsLoading)
            return;

        ValidateAllProperties();

        if (HasErrors)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var result = await _authService.LoginAsync(Email.Trim(), Password);

            if (result.IsSuccess)
            {
                await _displayService.ShowToast("Login successful!");
                // Navigation to main app will be handled by AppShell based on auth state
                await _navigationService.GoToAsync(AppShell.DefaultRoute);
            }
            else
            {
                await _displayService.ShowToast(result.ErrorMessage ?? "An unexpected error occurred.", ToastDuration.Long);
            }
        }
        catch (Exception ex)
        {
            await _displayService.ShowToast($"Login failed: {ex.Message}", ToastDuration.Long);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task NavigateToRegisterAsync()
    {
        await _navigationService.GoToAsync(nameof(RegisterPage));
    }

    [RelayCommand]
    public async Task NavigateToForgotPasswordAsync()
    {
        await _navigationService.GoToAsync(nameof(ForgotPasswordPage));
    }
}
