using PantryPal.Mobile.Views;

namespace PantryPal.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute(nameof(PantryPage), typeof(PantryPage));
        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
        Routing.RegisterRoute(nameof(RecipeGenerationPage), typeof(RecipeGenerationPage));
    }
}