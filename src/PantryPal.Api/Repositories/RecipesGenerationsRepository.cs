using PantryPal.Api.Db;
using Supabase;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository implementation for recipe generations data access using Supabase
/// </summary>
public class RecipesGenerationsRepository : IRecipesGenerationsRepository
{
    private readonly Client _supabaseClient;
    private readonly ILogger<RecipesGenerationsRepository> _logger;

    public RecipesGenerationsRepository(Client supabaseClient, ILogger<RecipesGenerationsRepository> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RecipesGenerationsSelect> CreateGenerationAsync(RecipesGenerationsInsert model)
    {
        try
        {
            var response = await _supabaseClient
                .From<RecipesGenerationsInsert>()
                .Insert(model);

            var createdGeneration = response.Models.First();

            // Convert back to select model for consistency
            var result = new RecipesGenerationsSelect
            {
                Id = createdGeneration.Id,
                UserId = createdGeneration.UserId,
                Model = createdGeneration.Model,
                DurationMs = createdGeneration.DurationMs,
                ErrorCode = createdGeneration.ErrorCode,
                ErrorMessage = createdGeneration.ErrorMessage,
                GeneratedRecipeId = createdGeneration.GeneratedRecipeId,
                RejectReasonId = createdGeneration.RejectReasonId,
                CreatedAt = createdGeneration.CreatedAt
            };

            _logger.LogInformation("Successfully created recipe generation {GenerationId} for user {UserId}",
                result.Id, result.UserId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recipe generation for user {UserId}",
                model.UserId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RecipesGenerationsSelect> UpdateGenerationAsync(RecipesGenerationsUpdate model)
    {
        try
        {
            var response = await _supabaseClient
                .From<RecipesGenerationsUpdate>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, model.Id)
                .Update(model);

            if (!response.Models.Any())
            {
                throw new ArgumentException("Recipe generation not found");
            }

            var updatedGeneration = response.Models.First();

            // Convert back to select model for consistency
            var result = new RecipesGenerationsSelect
            {
                Id = updatedGeneration.Id,
                UserId = updatedGeneration.UserId,
                Model = updatedGeneration.Model,
                DurationMs = updatedGeneration.DurationMs ?? 0,
                ErrorCode = updatedGeneration.ErrorCode,
                ErrorMessage = updatedGeneration.ErrorMessage,
                GeneratedRecipeId = updatedGeneration.GeneratedRecipeId,
                RejectReasonId = updatedGeneration.RejectReasonId,
                CreatedAt = updatedGeneration.CreatedAt
            };

            _logger.LogInformation("Successfully updated recipe generation {GenerationId}",
                result.Id);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recipe generation {GenerationId}",
                model.Id);
            throw;
        }
    }
}

