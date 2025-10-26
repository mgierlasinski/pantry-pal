using PantryPal.Api.Db;
using Supabase;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository implementation for pantry items data access using Supabase
/// </summary>
public class PantryRepository : IPantryRepository
{
    private readonly Client _supabaseClient;
    private readonly ILogger<PantryRepository> _logger;

    public PantryRepository(Client supabaseClient, ILogger<PantryRepository> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<PantryItemsSelect> Items, int Total)> GetPantryItemsAsync(
        Guid userId,
        int page,
        int pageSize,
        string sortField)
    {
        try
        {
            // Build the base query filtered by user_id
            var query = _supabaseClient
                .From<PantryItemsSelect>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString());

            // Apply sorting
            var ascending = sortField == "name"; // Sort name ascending, created_at descending
            query = query.Order(sortField, ascending 
                ? Supabase.Postgrest.Constants.Ordering.Ascending 
                : Supabase.Postgrest.Constants.Ordering.Descending);

            // Get total count before applying pagination
            var countQuery = _supabaseClient
                .From<PantryItemsSelect>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString());

            // Execute count query
            var total = await countQuery.Count(Supabase.Postgrest.Constants.CountType.Exact);

            // Apply pagination (convert to 0-based index)
            var from = (page - 1) * pageSize;
            var to = from + pageSize - 1;
            query = query.Range(from, to);

            // Execute the main query
            var response = await query.Get();

            _logger.LogInformation(
                "Retrieved {Count} pantry items for user {UserId} (page {Page}, total {Total})",
                response.Models.Count, userId, page, total);

            return (response.Models, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error retrieving pantry items for user {UserId} (page {Page}, pageSize {PageSize})", 
                userId, page, pageSize);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PantryItemsSelect> CreatePantryItemAsync(PantryItemsInsert model)
    {
        try
        {
            // Use Supabase.Client.From<PantryItemsInsert>().Insert() to insert record
            var response = await _supabaseClient
                .From<PantryItemsInsert>()
                .Insert(model);

            var createdItem = response.Models.First();

            // Convert back to select model for consistency
            var result = new PantryItemsSelect
            {
                Id = createdItem.Id,
                Name = createdItem.Name,
                IsFavorite = createdItem.IsFavorite,
                CreatedAt = createdItem.CreatedAt,
                UpdatedAt = createdItem.UpdatedAt,
                UserId = createdItem.UserId
            };

            _logger.LogInformation("Successfully created pantry item {ItemId} '{ItemName}' for user {UserId}",
                result.Id, result.Name, result.UserId);

            return result;
        }
        catch (Supabase.Postgrest.Exceptions.PostgrestException ex)
        {
            // Handle database errors (e.g., unique constraint violation)
            if (ex.Message.Contains("duplicate key") || ex.Message.Contains("unique constraint"))
            {
                _logger.LogWarning(ex, "Duplicate pantry item name for user {UserId}: '{ItemName}'",
                    model.UserId, model.Name);
                throw new InvalidOperationException($"An item with the name '{model.Name}' already exists for this user");
            }

            _logger.LogError(ex, "Postgrest exception while creating pantry item for user {UserId}: '{ItemName}'",
                model.UserId, model.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception while creating pantry item for user {UserId}: '{ItemName}'",
                model.UserId, model.Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PantryItemsSelect> UpdatePantryItemAsync(PantryItemsUpdate model)
    {
        try
        {
            var response = await _supabaseClient
                .From<PantryItemsUpdate>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, model.Id)
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, model.UserId)
                .Update(model);

            if (!response.Models.Any())
            {
                throw new ArgumentException("Pantry item not found or not owned by user");
            }

            var updatedItem = response.Models.First();

            // Convert back to select model for consistency
            var result = new PantryItemsSelect
            {
                Id = updatedItem.Id,
                Name = updatedItem.Name,
                IsFavorite = updatedItem.IsFavorite ?? false,
                CreatedAt = updatedItem.CreatedAt,
                UpdatedAt = updatedItem.UpdatedAt,
                UserId = updatedItem.UserId
            };

            _logger.LogInformation("Successfully updated pantry item {ItemId} '{ItemName}' for user {UserId}",
                result.Id, result.Name, result.UserId);

            return result;
        }
        catch (Supabase.Postgrest.Exceptions.PostgrestException ex)
        {
            // Handle database errors (e.g., unique constraint violation)
            if (ex.Message.Contains("duplicate key") || ex.Message.Contains("unique constraint"))
            {
                _logger.LogWarning(ex, "Duplicate pantry item name for user {UserId}: '{ItemName}'",
                    model.UserId, model.Name);
                throw new InvalidOperationException($"An item with the name '{model.Name}' already exists for this user");
            }

            _logger.LogError(ex, "Postgrest exception while updating pantry item {ItemId} for user {UserId}: '{ItemName}'",
                model.Id, model.UserId, model.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception while updating pantry item {ItemId} for user {UserId}: '{ItemName}'",
                model.Id, model.UserId, model.Name);
            throw;
        }
    }
}

