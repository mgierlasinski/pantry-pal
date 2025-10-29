using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Data;
using System.Diagnostics;

namespace PantryPal.Api.Services;

/// <summary>
/// Service implementation for recipes business logic
/// </summary>
public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IPantryRepository _pantryRepository;
    private readonly IUserPreferencesRepository _userPreferencesRepository;
    private readonly IRecipesGenerationsRepository _recipesGenerationsRepository;
    private readonly IAIRecipeGeneratorService _aiService;
    private readonly ILogger<RecipeService> _logger;

    private const string DefaultAIModel = "mock-gpt-4";

    public RecipeService(
        IRecipeRepository recipeRepository,
        IPantryRepository pantryRepository,
        IUserPreferencesRepository userPreferencesRepository,
        IRecipesGenerationsRepository recipesGenerationsRepository,
        IAIRecipeGeneratorService aiService,
        ILogger<RecipeService> logger)
    {
        _recipeRepository = recipeRepository;
        _pantryRepository = pantryRepository;
        _userPreferencesRepository = userPreferencesRepository;
        _recipesGenerationsRepository = recipesGenerationsRepository;
        _aiService = aiService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RecipesPaginatedResponseDto> GetRecipesAsync(
        Guid userId,
        int page,
        int pageSize)
    {
        try
        {
            // Call repository to get raw data
            var (items, total) = await _recipeRepository.GetRecipesAsync(
                userId,
                page,
                pageSize);

            // Map database models to DTOs
            var recipeDtos = items.Select(recipe => new RecipeDto(
                Id: recipe.Id,
                RecipeText: recipe.RecipeText,
                CreatedAt: recipe.CreatedAt,
                UpdatedAt: recipe.UpdatedAt
            ));

            // Construct paginated response
            var response = new RecipesPaginatedResponseDto(
                Items: recipeDtos,
                Page: page,
                PageSize: pageSize,
                Total: total
            );

            _logger.LogInformation(
                "Successfully retrieved {Count} recipes for user {UserId} (page {Page}/{TotalPages})",
                recipeDtos.Count(), userId, page, (int)Math.Ceiling(total / (double)pageSize));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error in RecipeService while retrieving recipes for user {UserId} (page {Page}, pageSize {PageSize})",
                userId, page, pageSize);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RecipeGenerateResponseDto> GenerateRecipeAsync(Guid userId)
    {
        var generationId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting recipe generation for user {UserId}", userId);

            // Step 1: Validate user has preferences set
            var userPreferences = await _userPreferencesRepository.GetUserPreferencesAsync(userId);
            if (userPreferences == null)
            {
                _logger.LogWarning("User {UserId} does not have preferences set", userId);
                throw new InvalidOperationException("User preferences not set.");
            }

            // Step 2: Retrieve pantry items
            var pantryItems = await _pantryRepository.GetAllPantryItemsAsync(userId);
            var pantryItemsList = pantryItems.ToList();
            
            if (!pantryItemsList.Any())
            {
                _logger.LogWarning("User {UserId} has an empty pantry", userId);
                throw new InvalidOperationException("Pantry is empty.");
            }

            // Step 3: Create initial generation record (without recipe text yet)
            var generationInsert = new RecipesGenerationsInsert
            {
                Id = generationId,
                UserId = userId.ToString(),
                Model = DefaultAIModel,
                DurationMs = 0
            };

            var generation = await _recipesGenerationsRepository.CreateGenerationAsync(generationInsert);
            _logger.LogInformation("Created generation record {GenerationId} for user {UserId}", 
                generation.Id, userId);

            // Step 4: Build AI prompt
            var ingredientsList = string.Join(", ", pantryItemsList.Select(p => p.Name));
            var dislikedIngredients = !string.IsNullOrEmpty(userPreferences.DislikedIngredients) 
                ? userPreferences.DislikedIngredients 
                : "none";

            var prompt = $@"Generate a recipe using the following ingredients:
Ingredients: {ingredientsList}

User Preferences:
- Diet Type ID: {userPreferences.DietTypeId}
- Preferred Cuisine ID: {userPreferences.PreferredCuisineId}
- Disliked Ingredients: {dislikedIngredients}

Please create a detailed recipe in markdown format with ingredients, instructions, and notes.";

            _logger.LogInformation("Calling AI service to generate recipe for user {UserId}", userId);

            // Step 5: Call AI service
            string recipeText;
            try
            {
                recipeText = await _aiService.GenerateAsync(prompt);
                stopwatch.Stop();
                _logger.LogInformation("AI service successfully generated recipe in {Duration}ms", 
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception aiEx)
            {
                stopwatch.Stop();
                _logger.LogError(aiEx, "AI service failed to generate recipe for user {UserId}", userId);

                // Update generation record with error
                var errorUpdate = new RecipesGenerationsUpdate
                {
                    Id = generationId,
                    DurationMs = (int)stopwatch.ElapsedMilliseconds,
                    ErrorCode = "AI_SERVICE_ERROR",
                    ErrorMessage = aiEx.Message
                };
                await _recipesGenerationsRepository.UpdateGenerationAsync(errorUpdate);

                throw new InvalidOperationException("Failed to generate recipe. Please try again later.", aiEx);
            }

            // Step 6: Update generation record with success and store recipe text
            // Recipe text is stored temporarily in generations table for hybrid approach:
            // - Client can use the returned recipe text immediately
            // - If client crashes, recipe text can be retrieved from generation record
            // - Recipe will be moved to recipes table when user calls accept endpoint
            var successUpdate = new RecipesGenerationsUpdate
            {
                Id = generationId,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
                GeneratedRecipeText = recipeText
            };
            await _recipesGenerationsRepository.UpdateGenerationAsync(successUpdate);

            _logger.LogInformation(
                "Successfully completed recipe generation {GenerationId} for user {UserId} in {Duration}ms",
                generationId, userId, stopwatch.ElapsedMilliseconds);

            // Step 7: Return response (recipe stored in generations, not in recipes table yet)
            return new RecipeGenerateResponseDto(
                GenerationId: generationId,
                RecipeText: recipeText
            );
        }
        catch (InvalidOperationException)
        {
            // Re-throw validation errors (preferences not set, empty pantry, AI failure)
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, 
                "Unexpected error during recipe generation for user {UserId}", 
                userId);

            // Attempt to update generation record with error
            try
            {
                var errorUpdate = new RecipesGenerationsUpdate
                {
                    Id = generationId,
                    DurationMs = (int)stopwatch.ElapsedMilliseconds,
                    ErrorCode = "INTERNAL_ERROR",
                    ErrorMessage = ex.Message
                };
                await _recipesGenerationsRepository.UpdateGenerationAsync(errorUpdate);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, 
                    "Failed to update generation record {GenerationId} with error status", 
                    generationId);
            }

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<RecipeAcceptResponseDto> AcceptGeneratedRecipeAsync(Guid generationId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Starting recipe acceptance for generation {GenerationId} by user {UserId}",
                generationId, userId);

            // Step 1: Retrieve generation record
            var generation = await _recipesGenerationsRepository.GetByIdAsync(generationId, userId);

            // Step 2: Validate generation exists
            if (generation == null)
            {
                _logger.LogWarning("Generation {GenerationId} not found for user {UserId}",
                    generationId, userId);
                throw new ArgumentException("Generation not found");
            }

            // Step 3: Validate not already accepted
            if (!string.IsNullOrEmpty(generation.GeneratedRecipeId))
            {
                _logger.LogWarning("Generation {GenerationId} already accepted for user {UserId}",
                    generationId, userId);
                throw new InvalidOperationException("Already accepted");
            }

            // Step 4: Validate recipe text exists
            if (string.IsNullOrWhiteSpace(generation.GeneratedRecipeText))
            {
                _logger.LogWarning("Generation {GenerationId} has no recipe text for user {UserId}",
                    generationId, userId);
                throw new InvalidOperationException("No recipe text available");
            }

            // Step 5: Create recipe in recipes table
            var recipeInsert = new RecipesInsert
            {
                UserId = userId.ToString(),
                RecipeText = generation.GeneratedRecipeText
            };

            var createdRecipe = await _recipeRepository.CreateRecipeAsync(recipeInsert);

            _logger.LogInformation("Created recipe {RecipeId} from generation {GenerationId}",
                createdRecipe.Id, generationId);

            // Step 6: Mark generation as accepted (link to recipe)
            var recipeId = Guid.Parse(createdRecipe.Id);
            await _recipesGenerationsRepository.MarkAsAcceptedAsync(generationId, recipeId);

            _logger.LogInformation(
                "Successfully accepted generation {GenerationId} and created recipe {RecipeId} for user {UserId}",
                generationId, createdRecipe.Id, userId);

            // Step 7: Return response
            return new RecipeAcceptResponseDto(
                RecipeId: createdRecipe.Id,
                SavedAt: createdRecipe.CreatedAt
            );
        }
        catch (ArgumentException)
        {
            // Re-throw generation not found
            throw;
        }
        catch (InvalidOperationException)
        {
            // Re-throw validation errors (already accepted, no recipe text)
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while accepting generation {GenerationId} for user {UserId}",
                generationId, userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RejectGeneratedRecipeAsync(Guid generationId, short rejectReasonId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Starting recipe rejection for generation {GenerationId} with reason {RejectReasonId} by user {UserId}",
                generationId, rejectReasonId, userId);

            // Step 1: Retrieve generation record
            var generation = await _recipesGenerationsRepository.GetByIdAsync(generationId, userId);

            // Step 2: Validate generation exists and belongs to user
            if (generation == null)
            {
                _logger.LogWarning("Generation {GenerationId} not found for user {UserId}",
                    generationId, userId);
                throw new ArgumentException("Generation not found");
            }

            // Step 3: Validate not already rejected
            if (generation.RejectReasonId.HasValue)
            {
                _logger.LogWarning("Generation {GenerationId} already rejected with reason {ExistingReasonId} for user {UserId}",
                    generationId, generation.RejectReasonId.Value, userId);
                throw new InvalidOperationException("Already rejected");
            }

            // Step 4: Update generation with reject reason
            await _recipesGenerationsRepository.UpdateRejectReasonAsync(generationId, rejectReasonId);

            _logger.LogInformation(
                "Successfully rejected generation {GenerationId} with reason {RejectReasonId} for user {UserId}",
                generationId, rejectReasonId, userId);
        }
        catch (ArgumentException)
        {
            // Re-throw generation not found
            throw;
        }
        catch (InvalidOperationException)
        {
            // Re-throw validation errors (already rejected)
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while rejecting generation {GenerationId} for user {UserId}",
                generationId, userId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteRecipeAsync(string recipeId, Guid userId)
    {
        try
        {
            // Early guard clause: validate recipe ID format
            if (!Guid.TryParse(recipeId, out _))
            {
                _logger.LogWarning("Invalid recipe ID format: {RecipeId}", recipeId);
                throw new ArgumentException("Invalid recipe ID format.", nameof(recipeId));
            }

            // Query repository to fetch recipe for ownership verification
            var recipe = await _recipeRepository.GetByIdAsync(recipeId);

            // Check if recipe exists
            if (recipe == null)
            {
                _logger.LogWarning("Recipe {RecipeId} not found", recipeId);
                throw new KeyNotFoundException("Recipe not found.");
            }

            // Verify ownership by comparing user IDs
            if (recipe.UserId != userId.ToString())
            {
                _logger.LogWarning(
                    "User {UserId} attempted to delete recipe {RecipeId} owned by {OwnerId}",
                    userId, recipeId, recipe.UserId);
                throw new KeyNotFoundException("Recipe not found.");
            }

            // Perform the deletion
            await _recipeRepository.DeleteAsync(recipeId);

            _logger.LogInformation("Recipe {RecipeId} successfully deleted by user {UserId}", recipeId, userId);
        }
        catch (ArgumentException)
        {
            // Re-throw validation errors
            throw;
        }
        catch (KeyNotFoundException)
        {
            // Re-throw not found errors (including unauthorized access)
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while deleting recipe {RecipeId} for user {UserId}",
                recipeId, userId);
            throw;
        }
    }
}
