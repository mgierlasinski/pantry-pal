using FluentValidation;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;
using PantryPal.Data;
using Supabase;
using System.Security.Claims;

const string DefaultUserId = "cedc2d66-51dc-4b19-8713-b51bf177df39";

var builder = WebApplication.CreateBuilder(args);

// Configure Supabase client
builder.Services.AddSingleton(provider =>
{
    var url = builder.Configuration["Supabase:Url"]!;
    var key = builder.Configuration["Supabase:AnonKey"]!;
    var options = new SupabaseOptions { AutoConnectRealtime = true };

    return new Client(url, key, options);
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();

// Register services
builder.Services.AddScoped<IPantryService, PantryService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();

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
        var userId = Guid.Parse(DefaultUserId);
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

// POST /pantry-items endpoint
app.MapPost("/pantry-items", async (
    PantryItemCreateDto dto,
    IValidator<PantryItemCreateDto> validator,
    IPantryService pantryService,
    ILogger<Program> logger) =>
{
    // Validate request body using FluentValidation
    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        logger.LogWarning("Validation failed for POST /pantry-items: {Errors}",
            string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        return Results.ValidationProblem(validationResult.ToDictionary());
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
        var userId = Guid.Parse(DefaultUserId);
        logger.LogWarning("Using hardcoded userId for testing. Authentication not yet enabled.");

        var createdItem = await pantryService.CreatePantryItemAsync(userId, dto);

        logger.LogInformation("Successfully created pantry item {ItemId} for user {UserId}", createdItem.Id, userId);
        return Results.Created($"/pantry-items/{createdItem.Id}", createdItem);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
    {
        logger.LogWarning(ex, "Duplicate pantry item name");
        return Results.Conflict("An item with this name already exists");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in POST /pantry-items endpoint");
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}); // TODO: Add .RequireAuthorization() when authentication is enabled

// PATCH /pantry-items/{id} endpoint
app.MapPatch("/pantry-items/{id}", async (
    Guid id,
    PantryItemUpdateDto dto,
    IValidator<PantryItemUpdateDto> validator,
    IPantryService pantryService,
    ILogger<Program> logger) =>
{
    // Validate request body using FluentValidation
    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        logger.LogWarning("Validation failed for PATCH /pantry-items/{Id}: {Errors}",
            id, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        return Results.ValidationProblem(validationResult.ToDictionary());
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
        var userId = Guid.Parse(DefaultUserId);
        logger.LogWarning("Using hardcoded userId for testing. Authentication not yet enabled.");

        var updatedItem = await pantryService.UpdatePantryItemAsync(id, userId, dto);

        logger.LogInformation("Successfully updated pantry item {ItemId} for user {UserId}", updatedItem.Id, userId);
        return Results.Ok(updatedItem);
    }
    catch (ArgumentException ex) when (ex.Message.Contains("not found"))
    {
        logger.LogWarning(ex, "Pantry item {ItemId} not found for user", id);
        return Results.NotFound("Pantry item not found");
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
    {
        logger.LogWarning(ex, "Duplicate pantry item name for user");
        return Results.Conflict("An item with this name already exists");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in PATCH /pantry-items/{Id} endpoint", id);
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}); // TODO: Add .RequireAuthorization() when authentication is enabled

// DELETE /pantry-items/{id} endpoint
app.MapDelete("/pantry-items/{id}", async (
    Guid id,
    ClaimsPrincipal user,
    IPantryService pantryService,
    ILogger<Program> logger) =>
{
    try
    {
        // TODO: Extract userId from authenticated user when authentication is enabled
        // var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        // {
        //     logger.LogWarning("Unable to extract valid user ID from claims");
        //     return Results.Unauthorized();
        // }

        // TEMPORARY: Use a hardcoded userId for testing until authentication is implemented
        var userId = Guid.Parse(DefaultUserId);
        logger.LogWarning("Using hardcoded userId for testing. Authentication not yet enabled.");

        await pantryService.DeletePantryItemAsync(id, userId);

        logger.LogInformation("Successfully deleted pantry item {ItemId} for user {UserId}", id, userId);
        return Results.NoContent();
    }
    catch (ArgumentException ex) when (ex.Message.Contains("not found"))
    {
        logger.LogWarning(ex, "Pantry item {ItemId} not found for user", id);
        return Results.NotFound("Pantry item not found");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in DELETE /pantry-items/{Id} endpoint", id);
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}); // TODO: Add .RequireAuthorization() when authentication is enabled

// GET /recipes endpoint
app.MapGet("/recipes", async (
    int? page,
    int? pageSize,
    string? sort,
    IRecipeService recipeService,
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

    // Validate sort parameter (only "created_at" is supported)
    if (validatedSort != "created_at")
    {
        logger.LogWarning("Invalid sort parameter: {Sort}", validatedSort);
        return Results.BadRequest(new { error = "Sort must be 'created_at'." });
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
        var userId = Guid.Parse(DefaultUserId);
        logger.LogWarning("Using hardcoded userId for testing. Authentication not yet enabled.");

        var result = await recipeService.GetRecipesAsync(
            userId,
            validatedPage,
            validatedPageSize);

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in GET /recipes endpoint");
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}); // TODO: Add .RequireAuthorization() when authentication is enabled

app.Run();
