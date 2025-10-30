using PantryPal.Api.Db;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository interface for diet types data access operations
/// </summary>
public interface IDietTypesRepository
{
    /// <summary>
    /// Retrieves all diet types from the database
    /// </summary>
    /// <returns>A list of all diet types</returns>
    Task<IEnumerable<DietTypesSelect>> GetAllAsync();
}
