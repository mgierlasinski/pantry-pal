using PantryPal.Api.Db;
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
        string sortField)
    {
        try
        {
            // Call repository to get raw data
            var (items, total) = await _repository.GetPantryItemsAsync(
                userId, 
                page, 
                pageSize, 
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

    /// <inheritdoc />
    public async Task<PantryItemDto> CreatePantryItemAsync(Guid userId, PantryItemCreateDto dto)
    {
        try
        {
            // Construct PantryItemsInsert model with UserId and Name
            var insertModel = new PantryItemsInsert
            {
                UserId = userId.ToString(),
                Name = dto.Name, // Validation already done at endpoint level
                IsFavorite = false
            };

            // Call repository to create the item
            var createdItem = await _repository.CreatePantryItemAsync(insertModel);

            // Map database model to DTO
            var result = new PantryItemDto(
                Id: createdItem.Id,
                Name: createdItem.Name,
                IsFavorite: createdItem.IsFavorite,
                CreatedAt: createdItem.CreatedAt,
                UpdatedAt: createdItem.UpdatedAt
            );

            _logger.LogInformation("Successfully created pantry item {ItemId} '{ItemName}' for user {UserId}",
                result.Id, result.Name, userId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pantry item for user {UserId} with name '{ItemName}'",
                userId, dto.Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PantryItemDto> UpdatePantryItemAsync(Guid id, Guid userId, PantryItemUpdateDto dto)
    {
        try
        {
            var updateModel = new PantryItemsUpdate
            {
                Id = id.ToString(),
                UserId = userId.ToString(),
                Name = dto.Name,
                IsFavorite = dto.IsFavorite
            };

            var updatedItem = await _repository.UpdatePantryItemAsync(updateModel);

            // Map to DTO
            var result = new PantryItemDto(
                Id: updatedItem.Id,
                Name: updatedItem.Name,
                IsFavorite: updatedItem.IsFavorite,
                CreatedAt: updatedItem.CreatedAt,
                UpdatedAt: updatedItem.UpdatedAt
            );

            _logger.LogInformation("Successfully updated pantry item {ItemId} '{ItemName}' for user {UserId}",
                result.Id, result.Name, userId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pantry item {ItemId} for user {UserId}", id, userId);
            throw;
        }
    }
}

