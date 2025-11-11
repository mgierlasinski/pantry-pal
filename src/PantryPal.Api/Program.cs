using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using PantryPal.Api.Exceptions;
using PantryPal.Api.Extensions;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;
using PantryPal.Data;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<ArgumentExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidOperationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(configure =>
{
    configure.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
    };
});

builder.Services.AddSupabase();
builder.Services.AddOpenRouter(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

if (builder.Environment.IsEnvironment("Test"))
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    var secret = builder.Configuration["Supabase:Auth:JwtSecret"]!;
    var bytes = Encoding.UTF8.GetBytes(secret);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(bytes),
        ValidIssuer = builder.Configuration["Supabase:Auth:Issuer"],
        ValidAudience = "authenticated"
    };
});

builder.Services.AddOpenApi();

// Register repositories
builder.Services.AddScoped<IPantryRepository, PantryRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
builder.Services.AddScoped<IRecipesGenerationsRepository, RecipesGenerationsRepository>();
builder.Services.AddScoped<IRecipeRejectReasonsRepository, RecipeRejectReasonsRepository>();
builder.Services.AddScoped<IDietTypesRepository, DietTypesRepository>();
builder.Services.AddScoped<IPreferredCuisinesRepository, PreferredCuisinesRepository>();

// Register services
builder.Services.AddScoped<IPantryService, PantryService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<IDietTypesService, DietTypesService>();
builder.Services.AddScoped<IPreferredCuisinesService, PreferredCuisinesService>();
builder.Services.AddScoped<IRecipeRejectReasonsService, RecipeRejectReasonsService>();
builder.Services.AddScoped<IAIRecipeGeneratorService, AIRecipeGeneratorService>();

var app = builder.Build();
app.UseExceptionHandler();

// Enable authentication/authorization middleware
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "Hello World!");

