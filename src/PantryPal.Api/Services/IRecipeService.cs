using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service interface for recipes business logic
/// </summary>
public interface IRecipeService
{
    /// <summary>
    /// Retrieves a paginated list of recipes for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user whose recipes to retrieve</param>
    /// <param name="page">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>A paginated response containing recipes and metadata</returns>
    Task<RecipesPaginatedResponseDto> GetRecipesAsync(
        Guid userId,
        int page,
        int pageSize);

    /// <summary>
    /// Generates an AI-powered recipe based on user's pantry items and preferences
    /// </summary>
    /// <param name="userId">The ID of the user requesting recipe generation</param>
    /// <returns>A response containing the generation ID and generated recipe text</returns>
    /// <exception cref="InvalidOperationException">Thrown when user preferences are not set or pantry is empty</exception>
    Task<RecipeGenerateResponseDto> GenerateRecipeAsync(Guid userId);

    /// <summary>
    /// Accepts a previously generated AI recipe and persists it in the recipes table
    /// </summary>
    /// <param name="generationId">The ID of the recipe generation to accept</param>
    /// <param name="userId">The ID of the user accepting the recipe</param>
    /// <returns>A response containing the recipe ID and timestamp</returns>
    /// <exception cref="ArgumentException">Thrown when generation is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when generation is already accepted or has no recipe text</exception>
    Task<RecipeAcceptResponseDto> AcceptGeneratedRecipeAsync(Guid generationId, Guid userId);

    /// <summary>
    /// Rejects a previously generated AI recipe by setting a reject reason
    /// </summary>
    /// <param name="generationId">The ID of the recipe generation to reject</param>
    /// <param name="rejectReasonId">The ID of the reject reason</param>
    /// <param name="userId">The ID of the user rejecting the recipe</param>
    /// <exception cref="ArgumentException">Thrown when generation is not found or does not belong to the user</exception>
    /// <exception cref="InvalidOperationException">Thrown when generation is already rejected</exception>
    Task RejectGeneratedRecipeAsync(Guid generationId, short rejectReasonId, Guid userId);

    /// <summary>
    /// Deletes a saved recipe from the user's personal collection
    /// </summary>
    /// <param name="recipeId">The unique identifier of the recipe to delete</param>
    /// <param name="userId">The ID of the user requesting the deletion</param>
    /// <exception cref="ArgumentException">Thrown when recipe ID format is invalid</exception>
    /// <exception cref="KeyNotFoundException">Thrown when recipe is not found or does not belong to the user</exception>
    Task DeleteRecipeAsync(string recipeId, Guid userId);
}
