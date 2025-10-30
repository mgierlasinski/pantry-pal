using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service interface for recipe reject reasons business logic operations
/// </summary>
public interface IRecipeRejectReasonsService
{
    /// <summary>
    /// Retrieves all reject reasons as DTOs
    /// </summary>
    /// <returns>A collection of recipe reject reason DTOs</returns>
    Task<IEnumerable<RecipeRejectReasonDto>> GetAllAsync();
}
