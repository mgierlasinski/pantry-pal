using PantryPal.Api.Db;
using Supabase;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository implementation for diet types data access using Supabase
/// </summary>
public class DietTypesRepository : IDietTypesRepository
{
    private readonly Client _supabaseClient;
    private readonly ILogger<DietTypesRepository> _logger;

    public DietTypesRepository(Client supabaseClient, ILogger<DietTypesRepository> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DietTypesSelect>> GetAllAsync()
    {
        try
        {
            var response = await _supabaseClient
                .From<DietTypesSelect>()
                .Order("id", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            _logger.LogInformation(
                "Retrieved {Count} diet types from database",
                response.Models.Count);

            return response.Models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diet types from database");
            throw;
        }
    }
}
