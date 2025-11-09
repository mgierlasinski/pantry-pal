using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PantryPal.Api.Repositories;
using PantryPal.Api.Services;
using PantryPal.Api.Services.OpenRouter;

/// <summary>
/// Test fixture for RecipeService tests
/// </summary>
public class RecipeServiceTestFixture
{
    public Mock<IRecipeRepository> MockRecipeRepository { get; }
    public Mock<IPantryRepository> MockPantryRepository { get; }
    public Mock<IUserPreferencesRepository> MockUserPreferencesRepository { get; }
    public Mock<IRecipesGenerationsRepository> MockRecipesGenerationsRepository { get; }
    public Mock<IAIRecipeGeneratorService> MockAIService { get; }
    public Mock<ILogger<RecipeService>> MockLogger { get; }
    public Mock<IOptions<OpenRouterOptions>> MockOptions { get; }
    public RecipeService Service { get; }

    public RecipeServiceTestFixture()
    {
        MockRecipeRepository = new Mock<IRecipeRepository>();
        MockPantryRepository = new Mock<IPantryRepository>();
        MockUserPreferencesRepository = new Mock<IUserPreferencesRepository>();
        MockRecipesGenerationsRepository = new Mock<IRecipesGenerationsRepository>();
        MockAIService = new Mock<IAIRecipeGeneratorService>();
        MockLogger = new Mock<ILogger<RecipeService>>();
        MockOptions = new Mock<IOptions<OpenRouterOptions>>();

        // Setup the options mock to return valid options
        MockOptions.Setup(o => o.Value).Returns(new OpenRouterOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://test.com",
            SiteName = "Test",
            Model = "gpt-4"
        });

        Service = new RecipeService(
            MockRecipeRepository.Object,
            MockPantryRepository.Object,
            MockUserPreferencesRepository.Object,
            MockRecipesGenerationsRepository.Object,
            MockAIService.Object,
            MockLogger.Object,
            MockOptions.Object
        );
    }
}
