using PantryPal.Data;

namespace PantryPal.Mobile.Services;

public interface IPantryService
{
    /// <summary>
    /// Gets a paginated list of pantry items
    /// </summary>
    Task<PantryItemsPaginatedResponseDto> GetPantryItemsAsync(int page, int pageSize, string sortField = "name");

    /// <summary>
    /// Creates a new pantry item
    /// </summary>
    Task<PantryItemDto> CreatePantryItemAsync(PantryItemCreateDto item);

    /// <summary>
    /// Updates an existing pantry item
    /// </summary>
    Task<PantryItemDto> UpdatePantryItemAsync(string id, PantryItemUpdateDto item);

    /// <summary>
    /// Deletes a pantry item
    /// </summary>
    Task DeletePantryItemAsync(string id);
}

