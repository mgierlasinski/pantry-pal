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
            // Query user preferences with joined diet type and cuisine data
            var response = await _supabaseClient
                .From<UserPreferencesSelect>()
                .Select("*, diet_types!inner(name), preferred_cuisines!inner(name)")
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
                .Single();

            if (response == null)
            {
                _logger.LogWarning("No user preferences found for user {UserId}", userId);
                return null;
            }

            _logger.LogInformation(
                "Retrieved user preferences for user {UserId} with joined diet type and cuisine data",
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

    /// <inheritdoc />
    public async Task<bool> DietTypeExistsAsync(int dietTypeId)
    {
        try
        {
            var response = await _supabaseClient
                .From<DietTypesSelect>()
                .Select("id")
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, dietTypeId)
                .Single();

            return response != null;
        }
        catch (Exception ex) when (ex.Message.Contains("Row not found"))
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if diet type {DietTypeId} exists", dietTypeId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> PreferredCuisineExistsAsync(int preferredCuisineId)
    {
        try
        {
            var response = await _supabaseClient
                .From<PreferredCuisinesSelect>()
                .Select("id")
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, preferredCuisineId)
                .Single();

            return response != null;
        }
        catch (Exception ex) when (ex.Message.Contains("Row not found"))
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if preferred cuisine {PreferredCuisineId} exists", preferredCuisineId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<UserPreferencesSelect> UpsertUserPreferencesAsync(string userId, short dietTypeId, short preferredCuisineId, string? dislikedIngredients)
    {
        try
        {
            // Create the insert object
            var insertData = new UserPreferencesInsert
            {
                UserId = userId,
                DietTypeId = dietTypeId,
                PreferredCuisineId = preferredCuisineId,
                DislikedIngredients = dislikedIngredients,
                CreatedAt = null, // Let database handle timestamps
                UpdatedAt = null
            };

            // Perform upsert using INSERT ... ON CONFLICT DO UPDATE
            _logger.LogInformation("Attempting upsert for user {UserId}", userId);
            var response = await _supabaseClient
                .From<UserPreferencesInsert>()
                .Upsert(insertData, new Supabase.Postgrest.QueryOptions
                {
                    OnConflict = "user_id"
                });

            if (response == null || response.Models.Count == 0)
            {
                throw new InvalidOperationException("Upsert operation returned null or empty result");
            }

            var upsertedItem = response.Models.First();

            // Convert to select model for consistency with other repository methods
            var result = new UserPreferencesSelect
            {
                UserId = upsertedItem.UserId,
                DietTypeId = upsertedItem.DietTypeId,
                PreferredCuisineId = upsertedItem.PreferredCuisineId,
                DislikedIngredients = upsertedItem.DislikedIngredients,
                CreatedAt = upsertedItem.CreatedAt,
                UpdatedAt = upsertedItem.UpdatedAt
            };

            _logger.LogInformation(
                "Successfully upserted user preferences for user {UserId}: DietTypeId={DietTypeId}, PreferredCuisineId={PreferredCuisineId}",
                userId, dietTypeId, preferredCuisineId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error upserting user preferences for user {UserId}. Exception Type: {ExceptionType}, Message: {Message}",
                userId, ex.GetType().Name, ex.Message);
            throw;
        }
    }
}

