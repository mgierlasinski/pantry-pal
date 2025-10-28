using PantryPal.Api.Db;
using Supabase;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository implementation for user preferences data access using Supabase
/// </summary>
public class UserPreferencesRepository : IUserPreferencesRepository
{
    private readonly Client _supabaseClient;
    private readonly ILogger<UserPreferencesRepository> _logger;

    public UserPreferencesRepository(Client supabaseClient, ILogger<UserPreferencesRepository> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<UserPreferencesSelect?> GetUserPreferencesAsync(Guid userId)
    {
        try
        {
            var response = await _supabaseClient
                .From<UserPreferencesSelect>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
                .Single();

            _logger.LogInformation(
                "Retrieved user preferences for user {UserId}",
                userId);

            return response;
        }
        catch (Exception ex) when (ex.Message.Contains("Row not found"))
        {
            _logger.LogWarning("No user preferences found for user {UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving user preferences for user {UserId}",
                userId);
            throw;
        }
    }
}

