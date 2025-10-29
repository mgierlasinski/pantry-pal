using PantryPal.Api.Db;
using Supabase;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository implementation for recipes data access using Supabase
/// </summary>
public class RecipeRepository : IRecipeRepository
{
    private readonly Client _supabaseClient;
    private readonly ILogger<RecipeRepository> _logger;

    public RecipeRepository(Client supabaseClient, ILogger<RecipeRepository> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<RecipesSelect> Items, int Total)> GetRecipesAsync(
        Guid userId,
        int page,
        int pageSize)
    {
        try
        {
            // Build the base query filtered by user_id
            var query = _supabaseClient
                .From<RecipesSelect>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString());

            // Apply sorting by created_at descending (newest first)
            query = query.Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending);

            // Get total count before applying pagination
            var countQuery = _supabaseClient
                .From<RecipesSelect>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString());

            // Execute count query
            var total = await countQuery.Count(Supabase.Postgrest.Constants.CountType.Exact);

            // Apply pagination (convert to 0-based index)
            var from = (page - 1) * pageSize;
            var to = from + pageSize - 1;
            query = query.Range(from, to);

            // Execute the main query
            var response = await query.Get();

            _logger.LogInformation(
                "Retrieved {Count} recipes for user {UserId} (page {Page}, total {Total})",
                response.Models.Count, userId, page, total);

            return (response.Models, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving recipes for user {UserId} (page {Page}, pageSize {PageSize})",
                userId, page, pageSize);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RecipesSelect> CreateRecipeAsync(RecipesInsert model)
    {
        try
        {
            var response = await _supabaseClient
                .From<RecipesInsert>()
                .Insert(model);

            var createdRecipe = response.Models.First();

            // Convert back to select model for consistency
            var result = new RecipesSelect
            {
                Id = createdRecipe.Id,
                RecipeText = createdRecipe.RecipeText,
                CreatedAt = createdRecipe.CreatedAt,
                UpdatedAt = createdRecipe.UpdatedAt,
                UserId = createdRecipe.UserId
            };

            _logger.LogInformation("Successfully created recipe {RecipeId} for user {UserId}",
                result.Id, result.UserId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recipe for user {UserId}",
                model.UserId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RecipesSelect?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _supabaseClient
                .From<RecipesSelect>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Get();

            var recipe = response.Models.FirstOrDefault();

            _logger.LogInformation("Retrieved recipe {RecipeId}: {Found}",
                id, recipe != null ? "found" : "not found");

            return recipe;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recipe {RecipeId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id)
    {
        try
        {
            await _supabaseClient
                .From<RecipesSelect>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id)
                .Delete();

            _logger.LogInformation("Successfully deleted recipe {RecipeId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting recipe {RecipeId}", id);
            throw;
        }
    }
}
