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
}
