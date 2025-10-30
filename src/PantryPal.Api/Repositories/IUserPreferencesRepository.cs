using PantryPal.Api.Db;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository interface for user preferences data access operations
/// </summary>
public interface IUserPreferencesRepository
{
    /// <summary>
    /// Retrieves user preferences for a specific user, including joined diet type and cuisine data
    /// </summary>
    /// <param name="userId">The ID of the user whose preferences to retrieve</param>
    /// <returns>The user preferences database record with joined data, or null if not found</returns>
    Task<UserPreferencesSelect?> GetUserPreferencesAsync(Guid userId);

    /// <summary>
    /// Checks if a diet type with the given ID exists
    /// </summary>
    /// <param name="dietTypeId">The diet type ID to check</param>
    /// <returns>True if the diet type exists, false otherwise</returns>
    Task<bool> DietTypeExistsAsync(int dietTypeId);

    /// <summary>
    /// Checks if a preferred cuisine with the given ID exists
    /// </summary>
    /// <param name="preferredCuisineId">The preferred cuisine ID to check</param>
    /// <returns>True if the preferred cuisine exists, false otherwise</returns>
    Task<bool> PreferredCuisineExistsAsync(int preferredCuisineId);

    /// <summary>
    /// Creates or updates user preferences using upsert functionality
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="dietTypeId">The diet type ID</param>
    /// <param name="preferredCuisineId">The preferred cuisine ID</param>
    /// <param name="dislikedIngredients">Optional disliked ingredients</param>
    /// <returns>The updated user preferences record</returns>
    Task<UserPreferencesSelect> UpsertUserPreferencesAsync(string userId, short dietTypeId, short preferredCuisineId, string? dislikedIngredients);
}

