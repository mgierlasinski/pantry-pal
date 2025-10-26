using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service interface for pantry items business logic
/// </summary>
public interface IPantryService
{
    /// <summary>
    /// Retrieves a paginated list of pantry items with sorting
    /// </summary>
    /// <param name="userId">The ID of the user whose pantry items to retrieve</param>
    /// <param name="page">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="sortField">The field to sort by (created_at or name)</param>
    /// <returns>A paginated response containing pantry items and metadata</returns>
    Task<PantryItemsPaginatedResponseDto> GetPantryItemsAsync(
        Guid userId,
        int page,
        int pageSize,
        string sortField);

    /// <summary>
    /// Creates a new pantry item for the specified user
    /// </summary>
    /// <param name="userId">The ID of the user creating the pantry item</param>
    /// <param name="dto">The create DTO containing the item name</param>
    /// <returns>The created pantry item DTO</returns>
    /// <exception cref="InvalidOperationException">Thrown when an item with the same name already exists for the user</exception>
    Task<PantryItemDto> CreatePantryItemAsync(Guid userId, PantryItemCreateDto dto);

    /// <summary>
    /// Updates an existing pantry item for the specified user
    /// </summary>
    /// <param name="id">The ID of the pantry item to update</param>
    /// <param name="userId">The ID of the user who owns the pantry item</param>
    /// <param name="dto">The update DTO containing the fields to modify</param>
    /// <returns>The updated pantry item DTO</returns>
    /// <exception cref="ArgumentException">Thrown when the pantry item is not found or not owned by the user</exception>
    /// <exception cref="InvalidOperationException">Thrown when an item with the same name already exists for the user</exception>
    Task<PantryItemDto> UpdatePantryItemAsync(Guid id, Guid userId, PantryItemUpdateDto dto);
}

