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
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
builder.Services.AddScoped<IRecipesGenerationsRepository, RecipesGenerationsRepository>();
builder.Services.AddScoped<IRecipeRejectReasonsRepository, RecipeRejectReasonsRepository>();
builder.Services.AddScoped<IDietTypesRepository, DietTypesRepository>();

// Register services
builder.Services.AddScoped<IPantryService, PantryService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<IDietTypesService, DietTypesService>();
builder.Services.AddScoped<IAIRecipeGeneratorService, MockAIRecipeGeneratorService>();

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

// POST /recipes/generate endpoint
app.MapPost("/recipes/generate", async (
    IRecipeService recipeService,
    ILogger<Program> logger) =>
{
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

        var result = await recipeService.GenerateRecipeAsync(userId);

        logger.LogInformation("Successfully generated recipe {GenerationId} for user {UserId}",
            result.GenerationId, userId);

        return Results.Ok(result);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("preferences not set"))
    {
        logger.LogWarning(ex, "Recipe generation failed: user preferences not set");
        return Results.BadRequest(new { error = "User preferences not set." });
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Pantry is empty"))
    {
        logger.LogWarning(ex, "Recipe generation failed: pantry is empty");
        return Results.BadRequest(new { error = "Pantry is empty." });
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Failed to generate recipe"))
    {
        logger.LogError(ex, "Recipe generation failed: AI service error");
        return Results.Problem(
            title: "Recipe Generation Failed",
            detail: "Failed to generate recipe. Please try again later.",
            statusCode: 500);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in POST /recipes/generate endpoint");
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}); // TODO: Add .RequireAuthorization() when authentication is enabled

// POST /recipes/{generationId}/accept endpoint
app.MapPost("/recipes/{generationId}/accept", async (
    Guid generationId,
    IRecipeService recipeService,
    ILogger<Program> logger) =>
{
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

        var result = await recipeService.AcceptGeneratedRecipeAsync(generationId, userId);

        logger.LogInformation("Successfully accepted recipe generation {GenerationId} and created recipe {RecipeId} for user {UserId}",
            generationId, result.RecipeId, userId);

        return Results.Created($"/recipes/{result.RecipeId}", result);
    }
    catch (ArgumentException ex) when (ex.Message.Contains("not found"))
    {
        logger.LogWarning(ex, "Generation {GenerationId} not found", generationId);
        return Results.NotFound(new { error = "Generation not found" });
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Already accepted"))
    {
        logger.LogWarning(ex, "Generation {GenerationId} already accepted", generationId);
        return Results.Conflict(new { error = "Already accepted" });
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("No recipe text available"))
    {
        logger.LogWarning(ex, "Generation {GenerationId} has no recipe text", generationId);
        return Results.BadRequest(new { error = "No recipe text available" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in POST /recipes/{GenerationId}/accept endpoint", generationId);
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}); // TODO: Add .RequireAuthorization() when authentication is enabled

// POST /recipes/{generationId}/reject endpoint
app.MapPost("/recipes/{generationId}/reject", async (
    Guid generationId,
    RecipeRejectRequestDto request,
    IValidator<RecipeRejectRequestDto> validator,
    IRecipeService recipeService,
    ILogger<Program> logger) =>
{
    // Validate request body using FluentValidation
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        logger.LogWarning("Validation failed for POST /recipes/{GenerationId}/reject: {Errors}",
            generationId, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
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

        await recipeService.RejectGeneratedRecipeAsync(generationId, request.RejectReasonId, userId);

        logger.LogInformation("Successfully rejected recipe generation {GenerationId} with reason {RejectReasonId} for user {UserId}",
            generationId, request.RejectReasonId, userId);

        return Results.NoContent();
    }
    catch (ArgumentException ex) when (ex.Message.Contains("not found"))
    {
        logger.LogWarning(ex, "Generation {GenerationId} not found", generationId);
        return Results.NotFound(new { error = "Generation not found" });
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Already rejected"))
    {
        logger.LogWarning(ex, "Generation {GenerationId} already rejected", generationId);
        return Results.Conflict(new { error = "Already rejected" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in POST /recipes/{GenerationId}/reject endpoint", generationId);
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}); // TODO: Add .RequireAuthorization() when authentication is enabled

// DELETE /recipes/{id} endpoint
app.MapDelete("/recipes/{id}", async (
    string id,
    IRecipeService recipeService,
    ILogger<Program> logger) =>
{
    // Validate recipe ID format
    if (!Guid.TryParse(id, out _))
    {
        logger.LogWarning("Invalid recipe ID format: {RecipeId}", id);
        return Results.BadRequest(new { error = "Invalid recipe ID format." });
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

        await recipeService.DeleteRecipeAsync(id, userId);

        logger.LogInformation("Successfully deleted recipe {RecipeId} for user {UserId}", id, userId);
        return Results.NoContent();
    }
    catch (ArgumentException ex) when (ex.Message.Contains("Invalid recipe ID format"))
    {
        logger.LogWarning(ex, "Invalid recipe ID format: {RecipeId}", id);
        return Results.BadRequest(new { error = "Invalid recipe ID format." });
    }
    catch (KeyNotFoundException ex) when (ex.Message.Contains("Recipe not found"))
    {
        logger.LogWarning(ex, "Recipe {RecipeId} not found for user {UserId}", id, Guid.Parse(DefaultUserId));
        return Results.NotFound(new { error = "Recipe not found." });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in DELETE /recipes/{Id} endpoint", id);
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}); // TODO: Add .RequireAuthorization() when authentication is enabled

// GET /user-preferences endpoint
app.MapGet("/user-preferences", async (
    IUserPreferencesService service,
    ILogger<Program> logger) =>
{
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

        var preferences = await service.GetUserPreferencesAsync(userId);

        if (preferences == null)
        {
            logger.LogWarning("User preferences not found for user {UserId}", userId);
            return Results.NotFound(new { error = "User preferences not found" });
        }

        logger.LogInformation(
            "Successfully retrieved user preferences for user {UserId}: Diet={DietType}, Cuisine={PreferredCuisine}",
            userId, preferences.DietTypeName, preferences.PreferredCuisineName);

        return Results.Ok(preferences);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in GET /user-preferences endpoint");
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}); // TODO: Add .RequireAuthorization() when authentication is enabled

// POST /user-preferences endpoint
app.MapPost("/user-preferences", async (
    UserPreferencesCreateDto dto,
    IValidator<UserPreferencesCreateDto> validator,
    IUserPreferencesService service,
    ILogger<Program> logger) =>
{
    // Validate request body using FluentValidation
    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        logger.LogWarning("Validation failed for POST /user-preferences: {Errors}",
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
        var userId = DefaultUserId;
        logger.LogWarning("Using hardcoded userId for testing. Authentication not yet enabled.");

        var result = await service.UpsertPreferencesAsync(dto, userId);

        logger.LogInformation(
            "Successfully upserted user preferences for user {UserId}: Diet={DietType}, Cuisine={PreferredCuisine}",
            userId, result.DietTypeName, result.PreferredCuisineName);

        return Results.Ok(result);
    }
    catch (ArgumentException ex) when (ex.Message.Contains("does not exist"))
    {
        logger.LogWarning(ex, "Invalid reference in user preferences");
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in POST /user-preferences endpoint");
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
}).Produces<UserPreferencesDto>(200).Produces(400).Produces(500); // TODO: Add .RequireAuthorization() when authentication is enabled

// GET /diet-types endpoint
app.MapGet("/diet-types", async (
    IDietTypesService dietTypesService,
    ILogger<Program> logger) =>
{
    try
    {
        var result = await dietTypesService.GetAllAsync();

        logger.LogInformation("Successfully retrieved {Count} diet types", result.DietTypes.Count());

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception in GET /diet-types endpoint");
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An error occurred while processing your request.",
            statusCode: 500);
    }
});

app.Run();
