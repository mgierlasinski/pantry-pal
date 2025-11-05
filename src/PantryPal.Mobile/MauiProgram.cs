using CommunityToolkit.Maui;
using Indiko.Maui.Controls.Markdown;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PantryPal.Mobile.Extensions;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.ViewModels;
using PantryPal.Mobile.Views;
using UraniumUI;

namespace PantryPal.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .UseMarkdownView()
            .ConfigureSettings("appsettings.json")
#if DEBUG
            .ConfigureSettings("appsettings.Development.json")
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddMaterialSymbolsFonts();
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        RegisterServices(builder.Services);
        RegisterViewModels(builder.Services);
        RegisterViews(builder.Services);

        // Register AppShell with dependency injection
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // Register HttpClient with authentication handler
        services.AddSingleton<AuthDelegatingHandler>();
        services.AddSingleton<HttpClient>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var authHandler = sp.GetRequiredService<AuthDelegatingHandler>();

            var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? configuration["Api:AndroidBaseUrl"] ?? "https://10.0.2.2:7154"
                : configuration["Api:DefaultBaseUrl"] ?? "https://localhost:7154";

            var client = new HttpClient(authHandler);
            client.BaseAddress = new Uri(baseUrl);

            return client;
        });

        services.AddSupabase();
        services.AddSingleton<IAuthService, SupabaseAuthService>();
        services.AddSingleton<IPantryService, PantryService>();
        services.AddSingleton<IRecipeService, RecipeService>();
        services.AddSingleton<IUserPreferencesService, UserPreferencesService>();
        services.AddSingleton<IDietTypesService, DietTypesService>();
        services.AddSingleton<IPreferredCuisinesService, PreferredCuisinesService>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginPageViewModel>();
        services.AddTransient<RegisterPageViewModel>();
        services.AddTransient<ForgotPasswordPageViewModel>();
        services.AddTransient<PantryPageViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<RecipeGenerationViewModel>();
        services.AddTransient<RecipeDetailViewModel>();
        services.AddTransient<SavedRecipesViewModel>();
    }

    private static void RegisterViews(IServiceCollection services)
    {
        services.AddTransient<LoginPage>();
        services.AddTransient<RegisterPage>();
        services.AddTransient<ForgotPasswordPage>();
        services.AddTransient<PantryPage>();
        services.AddTransient<ProfilePage>();
        services.AddTransient<RecipeDetailPage>();
        services.AddTransient<RecipeGenerationPage>();
        services.AddTransient<SavedRecipesPage>();
    }
}