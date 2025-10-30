using PantryPal.Api.Repositories;
using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service implementation for diet types business logic
/// </summary>
public class DietTypesService : IDietTypesService
{
    private readonly IDietTypesRepository _repository;
    private readonly ILogger<DietTypesService> _logger;

    public DietTypesService(IDietTypesRepository repository, ILogger<DietTypesService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<DietTypesResponseDto> GetAllAsync()
    {
        try
        {
            // Retrieve raw data from repository
            var dietTypes = await _repository.GetAllAsync();

            // Map database models to DTOs with guard clauses
            var dietTypeDtos = dietTypes
                .Where(dietType => !string.IsNullOrWhiteSpace(dietType.Name)) // Filter out invalid entries early
                .Select(dietType => new DietTypeDto(
                    Id: dietType.Id,
                    Name: dietType.Name!.Trim() // Name is not null after Where filter
                ))
                .ToList();

            _logger.LogInformation("Successfully mapped {Count} diet types to DTOs", dietTypeDtos.Count);

            // Return wrapped response
            return new DietTypesResponseDto(dietTypeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving diet types");
            throw;
        }
    }
}
