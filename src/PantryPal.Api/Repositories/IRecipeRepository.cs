using PantryPal.Api.Db;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository interface for recipes data access operations
/// </summary>
public interface IRecipeRepository
{
    /// <summary>
    /// Retrieves a paginated list of recipes for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user whose recipes to retrieve</param>
    /// <param name="page">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>A tuple containing the list of recipes and the total count</returns>
    Task<(IEnumerable<RecipesSelect> Items, int Total)> GetRecipesAsync(
        Guid userId,
        int page,
        int pageSize);

    /// <summary>
    /// Creates a new recipe in the database
    /// </summary>
    /// <param name="model">The insert model containing the recipe data</param>
    /// <returns>The created recipe record</returns>
    Task<RecipesSelect> CreateRecipeAsync(RecipesInsert model);
}
