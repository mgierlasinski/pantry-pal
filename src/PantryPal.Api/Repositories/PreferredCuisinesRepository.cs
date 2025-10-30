using PantryPal.Api.Db;
using Supabase;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository implementation for preferred cuisines data access using Supabase
/// </summary>
public class PreferredCuisinesRepository : IPreferredCuisinesRepository
{
    private readonly Client _supabaseClient;
    private readonly ILogger<PreferredCuisinesRepository> _logger;

    public PreferredCuisinesRepository(Client supabaseClient, ILogger<PreferredCuisinesRepository> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PreferredCuisinesSelect>> GetAllAsync()
    {
        try
        {
            var response = await _supabaseClient
                .From<PreferredCuisinesSelect>()
                .Order("name", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            _logger.LogInformation(
                "Retrieved {Count} preferred cuisines from database",
                response.Models.Count);

            return response.Models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving preferred cuisines from database");
            throw;
        }
    }
}
