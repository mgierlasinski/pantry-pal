using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using PantryPal.Mobile.Services;
using System.ComponentModel.DataAnnotations;

namespace PantryPal.Mobile.ViewModels;

public partial class RegisterPageViewModel : ObservableValidator
{
    private readonly IAuthService _authService;

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

    public RegisterPageViewModel(IAuthService authService)
    {
        _authService = authService;
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
            await Shell.Current.DisplayAlert("Validation Error", "Passwords do not match.", "OK");
            return;
        }

        if (HasErrors)
        {
            await Shell.Current.DisplayAlert("Validation Error",
                string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage)), "OK");
            return;
        }

        try
        {
            IsLoading = true;

            var result = await _authService.RegisterAsync(Email.Trim(), Password);

            if (result.IsSuccess)
            {
                await Toast.Make("Registration successful! Please check your email for verification.").Show();
                // Navigate back to login page
                await Shell.Current.GoToAsync(AppShell.LoginRoute);
            }
            else
            {
                await Shell.Current.DisplayAlert("Registration Failed", result.ErrorMessage ?? "An unexpected error occurred.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Registration failed: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync(AppShell.LoginRoute);
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
