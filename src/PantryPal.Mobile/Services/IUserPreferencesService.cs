using PantryPal.Data;

namespace PantryPal.Mobile.Services;

public interface IUserPreferencesService
{
    /// <summary>
    /// Gets the current user's preferences
    /// </summary>
    Task<UserPreferencesDto?> GetUserPreferencesAsync();

    /// <summary>
    /// Creates or updates user preferences
    /// </summary>
    Task<UserPreferencesDto> UpsertUserPreferencesAsync(UserPreferencesCreateDto preferences);
}
