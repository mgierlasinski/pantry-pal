using PantryPal.Mobile.Models;

namespace PantryPal.Mobile.Services;

public interface IAuthService
{
    Task<bool> IsAuthenticatedAsync();
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string email, string password);
    Task<AuthResult> LogoutAsync();
    Task<AuthResult> SendPasswordResetEmailAsync(string email);
}
