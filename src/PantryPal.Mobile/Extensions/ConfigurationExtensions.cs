using Microsoft.Extensions.Configuration;
using PantryPal.Mobile.Properties;
using System.Reflection;

namespace PantryPal.Mobile.Extensions;

public static class ConfigurationExtensions
{
    public static MauiAppBuilder ConfigureSettings(this MauiAppBuilder builder, Action<ConfigurationOptions> configure)
    {
        var options = new ConfigurationOptions();
        configure.Invoke(options);

        var assembly = Assembly.GetExecutingAssembly();

        foreach (var configuration in options.Configurations)
        {
            builder.AddEmbeddedConfiguration(assembly, configuration);

            var envResource = $"{Path.GetFileNameWithoutExtension(configuration)}.{MauiLaunchSettings.Environment}.json";
            builder.AddEmbeddedConfiguration(assembly, envResource);
        }

        if (options.UseSecrets)
        {
            builder.AddEmbeddedConfiguration(assembly, "secrets.json");
        }
        
        return builder;
    }

    private static void AddEmbeddedConfiguration(this MauiAppBuilder builder, Assembly assembly, string configResource)
    {
        using var resourceStream = assembly.GetManifestResourceStream($"PantryPal.Mobile.{configResource}");

        if (resourceStream != null)
        {
            var config = new ConfigurationBuilder()
                .AddJsonStream(resourceStream)
                .Build();

            builder.Configuration.AddConfiguration(config);
        }
    }
}

public class ConfigurationOptions
{
    public List<string> Configurations { get; } = new();
    public bool UseSecrets { get; private set; }

    public ConfigurationOptions AddConfiguration(string configuration)
    {
        Configurations.Add(configuration);
        return this;
    }

    public ConfigurationOptions AddUserSecrets()
    {
        UseSecrets = true;
        return this;
    }
}
