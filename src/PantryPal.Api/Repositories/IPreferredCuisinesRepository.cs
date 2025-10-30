using PantryPal.Api.Db;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository interface for preferred cuisines data access operations
/// </summary>
public interface IPreferredCuisinesRepository
{
    /// <summary>
    /// Retrieves all preferred cuisines from the database
    /// </summary>
    /// <returns>A list of all preferred cuisines</returns>
    Task<IEnumerable<PreferredCuisinesSelect>> GetAllAsync();
}
