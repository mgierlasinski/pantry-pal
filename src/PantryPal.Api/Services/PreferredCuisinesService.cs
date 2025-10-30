using PantryPal.Api.Repositories;
using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service implementation for preferred cuisines business logic
/// </summary>
public class PreferredCuisinesService : IPreferredCuisinesService
{
    private readonly IPreferredCuisinesRepository _repository;
    private readonly ILogger<PreferredCuisinesService> _logger;

    public PreferredCuisinesService(IPreferredCuisinesRepository repository, ILogger<PreferredCuisinesService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PreferredCuisinesResponseDto> GetAllAsync()
    {
        try
        {
            // Retrieve raw data from repository
            var preferredCuisines = await _repository.GetAllAsync();

            // Map database models to DTOs with guard clauses
            var preferredCuisineDtos = preferredCuisines
                .Where(cuisine => !string.IsNullOrWhiteSpace(cuisine.Name)) // Filter out invalid entries early
                .Select(cuisine => new PreferredCuisineDto(
                    Id: cuisine.Id,
                    Name: cuisine.Name!.Trim() // Name is not null after Where filter
                ))
                .ToList();

            _logger.LogInformation("Successfully mapped {Count} preferred cuisines to DTOs", preferredCuisineDtos.Count);

            // Return wrapped response
            return new PreferredCuisinesResponseDto(preferredCuisineDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving preferred cuisines");
            throw;
        }
    }
}
