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
}

