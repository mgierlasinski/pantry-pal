using CommunityToolkit.Maui;
using Indiko.Maui.Controls.Markdown;
using Microsoft.Extensions.Logging;
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

        return builder.Build();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<HttpClient>(sp =>
        {
            var handler = new HttpClientHandler();
#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert is { Issuer: "CN=localhost" })
                {
                    return true;
                }
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
#endif
            return new HttpClient(handler);
        });
        services.AddSingleton<IPantryService, PantryService>();
        services.AddSingleton<IRecipeService, RecipeService>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<PantryPageViewModel>();
        services.AddTransient<RecipeGenerationViewModel>();
    }

    private static void RegisterViews(IServiceCollection services)
    {
        services.AddTransient<PantryPage>();
        services.AddTransient<RecipeGenerationPage>();
    }
}