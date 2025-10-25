using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service interface for pantry items business logic
/// </summary>
public interface IPantryService
{
    /// <summary>
    /// Retrieves a paginated list of pantry items with filtering and sorting
    /// </summary>
    /// <param name="userId">The ID of the user whose pantry items to retrieve</param>
    /// <param name="page">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="favorite">Optional filter for favorite items only</param>
    /// <param name="sortField">The field to sort by (created_at or name)</param>
    /// <returns>A paginated response containing pantry items and metadata</returns>
    Task<PantryItemsPaginatedResponseDto> GetPantryItemsAsync(
        Guid userId,
        int page,
        int pageSize,
        bool? favorite,
        string sortField);
}

