using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PantryPal.Mobile.Models;
using PantryPal.Mobile.Services;
using Supabase.Gotrue;
using System.Text.Json;

namespace PantryPal.Mobile;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(appShell);
    }

    protected override async void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        // Handle Supabase authentication callback
        if (uri.Scheme == "pantrypal")
        {
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            await HandleAuthCallback(uri, authService);
        }
    }

    public async void HandleDeepLink(Uri uri)
    {
        // Handle Supabase authentication callback from deep links
        if (uri.Scheme == "pantrypal")
        {
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            await HandleAuthCallback(uri, authService);
        }
    }

    private async Task HandleAuthCallback(Uri uri, IAuthService authService)
    {
        try
        {
            // Extract tokens from URL fragment or query parameters
            var fragment = uri.Fragment?.TrimStart('#');
            if (!string.IsNullOrEmpty(fragment))
            {
                var parameters = ParseQueryString(fragment);

                var accessToken = parameters.GetValueOrDefault("access_token");
                var refreshToken = parameters.GetValueOrDefault("refresh_token");
                var tokenType = parameters.GetValueOrDefault("token_type");
                var expiresIn = parameters.GetValueOrDefault("expires_in");
                var type = parameters.GetValueOrDefault("type");

                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
                {
                    // Create and store the session
                    var session = new Session
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    };

                    var sessionJson = JsonSerializer.Serialize(session);
                    await SecureStorage.SetAsync("supabase_session", sessionJson);

                    // Notify that user is authenticated
                    WeakReferenceMessenger.Default.Send(new AuthStateChangedMessage(true));

                    // Navigate based on action type
                    string navigationTarget;
                    if (type == "recovery")
                    {
                        // Password reset - navigate to profile so user can change password
                        navigationTarget = AppShell.ProfileRoute;
                    }
                    else
                    {
                        // Registration or other actions - navigate to main app
                        navigationTarget = AppShell.DefaultRoute;
                    }

                    await Shell.Current.GoToAsync(navigationTarget);
                }
            }
        }
        catch (Exception ex)
        {
            // Handle error - could show a message to user
            Console.WriteLine($"Error handling auth callback: {ex.Message}");
        }
    }

    private Dictionary<string, string> ParseQueryString(string query)
    {
        var parameters = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(query)) return parameters;

        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 2)
            {
                parameters[keyValue[0]] = Uri.UnescapeDataString(keyValue[1]);
            }
        }
        return parameters;
    }
}