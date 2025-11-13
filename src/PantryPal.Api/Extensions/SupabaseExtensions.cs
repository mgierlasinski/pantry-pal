using Supabase;

namespace PantryPal.Api.Extensions;

public static class SupabaseExtensions
{
    public static void AddSupabase(this IServiceCollection services)
    {
        services.AddScoped(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var url = configuration["Supabase:Url"]!;
            var key = configuration["Supabase:AnonKey"]!;
            var options = new SupabaseOptions { AutoConnectRealtime = true };

            var client = new Client(url, key, options);

            // Authenticate the client with the user's JWT token if available
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var authHeader = httpContext.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    // Set the authorization header directly on the Postgrest client
                    // This ensures all database requests include the JWT token for RLS
                    client.Postgrest.Options.Headers["Authorization"] = authHeader;
                }
            }

            return client;
        });
    }
}
