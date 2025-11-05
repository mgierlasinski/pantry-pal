using CommunityToolkit.Mvvm.Messaging;
using PantryPal.Mobile.Models;
using Supabase;
using Supabase.Gotrue;
using System.Text.Json;

namespace PantryPal.Mobile.Services;

public class SupabaseAuthService : IAuthService
{
    private readonly Supabase.Client _supabaseClient;
    private const string SessionKey = "supabase_session";

    public SupabaseAuthService(Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            // Try to restore session from secure storage
            var sessionJson = await SecureStorage.GetAsync(SessionKey);
            if (!string.IsNullOrEmpty(sessionJson))
            {
                var session = JsonSerializer.Deserialize<Session>(sessionJson);
                if (session != null && !string.IsNullOrEmpty(session.AccessToken))
                {
                    // Try to set the session on the client and see if it succeeds
                    var restoredSession = await _supabaseClient.Auth.SetSession(session.AccessToken, session.RefreshToken ?? string.Empty);
                    if (restoredSession != null && !string.IsNullOrEmpty(restoredSession.AccessToken))
                    {
                        return true;
                    }
                }
            }

            // If we reach here, no valid session was found
            await ClearSessionAsync();
            return false;
        }
        catch (Exception)
        {
            // If there's any error during session restoration, consider user not authenticated
            await ClearSessionAsync();
            return false;
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _supabaseClient.Auth.SignInWithPassword(email, password);

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                // Save session to secure storage
                await SaveSessionAsync(response);
                WeakReferenceMessenger.Default.Send(new AuthStateChangedMessage(true));
                return AuthResult.Success();
            }

            return AuthResult.Failure("Invalid email or password");
        }
        catch (Exception ex)
        {
            // Map common Supabase errors to user-friendly messages
            var errorMessage = MapAuthError(ex);
            return AuthResult.Failure(errorMessage);
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string password)
    {
        try
        {
            var response = await _supabaseClient.Auth.SignUp(email, password);

            if (response != null)
            {
                // For email confirmation flow, we don't immediately authenticate the user
                // They need to confirm their email first
                return AuthResult.Success();
            }

            return AuthResult.Failure("Registration failed");
        }
        catch (Exception ex)
        {
            var errorMessage = MapAuthError(ex);
            return AuthResult.Failure(errorMessage);
        }
    }

    public async Task<AuthResult> LogoutAsync()
    {
        try
        {
            await _supabaseClient.Auth.SignOut();
            await ClearSessionAsync();
            WeakReferenceMessenger.Default.Send(new AuthStateChangedMessage(false));
            return AuthResult.Success();
        }
        catch (Exception)
        {
            // Even if logout fails on server, clear local session
            await ClearSessionAsync();
            WeakReferenceMessenger.Default.Send(new AuthStateChangedMessage(false));
            return AuthResult.Success();
        }
    }

    public async Task<AuthResult> SendPasswordResetEmailAsync(string email)
    {
        try
        {
            await _supabaseClient.Auth.ResetPasswordForEmail(email);
            return AuthResult.Success();
        }
        catch (Exception ex)
        {
            var errorMessage = MapAuthError(ex);
            return AuthResult.Failure(errorMessage);
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            // Try to get the current session
            var session = _supabaseClient.Auth.CurrentSession;
            if (session != null && !string.IsNullOrEmpty(session.AccessToken))
            {
                return session.AccessToken;
            }

            // If no current session, try to restore from secure storage
            var sessionJson = await SecureStorage.GetAsync(SessionKey);
            if (!string.IsNullOrEmpty(sessionJson))
            {
                var restoredSession = JsonSerializer.Deserialize<Session>(sessionJson);
                if (restoredSession != null && !string.IsNullOrEmpty(restoredSession.AccessToken))
                {
                    // Try to set the session on the client
                    var refreshedSession = await _supabaseClient.Auth.SetSession(restoredSession.AccessToken, restoredSession.RefreshToken ?? string.Empty);
                    if (refreshedSession != null && !string.IsNullOrEmpty(refreshedSession.AccessToken))
                    {
                        return refreshedSession.AccessToken;
                    }
                }
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task SaveSessionAsync(Session session)
    {
        try
        {
            var sessionJson = JsonSerializer.Serialize(session);
            await SecureStorage.SetAsync(SessionKey, sessionJson);
        }
        catch (Exception)
        {
            // If we can't save the session, continue without it
            // The user will need to log in again next time
        }
    }

    private Task ClearSessionAsync()
    {
        try
        {
            SecureStorage.Remove(SessionKey);
        }
        catch (Exception)
        {
            // Continue even if clearing fails
        }

        return Task.CompletedTask;
    }

    private string MapAuthError(Exception ex)
    {
        // Map common Supabase authentication errors to user-friendly messages
        var message = ex.Message.ToLowerInvariant();

        if (message.Contains("invalid login credentials") ||
            message.Contains("email not confirmed") ||
            message.Contains("invalid email or password"))
        {
            return "Invalid email or password";
        }

        if (message.Contains("user already registered"))
        {
            return "An account with this email already exists";
        }

        if (message.Contains("password should be at least"))
        {
            return "Password must be at least 6 characters long";
        }

        if (message.Contains("unable to validate email address"))
        {
            return "Please enter a valid email address";
        }

        // For any other errors, return a generic message
        return "An unexpected error occurred. Please try again.";
    }
}
