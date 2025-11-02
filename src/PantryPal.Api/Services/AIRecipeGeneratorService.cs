using PantryPal.Api.Services.OpenRouter;
using PantryPal.Data;

namespace PantryPal.Api.Services;

/// <summary>
/// Real implementation of AI recipe generator service using OpenRouter
/// Generates structured recipes based on ingredients and user preferences
/// </summary>
public class AIRecipeGeneratorService : IAIRecipeGeneratorService
{
    private readonly IOpenRouterService _openRouterService;
    private readonly ILogger<AIRecipeGeneratorService> _logger;

    public AIRecipeGeneratorService(
        IOpenRouterService openRouterService,
        ILogger<AIRecipeGeneratorService> logger)
    {
        _openRouterService = openRouterService ?? throw new ArgumentNullException(nameof(openRouterService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<string> GenerateAsync(string prompt)
    {
        try
        {
            _logger.LogInformation("Generating recipe with prompt length: {Length}", prompt.Length);

            var systemMessage = @"You are a master chef and culinary expert. Your task is to create a delicious, practical recipe based on the ingredients provided by the user. You must respond only with a JSON object that strictly adheres to the provided schema. Do not include any text outside of the JSON object.

Guidelines for recipe creation:
- Use only the ingredients mentioned in the prompt, or basic pantry staples that are commonly available
- Create recipes that are realistic and can actually be prepared with the given ingredients
- Ensure the recipe is balanced, nutritious, and appealing
- Provide clear, step-by-step instructions that anyone can follow
- Include appropriate cooking times and temperatures
- Suggest realistic serving sizes
- Make the recipe name creative and enticing
- Write a brief but appealing description of the dish";

            var userMessage = $"Please create a recipe using the following information: {prompt}";

            // Define the JSON schema for the recipe structure
            var recipeSchema = new
            {
                type = "object",
                properties = new
                {
                    recipeName = new { type = "string", description = "An enticing, creative name for the recipe" },
                    description = new { type = "string", description = "A brief, appealing description of the dish and its appeal" },
                    prepTimeMinutes = new { type = "integer", description = "Estimated preparation time in minutes (realistic for the recipe)" },
                    cookTimeMinutes = new { type = "integer", description = "Estimated cooking time in minutes (realistic for the recipe)" },
                    servings = new { type = "integer", description = "Number of servings the recipe makes (2-8)" },
                    ingredients = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "A list of ingredients with quantities, using the provided ingredients where possible"
                    },
                    instructions = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Step-by-step cooking instructions, clear and easy to follow"
                    }
                },
                required = new[] { "recipeName", "description", "prepTimeMinutes", "cookTimeMinutes", "servings", "ingredients", "instructions" },
                additionalProperties = false
            };

            // Get structured response from OpenRouter
            var recipe = await _openRouterService.GetStructuredResponseAsync<AIRecipeDto>(
                systemMessage,
                userMessage,
                recipeSchema);

            if (recipe == null)
            {
                _logger.LogWarning("Failed to generate recipe - OpenRouter service returned null");
                throw new InvalidOperationException("Failed to generate recipe. Please try again.");
            }

            // Convert structured recipe to markdown format
            var markdownRecipe = ConvertToMarkdown(recipe);
            _logger.LogInformation("Successfully generated recipe: {RecipeName}", recipe.RecipeName);

            return markdownRecipe;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recipe with AI service");
            throw new InvalidOperationException("Failed to generate recipe. Please try again.", ex);
        }
    }

    private static string ConvertToMarkdown(AIRecipeDto recipe)
    {
        var sb = new System.Text.StringBuilder();

        // Title
        sb.AppendLine($"# {recipe.RecipeName}");
        sb.AppendLine();

        // Description
        sb.AppendLine($"{recipe.Description}");
        sb.AppendLine();

        // Ingredients section
        sb.AppendLine("## Ingredients");
        foreach (var ingredient in recipe.Ingredients)
        {
            sb.AppendLine($"- {ingredient}");
        }
        sb.AppendLine();

        // Instructions section
        sb.AppendLine("## Instructions");
        sb.AppendLine();
        for (int i = 0; i < recipe.Instructions.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {recipe.Instructions[i]}");
        }
        sb.AppendLine();

        // Notes section
        sb.AppendLine("## Notes");
        sb.AppendLine();
        sb.AppendLine($"**Prep Time:** {recipe.PrepTimeMinutes} minutes");
        sb.AppendLine($"**Cook Time:** {recipe.CookTimeMinutes} minutes");
        sb.AppendLine($"**Total Time:** {recipe.PrepTimeMinutes + recipe.CookTimeMinutes} minutes");
        sb.AppendLine($"**Servings:** {recipe.Servings}");

        return sb.ToString();
    }
}
