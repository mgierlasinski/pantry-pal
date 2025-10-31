using PantryPal.Data;

namespace PantryPal.Mobile.Services;

public interface IPreferredCuisinesService
{
    /// <summary>
    /// Gets all available preferred cuisines
    /// </summary>
    Task<PreferredCuisinesResponseDto> GetPreferredCuisinesAsync();
}
