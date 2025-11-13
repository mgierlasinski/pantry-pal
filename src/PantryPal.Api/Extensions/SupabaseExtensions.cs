using Microsoft.Extensions.Logging;
using Supabase;

namespace PantryPal.Api.Extensions;

public static class SupabaseExtensions
{
    public static void AddSupabase(this IServiceCollection services)
    {
        services.AddScoped(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SupabaseExtensions");
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();

            var url = configuration["Supabase:Url"]!;
            var key = configuration["Supabase:AnonKey"]!;

            logger.LogInformation("Supabase Config - URL: {Url}, Key Length: {KeyLength}", url, key?.Length ?? 0);

            var options = new SupabaseOptions { AutoConnectRealtime = true };

            var client = new Client(url, key, options);

            // Authenticate the client with the user's JWT token if available
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var authHeader = httpContext.Request.Headers.Authorization.ToString();
                logger.LogInformation("Auth Header Present: {HasAuth}, Starts with Bearer: {IsBearer}",
                    !string.IsNullOrEmpty(authHeader), authHeader?.StartsWith("Bearer "));

                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    // Set the authorization header directly on the Postgrest client
                    // This ensures all database requests include the JWT token for RLS
                    client.Postgrest.Options.Headers["Authorization"] = authHeader;
                    logger.LogInformation("Authorization header set on Supabase client");
                }
            }
            else
            {
                logger.LogWarning("User not authenticated or HttpContext null");
            }

            return client;
        });
    }
}