// GET /pantry-items endpoint
app.MapGet("/pantry-items", async (
    HttpContext httpContext,
    [AsParameters] PantryItemsPaginatedRequestDto request,
    IValidator<PantryItemsPaginatedRequestDto> validator,
    IPantryService pantryService,
    ILogger<Program> logger) =>
{
    // Validate request using FluentValidation
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        logger.LogWarning("Validation failed for GET /pantry-items: {Errors}",
            string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var userId = httpContext.GetUserId();
    var result = await pantryService.GetPantryItemsAsync(
        userId,
        request.Page,
        request.PageSize,
        request.Sort);

    return Results.Ok(result);
}).RequireAuthorization();

// POST /pantry-items endpoint
app.MapPost("/pantry-items", async (
    HttpContext httpContext,
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

    var userId = httpContext.GetUserId();
    var createdItem = await pantryService.CreatePantryItemAsync(userId, dto);

    logger.LogInformation("Successfully created pantry item {ItemId} for user {UserId}", createdItem.Id, userId);
    return Results.Created($"/pantry-items/{createdItem.Id}", createdItem);
}).RequireAuthorization();

// PATCH /pantry-items/{id} endpoint
app.MapPatch("/pantry-items/{id}", async (
    HttpContext httpContext,
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

    var userId = httpContext.GetUserId();
    var updatedItem = await pantryService.UpdatePantryItemAsync(id, userId, dto);

    logger.LogInformation("Successfully updated pantry item {ItemId} for user {UserId}", updatedItem.Id, userId);
    return Results.Ok(updatedItem);
}).RequireAuthorization();

// DELETE /pantry-items/{id} endpoint
app.MapDelete("/pantry-items/{id}", async (
    HttpContext httpContext,
    Guid id,
    IPantryService pantryService,
    ILogger<Program> logger) =>
{
    var userId = httpContext.GetUserId();
    await pantryService.DeletePantryItemAsync(id, userId);

    logger.LogInformation("Successfully deleted pantry item {ItemId} for user {UserId}", id, userId);
    return Results.NoContent();
}).RequireAuthorization();

// GET /recipes endpoint
app.MapGet("/recipes", async (
    HttpContext httpContext,
    [AsParameters] RecipesPaginatedRequestDto request,
    IValidator<RecipesPaginatedRequestDto> validator,
    IRecipeService recipeService,
    ILogger<Program> logger) =>
{
    // Validate request using FluentValidation
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        logger.LogWarning("Validation failed for GET /recipes: {Errors}",
            string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var userId = httpContext.GetUserId();
    var result = await recipeService.GetRecipesAsync(
        userId,
        request.Page,
        request.PageSize);

    return Results.Ok(result);
}).RequireAuthorization();

// POST /recipes/generate endpoint
app.MapPost("/recipes/generate", async (
    HttpContext httpContext,
    IRecipeService recipeService,
    ILogger<Program> logger) =>
{
    var userId = httpContext.GetUserId();
    var result = await recipeService.GenerateRecipeAsync(userId);

    logger.LogInformation("Successfully generated recipe {GenerationId} for user {UserId}",
        result.GenerationId, userId);

    return Results.Ok(result);
}).RequireAuthorization();

// POST /recipes/{generationId}/accept endpoint
app.MapPost("/recipes/{generationId}/accept", async (
    HttpContext httpContext,
    Guid generationId,
    IRecipeService recipeService,
    ILogger<Program> logger) =>
{
    var userId = httpContext.GetUserId();
    var result = await recipeService.AcceptGeneratedRecipeAsync(generationId, userId);

    logger.LogInformation("Successfully accepted recipe generation {GenerationId} and created recipe {RecipeId} for user {UserId}",
        generationId, result.RecipeId, userId);

    return Results.Created($"/recipes/{result.RecipeId}", result);
}).RequireAuthorization();

// POST /recipes/{generationId}/reject endpoint
app.MapPost("/recipes/{generationId}/reject", async (
    HttpContext httpContext,
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

    var userId = httpContext.GetUserId();
    await recipeService.RejectGeneratedRecipeAsync(generationId, request.RejectReasonId, userId);

    logger.LogInformation("Successfully rejected recipe generation {GenerationId} with reason {RejectReasonId} for user {UserId}",
        generationId, request.RejectReasonId, userId);

    return Results.NoContent();
}).RequireAuthorization();

// DELETE /recipes/{id} endpoint
app.MapDelete("/recipes/{id}", async (
    HttpContext httpContext,
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

    var userId = httpContext.GetUserId();
    await recipeService.DeleteRecipeAsync(id, userId);

    logger.LogInformation("Successfully deleted recipe {RecipeId} for user {UserId}", id, userId);
    return Results.NoContent();
}).RequireAuthorization();

// GET /user-preferences endpoint
app.MapGet("/user-preferences", async (
    HttpContext httpContext,
    IUserPreferencesService service,
    ILogger<Program> logger) =>
{
    var userId = httpContext.GetUserId();
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
}).RequireAuthorization();

// POST /user-preferences endpoint
app.MapPost("/user-preferences", async (
    HttpContext httpContext,
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

    var userId = httpContext.GetUserId().ToString();
    var result = await service.UpsertPreferencesAsync(dto, userId);

    logger.LogInformation(
        "Successfully upserted user preferences for user {UserId}: Diet={DietType}, Cuisine={PreferredCuisine}",
        userId, result.DietTypeName, result.PreferredCuisineName);

    return Results.Ok(result);
}).RequireAuthorization().Produces<UserPreferencesDto>(200).Produces(400).Produces(500);

// GET /diet-types endpoint
app.MapGet("/diet-types", async (
    IDietTypesService dietTypesService,
    ILogger<Program> logger) =>
{
    var result = await dietTypesService.GetAllAsync();

    logger.LogInformation("Successfully retrieved {Count} diet types", result.DietTypes.Count());

    return Results.Ok(result);
});

app.MapGet("/preferred-cuisines", async (
    IPreferredCuisinesService preferredCuisinesService,
    ILogger<Program> logger) =>
{
    var result = await preferredCuisinesService.GetAllAsync();

    logger.LogInformation("Successfully retrieved {Count} preferred cuisines", result.PreferredCuisines.Count());

    return Results.Ok(result);
}).Produces<PreferredCuisinesResponseDto>(200);

// GET /recipe-reject-reasons endpoint
app.MapGet("/recipe-reject-reasons", async (
    IRecipeRejectReasonsService recipeRejectReasonsService,
    ILogger<Program> logger) =>
{
    var rejectReasons = await recipeRejectReasonsService.GetAllAsync();

    logger.LogInformation("Successfully retrieved {Count} recipe reject reasons", rejectReasons.Count());

    var response = new RecipeRejectReasonsResponseDto(rejectReasons);
    return Results.Ok(response);
}).Produces<RecipeRejectReasonsResponseDto>(200).Produces(500);

app.Run();
