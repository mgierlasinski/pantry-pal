using CommunityToolkit.Mvvm.Messaging;
using PantryPal.Mobile.Models;

namespace PantryPal.Mobile.Services;

public class SupabaseAuthService : IAuthService
{
    private bool _isAuthenticated;

    public Task<bool> IsAuthenticatedAsync()
    {
        // TODO: Implement actual authentication check with Supabase
        return Task.FromResult(_isAuthenticated);
    }

    public Task<AuthResult> LoginAsync(string email, string password)
    {
        // TODO: Implement actual login with Supabase client
        // For now, just simulate a successful login for demo purposes
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            _isAuthenticated = true;
            WeakReferenceMessenger.Default.Send(new AuthStateChangedMessage(true));
            return Task.FromResult(AuthResult.Success());
        }

        return Task.FromResult(AuthResult.Failure("Invalid login credentials"));
    }

    public Task<AuthResult> RegisterAsync(string email, string password)
    {
        // TODO: Implement actual registration with Supabase client
        // For now, just simulate a successful registration
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(AuthResult.Success());
        }

        return Task.FromResult(AuthResult.Failure("Registration failed"));
    }

    public Task<AuthResult> LogoutAsync()
    {
        // TODO: Implement actual logout with Supabase client
        _isAuthenticated = false;
        WeakReferenceMessenger.Default.Send(new AuthStateChangedMessage(false));
        return Task.FromResult(AuthResult.Success());
    }

    public Task<AuthResult> SendPasswordResetEmailAsync(string email)
    {
        // TODO: Implement actual password reset with Supabase client
        // For now, just simulate sending email
        if (!string.IsNullOrWhiteSpace(email))
        {
            return Task.FromResult(AuthResult.Success());
        }

        return Task.FromResult(AuthResult.Failure("Invalid email address"));
    }
}
