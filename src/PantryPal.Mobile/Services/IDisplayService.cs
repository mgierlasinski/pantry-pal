using System.Threading.Tasks;

namespace PantryPal.Mobile.Services;

public interface IDisplayService
{
    Task DisplayAlert(string title, string message, string cancel = "OK");
    Task<bool> DisplayAlert(string title, string message, string accept, string cancel);
    Task<string?> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string? placeholder = null, int maxLength = -1, Keyboard? keyboard = null, string? initialValue = null);
    Task ShowToast(string message);
}
