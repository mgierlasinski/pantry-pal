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
}

