using PantryPal.Api.Db;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository interface for user preferences data access operations
/// </summary>
public interface IUserPreferencesRepository
{
    /// <summary>
    /// Retrieves user preferences for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user whose preferences to retrieve</param>
    /// <returns>The user preferences record, or null if not found</returns>
    Task<UserPreferencesSelect?> GetUserPreferencesAsync(Guid userId);
}

