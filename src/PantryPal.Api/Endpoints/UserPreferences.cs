using FluentValidation;
using PantryPal.Api.Extensions;
using PantryPal.Api.Services;
using PantryPal.Data;

namespace PantryPal.Api.Endpoints;

public static class UserPreferences
{
    public static void RegisterUserPreferencesEndpoints(this IEndpointRouteBuilder app)
    {
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
    }
}
