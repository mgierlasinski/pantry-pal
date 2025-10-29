using PantryPal.Api.Db;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository interface for recipe generations data access operations
/// </summary>
public interface IRecipesGenerationsRepository
{
    /// <summary>
    /// Creates a new recipe generation record in the database
    /// </summary>
    /// <param name="model">The insert model containing the generation data</param>
    /// <returns>The created generation record</returns>
    Task<RecipesGenerationsSelect> CreateGenerationAsync(RecipesGenerationsInsert model);

    /// <summary>
    /// Updates an existing recipe generation record
    /// </summary>
    /// <param name="model">The update model containing the generation data</param>
    /// <returns>The updated generation record</returns>
    Task<RecipesGenerationsSelect> UpdateGenerationAsync(RecipesGenerationsUpdate model);

    /// <summary>
    /// Retrieves a recipe generation record by ID for a specific user
    /// </summary>
    /// <param name="generationId">The ID of the generation to retrieve</param>
    /// <param name="userId">The ID of the user who owns the generation</param>
    /// <returns>The generation record if found, null otherwise</returns>
    Task<RecipesGenerationsSelect?> GetByIdAsync(Guid generationId, Guid userId);

    /// <summary>
    /// Marks a generation as accepted by linking it to a created recipe
    /// </summary>
    /// <param name="generationId">The ID of the generation to mark as accepted</param>
    /// <param name="recipeId">The ID of the created recipe</param>
    /// <returns>The updated generation record</returns>
    Task<RecipesGenerationsSelect> MarkAsAcceptedAsync(Guid generationId, Guid recipeId);

    /// <summary>
    /// Updates the reject reason for a recipe generation
    /// </summary>
    /// <param name="generationId">The ID of the generation to update</param>
    /// <param name="rejectReasonId">The reject reason ID to set</param>
    /// <returns>The updated generation record</returns>
    Task<RecipesGenerationsSelect> UpdateRejectReasonAsync(Guid generationId, short rejectReasonId);
}

