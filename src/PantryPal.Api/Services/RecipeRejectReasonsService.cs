using Microsoft.Extensions.Logging;
using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service implementation for recipe reject reasons business logic operations
/// </summary>
public class RecipeRejectReasonsService : IRecipeRejectReasonsService
{
    private readonly IRecipeRejectReasonsRepository _repository;
    private readonly ILogger<RecipeRejectReasonsService> _logger;

    public RecipeRejectReasonsService(
        IRecipeRejectReasonsRepository repository,
        ILogger<RecipeRejectReasonsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RecipeRejectReasonDto>> GetAllAsync()
    {
        try
        {
            var entities = await _repository.GetAllAsync();

            // Guard clause: Check if no reject reasons found (unexpected for seeded data)
            if (!entities.Any())
            {
                _logger.LogError("No reject reasons found in database. This indicates missing seed data.");
                throw new InvalidOperationException("Configuration error: No reject reasons found.");
            }

            // Map entities to DTOs
            var dtos = entities.Select(entity => new RecipeRejectReasonDto(
                Id: entity.Id,
                Description: entity.Description
            ));

            _logger.LogInformation("Successfully retrieved {Count} reject reasons", dtos.Count());
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve reject reasons");
            throw;
        }
    }
}
