using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service implementation for recipes business logic
/// </summary>
public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _repository;
    private readonly ILogger<RecipeService> _logger;

    public RecipeService(IRecipeRepository repository, ILogger<RecipeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RecipesPaginatedResponseDto> GetRecipesAsync(
        Guid userId,
        int page,
        int pageSize)
    {
        try
        {
            // Call repository to get raw data
            var (items, total) = await _repository.GetRecipesAsync(
                userId,
                page,
                pageSize);

            // Map database models to DTOs
            var recipeDtos = items.Select(recipe => new RecipeDto(
                Id: recipe.Id,
                RecipeText: recipe.RecipeText,
                CreatedAt: recipe.CreatedAt,
                UpdatedAt: recipe.UpdatedAt
            ));

            // Construct paginated response
            var response = new RecipesPaginatedResponseDto(
                Items: recipeDtos,
                Page: page,
                PageSize: pageSize,
                Total: total
            );

            _logger.LogInformation(
                "Successfully retrieved {Count} recipes for user {UserId} (page {Page}/{TotalPages})",
                recipeDtos.Count(), userId, page, (int)Math.Ceiling(total / (double)pageSize));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error in RecipeService while retrieving recipes for user {UserId} (page {Page}, pageSize {PageSize})",
                userId, page, pageSize);
            throw;
        }
    }
}
