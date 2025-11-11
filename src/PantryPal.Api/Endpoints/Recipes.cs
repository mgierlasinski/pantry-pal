using FluentValidation;
using PantryPal.Api.Extensions;
using PantryPal.Api.Services;
using PantryPal.Data;

namespace PantryPal.Api.Endpoints;

public static class Recipes
{
    public static void RegisterRecipesEndpoints(this IEndpointRouteBuilder app)
    {
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
    }
}
