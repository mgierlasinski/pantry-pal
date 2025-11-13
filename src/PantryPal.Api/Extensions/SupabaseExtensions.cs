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
                var token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                if (!string.IsNullOrEmpty(token))
                {
                    // Set the auth token directly - this sets the Authorization header for all requests
                    client.Auth.SetSession(token, string.Empty);
                }
            }

            return client;
        });
    }
}
