using CommunityToolkit.Maui.Alerts;

namespace PantryPal.Mobile.Services;

public class DisplayService : IDisplayService
{
    public async Task DisplayAlert(string title, string message, string cancel = "OK")
    {
        await Shell.Current.DisplayAlert(title, message, cancel);
    }

    public async Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
    {
        return await Shell.Current.DisplayAlert(title, message, accept, cancel);
    }

    public async Task<string?> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string? placeholder = null, int maxLength = -1, Keyboard? keyboard = null, string? initialValue = null)
    {
        return await Shell.Current.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard ?? Keyboard.Default, initialValue);
    }

    public async Task<string?> DisplayActionSheet(string title, string? cancel, string? destruction, params string[] buttons)
    {
        return await Shell.Current.DisplayActionSheet(title, cancel, destruction, buttons);
    }

    public async Task ShowToast(string message)
    {
        await Toast.Make(message).Show();
    }
}
