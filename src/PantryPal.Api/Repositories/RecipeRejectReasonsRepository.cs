using PantryPal.Api.Db;
using Supabase;

namespace PantryPal.Api.Repositories;

/// <summary>
/// Repository implementation for recipe reject reasons data access operations
/// </summary>
public class RecipeRejectReasonsRepository : IRecipeRejectReasonsRepository
{
    private readonly Client _supabaseClient;

    public RecipeRejectReasonsRepository(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    /// <inheritdoc />
    public async Task<RecipeRejectReasonsSelect?> GetByIdAsync(short id)
    {
        var response = await _supabaseClient
            .From<RecipeRejectReasonsSelect>()
            .Where(x => x.Id == id)
            .Single();

        return response;
    }
}
