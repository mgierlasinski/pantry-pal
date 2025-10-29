using PantryPal.Api.Repositories;
using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service implementation for user preferences business logic
/// </summary>
public class UserPreferencesService : IUserPreferencesService
{
    private readonly IUserPreferencesRepository _repository;
    private readonly ILogger<UserPreferencesService> _logger;

    public UserPreferencesService(IUserPreferencesRepository repository, ILogger<UserPreferencesService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<UserPreferencesDto?> GetUserPreferencesAsync(Guid userId)
    {
        try
        {
            // Call repository to retrieve user preferences with joined data
            var userPreferencesRecord = await _repository.GetUserPreferencesAsync(userId);

            if (userPreferencesRecord == null)
            {
                _logger.LogWarning("User preferences not found for user {UserId}", userId);
                return null;
            }

            // Map database record to DTO with resolved names
            var preferences = new UserPreferencesDto(
                UserId: userPreferencesRecord.UserId,
                DietTypeId: userPreferencesRecord.DietTypeId,
                DietTypeName: userPreferencesRecord.DietTypes?.Name ?? string.Empty,
                PreferredCuisineId: userPreferencesRecord.PreferredCuisineId,
                PreferredCuisineName: userPreferencesRecord.PreferredCuisines?.Name ?? string.Empty,
                DislikedIngredients: userPreferencesRecord.DislikedIngredients,
                CreatedAt: userPreferencesRecord.CreatedAt,
                UpdatedAt: userPreferencesRecord.UpdatedAt
            );

            _logger.LogInformation(
                "Successfully retrieved user preferences for user {UserId}: Diet={DietType}, Cuisine={PreferredCuisine}",
                userId, preferences.DietTypeName, preferences.PreferredCuisineName);

            return preferences;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error in UserPreferencesService while retrieving preferences for user {UserId}",
                userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<UserPreferencesDto> UpsertPreferencesAsync(UserPreferencesCreateDto dto, string userId)
    {
        // Handle early returns for invalid inputs (guard clauses)
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }

        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "User preferences DTO cannot be null");
        }

        try
        {
            // Validate that the diet type and cuisine exist before upserting
            var dietTypeExists = await _repository.DietTypeExistsAsync(dto.DietTypeId);
            if (!dietTypeExists)
            {
                throw new ArgumentException($"Diet type with ID {dto.DietTypeId} does not exist", nameof(dto.DietTypeId));
            }

            var cuisineExists = await _repository.PreferredCuisineExistsAsync(dto.PreferredCuisineId);
            if (!cuisineExists)
            {
                throw new ArgumentException($"Preferred cuisine with ID {dto.PreferredCuisineId} does not exist", nameof(dto.PreferredCuisineId));
            }

            // Perform the upsert operation
            var upsertedRecord = await _repository.UpsertUserPreferencesAsync(
                userId,
                dto.DietTypeId,
                dto.PreferredCuisineId,
                dto.DislikedIngredients);

            // Map the database record to DTO with resolved names
            // Since we need the joined data, we need to fetch it again with joins
            var userPreferencesRecord = await _repository.GetUserPreferencesAsync(Guid.Parse(userId));

            if (userPreferencesRecord == null)
            {
                _logger.LogError("Failed to retrieve upserted user preferences for user {UserId}", userId);
                throw new InvalidOperationException("Failed to retrieve upserted user preferences");
            }

            var preferences = new UserPreferencesDto(
                UserId: userPreferencesRecord.UserId,
                DietTypeId: userPreferencesRecord.DietTypeId,
                DietTypeName: userPreferencesRecord.DietTypes?.Name ?? string.Empty,
                PreferredCuisineId: userPreferencesRecord.PreferredCuisineId,
                PreferredCuisineName: userPreferencesRecord.PreferredCuisines?.Name ?? string.Empty,
                DislikedIngredients: userPreferencesRecord.DislikedIngredients,
                CreatedAt: userPreferencesRecord.CreatedAt,
                UpdatedAt: userPreferencesRecord.UpdatedAt
            );

            _logger.LogInformation(
                "Successfully upserted user preferences for user {UserId}: Diet={DietType}, Cuisine={PreferredCuisine}",
                userId, preferences.DietTypeName, preferences.PreferredCuisineName);

            return preferences;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex,
                "Error in UserPreferencesService while upserting preferences for user {UserId}",
                userId);
            throw;
        }
    }
}
