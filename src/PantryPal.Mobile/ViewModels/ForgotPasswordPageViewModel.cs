using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using PantryPal.Mobile.Services;
using System.ComponentModel.DataAnnotations;

namespace PantryPal.Mobile.ViewModels;

public partial class ForgotPasswordPageViewModel : ObservableValidator
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    private string _email = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public ForgotPasswordPageViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    public async Task SendResetEmailAsync()
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

            var result = await _authService.SendPasswordResetEmailAsync(Email.Trim());

            if (result.IsSuccess)
            {
                await Toast.Make("If an account with this email exists, a password reset link has been sent.").Show();
                // Navigate back to login page
                await Shell.Current.GoToAsync(AppShell.LoginRoute);
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", result.ErrorMessage ?? "An unexpected error occurred.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to send reset email: {ex.Message}", "OK");
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
}
