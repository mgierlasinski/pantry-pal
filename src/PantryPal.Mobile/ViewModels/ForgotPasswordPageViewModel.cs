using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Core;
using PantryPal.Mobile.Services;
using System.ComponentModel.DataAnnotations;

namespace PantryPal.Mobile.ViewModels;

public partial class ForgotPasswordPageViewModel : ObservableValidator
{
    private readonly IAuthService _authService;
    private readonly IDisplayService _displayService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    private string _email = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public ForgotPasswordPageViewModel(IAuthService authService, IDisplayService displayService, INavigationService navigationService)
    {
        _authService = authService;
        _displayService = displayService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task SendResetEmailAsync()
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

            var result = await _authService.SendPasswordResetEmailAsync(Email.Trim());

            if (result.IsSuccess)
            {
                await _displayService.ShowToast("If an account with this email exists, a password reset link has been sent.");
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
            await _displayService.ShowToast($"Failed to send reset email: {ex.Message}", ToastDuration.Long);
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
