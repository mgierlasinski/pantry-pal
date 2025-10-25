using PantryPal.Api.Repositories;
using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Service implementation for pantry items business logic
/// </summary>
public class PantryService : IPantryService
{
    private readonly IPantryRepository _repository;
    private readonly ILogger<PantryService> _logger;

    public PantryService(IPantryRepository repository, ILogger<PantryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PantryItemsPaginatedResponseDto> GetPantryItemsAsync(
        Guid userId,
        int page,
        int pageSize,
        bool? favorite,
        string sortField)
    {
        try
        {
            // Call repository to get raw data
            var (items, total) = await _repository.GetPantryItemsAsync(
                userId, 
                page, 
                pageSize, 
                favorite, 
                sortField);

            // Map database models to DTOs
            var itemDtos = items.Select(item => new PantryItemDto(
                Id: item.Id,
                Name: item.Name,
                IsFavorite: item.IsFavorite,
                CreatedAt: item.CreatedAt,
                UpdatedAt: item.UpdatedAt
            ));

            // Construct paginated response
            var response = new PantryItemsPaginatedResponseDto(
                Items: itemDtos,
                Page: page,
                PageSize: pageSize,
                Total: total
            );

            _logger.LogInformation(
                "Successfully retrieved {Count} pantry items for user {UserId} (page {Page}/{TotalPages})",
                itemDtos.Count(), userId, page, (int)Math.Ceiling(total / (double)pageSize));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error in PantryService while retrieving items for user {UserId} (page {Page}, pageSize {PageSize})",
                userId, page, pageSize);
            throw;
        }
    }
}

