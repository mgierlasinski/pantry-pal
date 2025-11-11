using PantryPal.Api.Services;
using PantryPal.Data;

namespace PantryPal.Api.Endpoints;

public static class Dictionaries
{
    public static void RegisterDictionariesEndpoints(this IEndpointRouteBuilder app)
    {
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
    }
}
