using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace PantryPal.Mobile.Extensions;

public static class ConfigurationExtensions
{
    public static MauiAppBuilder ConfigureSettings(this MauiAppBuilder builder, string jsonName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var resourceStream = assembly.GetManifestResourceStream($"PantryPal.Mobile.{jsonName}");

        if (resourceStream != null)
        {
            var config = new ConfigurationBuilder()
                .AddJsonStream(resourceStream)
                .Build();

            builder.Configuration.AddConfiguration(config);
        }

        return builder;
    }
}
