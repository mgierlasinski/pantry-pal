using Microsoft.Extensions.Configuration;
using Supabase;

namespace PantryPal.Mobile.Extensions;

public static class SupabaseExtensions
{
    public static void AddSupabase(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var url = configuration["Supabase:Url"]!;
            var key = configuration["Supabase:AnonKey"]!;
            var options = new SupabaseOptions { AutoRefreshToken = true, AutoConnectRealtime = false };

            return new Client(url, key, options);
        });
    }
}
