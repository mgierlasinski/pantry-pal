namespace PantryPal.Data;

/// <summary>
/// Data Transfer Objects and Command Models for the PantryPal API
/// All DTOs are derived from database entity definitions in DatabaseTypes.cs
/// </summary>

// ================================
// Pantry Items DTOs
// ================================

/// <summary>
/// DTO for pantry item responses (derived from PantryItemsSelect)
/// </summary>
public record PantryItemDto(
    string Id,
    string Name,
    bool IsFavorite,
    string CreatedAt,
    string UpdatedAt
);

/// <summary>
/// DTO for creating new pantry items (derived from PantryItemsInsert)
/// </summary>
public record PantryItemCreateDto(string Name);

/// <summary>
/// DTO for updating pantry items (derived from PantryItemsUpdate)
/// All properties are optional for partial updates
/// </summary>
public record PantryItemUpdateDto(
    string? Name = null,
    bool? IsFavorite = null
);

/// <summary>
/// Paginated response for pantry items list
/// </summary>
public record PantryItemsPaginatedResponseDto(
    IEnumerable<PantryItemDto> Items,
    int Page,
    int PageSize,
    int Total
);

// ================================
// Recipe DTOs
// ================================

/// <summary>
/// DTO for recipe responses (derived from RecipesSelect)
/// </summary>
public record RecipeDto(
    string Id,
    string RecipeText,
    string CreatedAt,
    string UpdatedAt
);

/// <summary>
/// Paginated response for recipes list
/// </summary>
public record RecipesPaginatedResponseDto(
    IEnumerable<RecipeDto> Items,
    int Page,
    int PageSize,
    int Total
);

// ================================
// Recipe Generation DTOs
// ================================

/// <summary>
/// Response DTO for recipe generation endpoint
/// Contains generation metadata and the generated recipe text
/// </summary>
public record RecipeGenerateResponseDto(
    string GenerationId,
    string RecipeText
);

/// <summary>
/// Response DTO for accepting a generated recipe
/// </summary>
public record RecipeAcceptResponseDto(
    string RecipeId,
    string SavedAt
);

/// <summary>
/// Request DTO for rejecting a generated recipe
/// </summary>
public record RecipeRejectRequestDto(short RejectReasonId);

// ================================
// Recipe Generation Logs DTOs
// ================================

/// <summary>
/// DTO for recipe generation log entries (derived from RecipesGenerationsSelect)
/// </summary>
public record RecipeGenerationDto(
    string Id,
    string CreatedAt,
    int DurationMs,
    string? ErrorCode,
    string? ErrorMessage,
    string? GeneratedRecipeId,
    string Model,
    short? RejectReasonId
);

/// <summary>
/// Paginated response for recipe generations list
/// </summary>
public record RecipeGenerationsPaginatedResponseDto(
    IEnumerable<RecipeGenerationDto> Items,
    int Page,
    int PageSize,
    int Total
);

// ================================
// User Preferences DTOs
// ================================

/// <summary>
/// DTO for user preferences responses (derived from UserPreferencesSelect)
/// Includes resolved names for diet type and preferred cuisine for better usability
/// </summary>
public record UserPreferencesDto(
    string UserId,
    short DietTypeId,
    string DietTypeName,
    short PreferredCuisineId,
    string PreferredCuisineName,
    string? DislikedIngredients,
    string CreatedAt,
    string UpdatedAt
);

/// <summary>
/// DTO for creating/updating user preferences (derived from UserPreferencesInsert)
/// </summary>
public record UserPreferencesCreateDto(
    short DietTypeId,
    short PreferredCuisineId,
    string? DislikedIngredients
);

// ================================
// Dictionary DTOs
// ================================

/// <summary>
/// DTO for diet types (derived from DietTypesSelect)
/// </summary>
public record DietTypeDto(
    short Id,
    string Name
);

/// <summary>
/// Response DTO for diet types list endpoint
/// </summary>
public record DietTypesResponseDto(IEnumerable<DietTypeDto> DietTypes);

/// <summary>
/// DTO for preferred cuisines (derived from PreferredCuisinesSelect)
/// </summary>
public record PreferredCuisineDto(
    short Id,
    string Name
);

/// <summary>
/// Response DTO for preferred cuisines list endpoint
/// </summary>
public record PreferredCuisinesResponseDto(IEnumerable<PreferredCuisineDto> PreferredCuisines);

/// <summary>
/// DTO for recipe reject reasons (derived from RecipeRejectReasonsSelect)
/// </summary>
public record RecipeRejectReasonDto(
    short Id,
    string Description
);

/// <summary>
/// Response DTO for recipe reject reasons list endpoint
/// </summary>
public record RecipeRejectReasonsResponseDto(IEnumerable<RecipeRejectReasonDto> RejectReasons);