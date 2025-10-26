using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace PantryPal.Api.Db;

[Table("diet_types")]
public class DietTypesSelect : BaseModel
{
    [PrimaryKey("id")]
    public short Id { get; set; }

    [Column("name")]
    public string Name { get; set; }
}

[Table("diet_types")]
public class DietTypesInsert : BaseModel
{
    [Column("id")]
    public short? Id { get; set; }

    [Column("name")]
    public string Name { get; set; }
}

[Table("diet_types")]
public class DietTypesUpdate : BaseModel
{
    [Column("id")]
    public short? Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }
}

[Table("preferred_cuisines")]
public class PreferredCuisinesSelect : BaseModel
{
    [PrimaryKey("id")]
    public short Id { get; set; }

    [Column("name")]
    public string Name { get; set; }
}

[Table("preferred_cuisines")]
public class PreferredCuisinesInsert : BaseModel
{
    [Column("id")]
    public short? Id { get; set; }

    [Column("name")]
    public string Name { get; set; }
}

[Table("preferred_cuisines")]
public class PreferredCuisinesUpdate : BaseModel
{
    [Column("id")]
    public short? Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }
}

[Table("recipe_reject_reasons")]
public class RecipeRejectReasonsSelect : BaseModel
{
    [Column("description")]
    public string Description { get; set; }

    [PrimaryKey("id")]
    public short Id { get; set; }
}

[Table("recipe_reject_reasons")]
public class RecipeRejectReasonsInsert : BaseModel
{
    [Column("description")]
    public string Description { get; set; }

    [Column("id")]
    public short? Id { get; set; }
}

[Table("recipe_reject_reasons")]
public class RecipeRejectReasonsUpdate : BaseModel
{
    [Column("description")]
    public string? Description { get; set; }

    [Column("id")]
    public short? Id { get; set; }
}

[Table("pantry_items")]
public class PantryItemsSelect : BaseModel
{
    [Column("created_at")]
    public string CreatedAt { get; set; }

    [PrimaryKey("id")]
    public string Id { get; set; }

    [Column("is_favorite")]
    public bool IsFavorite { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("updated_at")]
    public string UpdatedAt { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }
}

[Table("pantry_items")]
public class PantryItemsInsert : BaseModel
{
    [Column("created_at", nullValueHandling: NullValueHandling.Ignore)]
    public string? CreatedAt { get; set; }

    [PrimaryKey("id")]
    public string? Id { get; set; }

    [Column("is_favorite")]
    public bool IsFavorite { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("updated_at", nullValueHandling: NullValueHandling.Ignore)]
    public string? UpdatedAt { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }
}

[Table("pantry_items")]
public class PantryItemsUpdate : BaseModel
{
    [Column("created_at", nullValueHandling: NullValueHandling.Ignore)]
    public string? CreatedAt { get; set; }

    [PrimaryKey("id")]
    public string? Id { get; set; }

    [Column("is_favorite")]
    public bool? IsFavorite { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("updated_at", nullValueHandling: NullValueHandling.Ignore)]
    public string? UpdatedAt { get; set; }

    [Column("user_id")]
    public string? UserId { get; set; }
}

[Table("recipes")]
public class RecipesSelect : BaseModel
{
    [Column("created_at")]
    public string CreatedAt { get; set; }

    [PrimaryKey("id")]
    public string Id { get; set; }

    [Column("recipe_text")]
    public string RecipeText { get; set; }

    [Column("updated_at")]
    public string UpdatedAt { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }
}

[Table("recipes")]
public class RecipesInsert : BaseModel
{
    [Column("created_at")]
    public string? CreatedAt { get; set; }

    [Column("id")]
    public string? Id { get; set; }

    [Column("recipe_text")]
    public string RecipeText { get; set; }

    [Column("updated_at")]
    public string? UpdatedAt { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }
}

[Table("recipes")]
public class RecipesUpdate : BaseModel
{
    [Column("created_at")]
    public string? CreatedAt { get; set; }

    [Column("id")]
    public string? Id { get; set; }

    [Column("recipe_text")]
    public string? RecipeText { get; set; }

    [Column("updated_at")]
    public string? UpdatedAt { get; set; }

    [Column("user_id")]
    public string? UserId { get; set; }
}

[Table("recipes_generations")]
public class RecipesGenerationsSelect : BaseModel
{
    [Column("created_at")]
    public string CreatedAt { get; set; }

    [Column("duration_ms")]
    public int DurationMs { get; set; }

    [Column("error_code")]
    public string? ErrorCode { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("generated_recipe_id")]
    public string? GeneratedRecipeId { get; set; }

    [PrimaryKey("id")]
    public string Id { get; set; }

    [Column("model")]
    public string Model { get; set; }

    [Column("reject_reason_id")]
    public short? RejectReasonId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }
}

[Table("recipes_generations")]
public class RecipesGenerationsInsert : BaseModel
{
    [Column("created_at")]
    public string? CreatedAt { get; set; }

    [Column("duration_ms")]
    public int DurationMs { get; set; }

    [Column("error_code")]
    public string? ErrorCode { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("generated_recipe_id")]
    public string? GeneratedRecipeId { get; set; }

    [Column("id")]
    public string? Id { get; set; }

    [Column("model")]
    public string Model { get; set; }

    [Column("reject_reason_id")]
    public short? RejectReasonId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }
}

[Table("recipes_generations")]
public class RecipesGenerationsUpdate : BaseModel
{
    [Column("created_at")]
    public string? CreatedAt { get; set; }

    [Column("duration_ms")]
    public int? DurationMs { get; set; }

    [Column("error_code")]
    public string? ErrorCode { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("generated_recipe_id")]
    public string? GeneratedRecipeId { get; set; }

    [Column("id")]
    public string? Id { get; set; }

    [Column("model")]
    public string? Model { get; set; }

    [Column("reject_reason_id")]
    public short? RejectReasonId { get; set; }

    [Column("user_id")]
    public string? UserId { get; set; }
}

[Table("user_preferences")]
public class UserPreferencesSelect : BaseModel
{
    [Column("created_at")]
    public string CreatedAt { get; set; }

    [Column("diet_type_id")]
    public short DietTypeId { get; set; }

    [Column("disliked_ingredients")]
    public string? DislikedIngredients { get; set; }

    [Column("preferred_cuisine_id")]
    public short PreferredCuisineId { get; set; }

    [Column("updated_at")]
    public string UpdatedAt { get; set; }

    [PrimaryKey("user_id")]
    public string UserId { get; set; }
}

[Table("user_preferences")]
public class UserPreferencesInsert : BaseModel
{
    [Column("created_at")]
    public string? CreatedAt { get; set; }

    [Column("diet_type_id")]
    public short DietTypeId { get; set; }

    [Column("disliked_ingredients")]
    public string? DislikedIngredients { get; set; }

    [Column("preferred_cuisine_id")]
    public short PreferredCuisineId { get; set; }

    [Column("updated_at")]
    public string? UpdatedAt { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }
}

[Table("user_preferences")]
public class UserPreferencesUpdate : BaseModel
{
    [Column("created_at")]
    public string? CreatedAt { get; set; }

    [Column("diet_type_id")]
    public short? DietTypeId { get; set; }

    [Column("disliked_ingredients")]
    public string? DislikedIngredients { get; set; }

    [Column("preferred_cuisine_id")]
    public short? PreferredCuisineId { get; set; }

    [Column("updated_at")]
    public string? UpdatedAt { get; set; }

    [Column("user_id")]
    public string? UserId { get; set; }
}

