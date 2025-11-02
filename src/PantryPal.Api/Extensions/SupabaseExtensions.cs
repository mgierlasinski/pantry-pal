using Supabase;

namespace PantryPal.Api.Extensions;

public static class SupabaseExtensions
{
    public static void AddSupabase(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var url = configuration["Supabase:Url"]!;
            var key = configuration["Supabase:AnonKey"]!;
            var options = new SupabaseOptions { AutoConnectRealtime = true };

            return new Client(url, key, options);
        });
    }
}
