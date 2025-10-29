using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service interface for user preferences business logic
/// </summary>
public interface IUserPreferencesService
{
    /// <summary>
    /// Retrieves user preferences for a specific user, including resolved names for diet type and cuisine
    /// </summary>
    /// <param name="userId">The ID of the user whose preferences to retrieve</param>
    /// <returns>The user preferences DTO with resolved names, or null if not found</returns>
    Task<UserPreferencesDto?> GetUserPreferencesAsync(Guid userId);

    /// <summary>
    /// Creates or updates user preferences for a specific user
    /// </summary>
    /// <param name="dto">The create/update DTO containing preference data</param>
    /// <param name="userId">The ID of the user whose preferences to upsert</param>
    /// <returns>The updated user preferences DTO with resolved names</returns>
    Task<UserPreferencesDto> UpsertPreferencesAsync(UserPreferencesCreateDto dto, string userId);
}
