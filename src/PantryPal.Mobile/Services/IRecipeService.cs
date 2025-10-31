using PantryPal.Data;

namespace PantryPal.Mobile.Services;

/// <summary>
/// Service for managing recipe generation and acceptance/rejection
/// </summary>
public interface IRecipeService
{
    /// <summary>
    /// Gets the list of available recipe rejection reasons
    /// </summary>
    Task<List<RecipeRejectReasonDto>> GetRejectReasonsAsync();

    /// <summary>
    /// Generates a new recipe based on user's pantry and preferences
    /// </summary>
    Task<RecipeGenerateResponseDto> GenerateRecipeAsync();

    /// <summary>
    /// Accepts a generated recipe and saves it to the user's collection
    /// </summary>
    Task<RecipeAcceptResponseDto> AcceptRecipeAsync(string generationId);

    /// <summary>
    /// Rejects a generated recipe with a specified reason
    /// </summary>
    Task RejectRecipeAsync(string generationId, RecipeRejectRequestDto payload);
}

