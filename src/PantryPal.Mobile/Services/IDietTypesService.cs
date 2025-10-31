using PantryPal.Data;

namespace PantryPal.Mobile.Services;

public interface IDietTypesService
{
    /// <summary>
    /// Gets all available diet types
    /// </summary>
    Task<DietTypesResponseDto> GetDietTypesAsync();
}
