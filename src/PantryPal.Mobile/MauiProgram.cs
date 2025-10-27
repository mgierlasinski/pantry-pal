using CommunityToolkit.Maui;
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
        services.AddSingleton<HttpClient>();
        services.AddSingleton<IPantryService, PantryService>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<PantryPageViewModel>();
    }

    private static void RegisterViews(IServiceCollection services)
    {
        services.AddTransient<PantryPage>();
    }
}