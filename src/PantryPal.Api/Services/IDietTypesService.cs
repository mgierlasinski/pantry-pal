using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service interface for diet types business logic
/// </summary>
public interface IDietTypesService
{
    /// <summary>
    /// Retrieves all diet types
    /// </summary>
    /// <returns>A response DTO containing all diet types</returns>
    Task<DietTypesResponseDto> GetAllAsync();
}
