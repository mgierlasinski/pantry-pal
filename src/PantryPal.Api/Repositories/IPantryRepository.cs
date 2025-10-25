using PantryPal.Api.Db;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository interface for pantry items data access operations
/// </summary>
public interface IPantryRepository
{
    /// <summary>
    /// Retrieves a paginated list of pantry items for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user whose pantry items to retrieve</param>
    /// <param name="page">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="sortField">The field to sort by (created_at or name)</param>
    /// <returns>A tuple containing the list of items and the total count</returns>
    Task<(IEnumerable<PantryItemsSelect> Items, int Total)> GetPantryItemsAsync(
        Guid userId, 
        int page, 
        int pageSize, 
        string sortField);
}

