using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service interface for preferred cuisines business logic
/// </summary>
public interface IPreferredCuisinesService
{
    /// <summary>
    /// Retrieves all preferred cuisines
    /// </summary>
    /// <returns>A response DTO containing all preferred cuisines</returns>
    Task<PreferredCuisinesResponseDto> GetAllAsync();
}
