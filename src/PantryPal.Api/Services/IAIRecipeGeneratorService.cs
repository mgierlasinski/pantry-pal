namespace PantryPal.Api.Services;

/// <summary>
/// Service interface for AI-powered recipe generation
/// </summary>
public interface IAIRecipeGeneratorService
{
    /// <summary>
    /// Generates a recipe based on the provided prompt
    /// </summary>
    /// <param name="prompt">The prompt containing ingredients and preferences</param>
    /// <returns>The generated recipe text in markdown format</returns>
    Task<string> GenerateAsync(string prompt);
}

