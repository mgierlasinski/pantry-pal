namespace PantryPal.Api.Services;

/// <summary>
/// Mock implementation of AI recipe generator service for testing
/// Returns a hardcoded markdown recipe
/// </summary>
public class MockAIRecipeGeneratorService : IAIRecipeGeneratorService
{
    private readonly ILogger<MockAIRecipeGeneratorService> _logger;

    public MockAIRecipeGeneratorService(ILogger<MockAIRecipeGeneratorService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> GenerateAsync(string prompt)
    {
        _logger.LogInformation("Mock AI service generating recipe with prompt length: {Length}", prompt.Length);
        
        // Simulate API delay
        await Task.Delay(500);

        // Return a mock recipe in markdown format
        return @"# Delicious Mock Recipe

## Ingredients
- 2 cups of available pantry items
- 1 tablespoon of creativity
- A pinch of user preferences

## Instructions

1. Preheat your imagination to 350°F (175°C).
2. Combine all available pantry items in a large bowl.
3. Mix thoroughly while considering dietary preferences.
4. Cook until perfectly done.
5. Serve with a smile!

## Notes

This is a mock recipe generated for testing purposes. In production, this would be replaced with actual AI-generated content based on your pantry items and preferences.

**Prep Time:** 10 minutes  
**Cook Time:** 20 minutes  
**Total Time:** 30 minutes  
**Servings:** 4
";
    }
}

