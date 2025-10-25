using PantryPal.Api.Repositories;
using PantryPal.Api.Services;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Configure Supabase client
builder.Services.AddSingleton(provider =>
{
    var url = builder.Configuration["Supabase:Url"]!;
    var key = builder.Configuration["Supabase:AnonKey"]!;
    var options = new SupabaseOptions { AutoConnectRealtime = true };

    return new Client(url, key, options);
});

// TODO: Configure authentication when ready
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.Authority = builder.Configuration["Supabase:Url"];
//         options.Audience = "authenticated";
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true
//         };
//     });

// builder.Services.AddAuthorization();

// Register repositories
builder.Services.AddScoped<IPantryRepository, PantryRepository>();

// Register services
builder.Services.AddScoped<IPantryService, PantryService>();

var app = builder.Build();

// TODO: Enable authentication/authorization middleware when ready
// app.UseAuthentication();
// app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

// GET /pantry-items endpoint
app.MapGet("/pantry-items", async (
    int? page,
    int? pageSize,
    string? sort,
    IPantryService pantryService,
    ILogger<Program> logger) =>
{
    // Set default values
    var validatedPage = page ?? 1;
    var validatedPageSize = pageSize ?? 20;
    var validatedSort = sort ?? "created_at";

    // Validate page parameter
    if (validatedPage < 1)
    {
        logger.LogWarning("Invalid page parameter: {Page}", validatedPage);
        return Results.BadRequest(new { error = "Page must be greater than or equal to 1." });
    }

    // Validate pageSize parameter
    if (validatedPageSize < 1 || validatedPageSize > 100)
    {
        logger.LogWarning("Invalid pageSize parameter: {PageSize}", validatedPageSize);
        return Results.BadRequest(new { error = "PageSize must be between 1 and 100." });
    }

    // Validate sort parameter
    var allowedSortFields = new[] { "created_at", "name" };
    if (!allowedSortFields.Contains(validatedSort))
    {
        logger.LogWarning("Invalid sort parameter: {Sort}", validatedSort);
        return Results.BadRequest(new { error = "Sort must be either 'created_at' or 'name'." });
    }

    try
    {
        // TODO: Extract userId from authenticated user when authentication is enabled
        // var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        // {
        //     logger.LogWarning("Unable to extract valid user ID from claims");
        //     return Results.Unauthorized();
        // }

        // TEMPORARY: Use a hardcoded userId for testing until authentication is implemented
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        logger.LogWarning("Using hardcoded userId for testing. Authentication not yet enabled.");

        var result = await pantryService.GetPantryItemsAsync(
            userId,
            validatedPage,
            validatedPageSize,
            validatedSort);

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in GET /pantry-items endpoint");
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}); // TODO: Add .RequireAuthorization() when authentication is enabled

app.Run();
