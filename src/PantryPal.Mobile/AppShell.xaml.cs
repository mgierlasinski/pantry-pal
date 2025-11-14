using CommunityToolkit.Mvvm.Messaging;
using PantryPal.Mobile.Models;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.Views;

namespace PantryPal.Mobile;

public partial class AppShell : Shell, IRecipient<AuthStateChangedMessage>
{
    public const string DefaultRoute = $"//{nameof(PantryPage)}";
    public const string LoginRoute = $"//{nameof(LoginPage)}";
    public const string ProfileRoute = $"//{nameof(ProfilePage)}";

    private readonly IAuthService _authService;

    public AppShell(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;

        // Register routes for navigation
        Routing.RegisterRoute(nameof(PantryPage), typeof(PantryPage));
        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
        Routing.RegisterRoute(nameof(RecipeGenerationPage), typeof(RecipeGenerationPage));

        // Register auth routes for navigation
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));

        // Register for auth state change messages
        WeakReferenceMessenger.Default.Register<AuthStateChangedMessage>(this);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Check initial auth state and set default route
        var isAuthenticated = await _authService.IsAuthenticatedAsync();
        UpdateNavigation(isAuthenticated);
        SetDefaultRoute(isAuthenticated);
    }

    private void UpdateNavigation(bool isAuthenticated)
    {
        // Show/hide main app routes based on authentication state
        var mainTabBar = Items.FirstOrDefault(item => item is TabBar) as TabBar;
        if (mainTabBar != null)
        {
            mainTabBar.IsVisible = isAuthenticated;
        }
    }

    private void SetDefaultRoute(bool isAuthenticated)
    {
        if (isAuthenticated)
        {
            // Navigate to main app
            Shell.Current.GoToAsync(DefaultRoute);
        }
        else
        {
            // Navigate to login page
            Shell.Current.GoToAsync(LoginRoute);
        }
    }

    public void Receive(AuthStateChangedMessage message)
    {
        UpdateNavigation(message.IsAuthenticated);
        SetDefaultRoute(message.IsAuthenticated);
    }
}