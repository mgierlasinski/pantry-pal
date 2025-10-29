using PantryPal.Api.Db;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository interface for recipe reject reasons data access operations
/// </summary>
public interface IRecipeRejectReasonsRepository
{
    /// <summary>
    /// Retrieves a reject reason by ID
    /// </summary>
    /// <param name="id">The ID of the reject reason to retrieve</param>
    /// <returns>The reject reason record if found, null otherwise</returns>
    Task<RecipeRejectReasonsSelect?> GetByIdAsync(short id);
}
