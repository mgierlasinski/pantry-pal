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
                GeneratedRecipeText = createdGeneration.GeneratedRecipeText,
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
                GeneratedRecipeText = updatedGeneration.GeneratedRecipeText,
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

    /// <inheritdoc />
    public async Task<RecipesGenerationsSelect?> GetByIdAsync(Guid generationId, Guid userId)
    {
        try
        {
            var response = await _supabaseClient
                .From<RecipesGenerationsSelect>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, generationId.ToString())
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
                .Single();

            _logger.LogInformation("Retrieved recipe generation {GenerationId} for user {UserId}",
                generationId, userId);

            return response;
        }
        catch (Exception ex)
        {
            // If not found, Supabase may throw an exception or return null
            _logger.LogWarning(ex, "Recipe generation {GenerationId} not found for user {UserId}",
                generationId, userId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<RecipesGenerationsSelect> MarkAsAcceptedAsync(Guid generationId, Guid recipeId)
    {
        try
        {
            var updateModel = new RecipesGenerationsUpdate
            {
                Id = generationId.ToString(),
                GeneratedRecipeId = recipeId.ToString()
            };

            var response = await _supabaseClient
                .From<RecipesGenerationsUpdate>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, generationId.ToString())
                .Update(updateModel);

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
                GeneratedRecipeText = updatedGeneration.GeneratedRecipeText,
                RejectReasonId = updatedGeneration.RejectReasonId,
                CreatedAt = updatedGeneration.CreatedAt
            };

            _logger.LogInformation("Marked recipe generation {GenerationId} as accepted with recipe {RecipeId}",
                generationId, recipeId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking recipe generation {GenerationId} as accepted",
                generationId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RecipesGenerationsSelect> UpdateRejectReasonAsync(Guid generationId, short rejectReasonId)
    {
        try
        {
            var updateModel = new RecipesGenerationsUpdate
            {
                Id = generationId.ToString(),
                RejectReasonId = rejectReasonId
            };

            var response = await _supabaseClient
                .From<RecipesGenerationsUpdate>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, generationId.ToString())
                .Update(updateModel);

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
                GeneratedRecipeText = updatedGeneration.GeneratedRecipeText,
                RejectReasonId = updatedGeneration.RejectReasonId,
                CreatedAt = updatedGeneration.CreatedAt
            };

            _logger.LogInformation("Updated recipe generation {GenerationId} with reject reason {RejectReasonId}",
                generationId, rejectReasonId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reject reason for recipe generation {GenerationId}",
                generationId);
            throw;
        }
    }
}

