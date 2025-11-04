using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using PantryPal.Mobile.Services;
using System.ComponentModel.DataAnnotations;
using PantryPal.Mobile.Views;

namespace PantryPal.Mobile.ViewModels;

public partial class LoginPageViewModel : ObservableValidator
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    private string _email = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Password is required.")]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isPasswordVisible;

    public LoginPageViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (IsLoading)
            return;

        // Validate input
        ValidateAllProperties();
        if (HasErrors)
        {
            await Shell.Current.DisplayAlert("Validation Error",
                string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage)), "OK");
            return;
        }

        try
        {
            IsLoading = true;

            var result = await _authService.LoginAsync(Email.Trim(), Password);

            if (result.IsSuccess)
            {
                await Toast.Make("Login successful!").Show();
                // Navigation to main app will be handled by AppShell based on auth state
                await Shell.Current.GoToAsync("//PantryPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Login Failed", result.ErrorMessage ?? "An unexpected error occurred.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task NavigateToRegisterAsync()
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    [RelayCommand]
    public async Task NavigateToForgotPasswordAsync()
    {
        await Shell.Current.GoToAsync(nameof(ForgotPasswordPage));
    }

    [RelayCommand]
    public void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }
}
