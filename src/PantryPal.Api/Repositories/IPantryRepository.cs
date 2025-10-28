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

    /// <summary>
    /// Retrieves all pantry items for a specific user (no pagination)
    /// </summary>
    /// <param name="userId">The ID of the user whose pantry items to retrieve</param>
    /// <returns>A list of all pantry items for the user</returns>
    Task<IEnumerable<PantryItemsSelect>> GetAllPantryItemsAsync(Guid userId);

    /// <summary>
    /// Creates a new pantry item in the database
    /// </summary>
    /// <param name="model">The insert model containing the pantry item data</param>
    /// <returns>The created pantry item record</returns>
    /// <exception cref="InvalidOperationException">Thrown when a pantry item with the same name already exists for the user</exception>
    Task<PantryItemsSelect> CreatePantryItemAsync(PantryItemsInsert model);

    /// <summary>
    /// Updates an existing pantry item in the database
    /// </summary>
    /// <param name="model">The update model containing the pantry item data</param>
    /// <returns>The updated pantry item record</returns>
    /// <exception cref="InvalidOperationException">Thrown when a pantry item with the same name already exists for the user</exception>
    Task<PantryItemsSelect> UpdatePantryItemAsync(PantryItemsUpdate model);

    /// <summary>
    /// Deletes a pantry item from the database
    /// </summary>
    /// <param name="id">The ID of the pantry item to delete</param>
    /// <param name="userId">The ID of the user who owns the pantry item</param>
    /// <returns>The number of rows affected (0 if item not found, 1 if deleted)</returns>
    Task<int> DeletePantryItemAsync(Guid id, Guid userId);
}

