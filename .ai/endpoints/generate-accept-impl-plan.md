# API Endpoint Implementation Plan: POST /recipes/{generationId}/accept

## 1. Endpoint Overview
Accepts a previously generated AI recipe and persists it in the `recipes` table, linking the new recipe record back to its originating generation entry.

## 2. Request Details
- HTTP Method: POST
- URL Structure: `/recipes/{generationId}/accept`
- Path Parameters:
  - `generationId` (GUID, required): Identifier of the recipe generation to accept.
- Request Body: None

## 3. Used Types
- **RecipeAcceptResponseDto** (defined in `PantryPal.Data.ApiModels`):
  - `string RecipeId`
  - `string SavedAt`
- **Domain / Repository Models:**
  - `RecipesGenerationsSelect` (from `recipes_generations` table)
  - `RecipesInsert` (insert command for `recipes` table)

## 4. Response Details
- **201 Created**
  - Content-Type: `application/json`
  - Body: `RecipeAcceptResponseDto`
- **400 Bad Request**
  - Invalid or missing recipe text in generation record.
- **401 Unauthorized**
  - Missing or invalid authentication token.
- **404 Not Found**
  - No generation record found for the provided `generationId` belonging to the user.
- **409 Conflict**
  - Generation already accepted (duplicate accept attempt).
- **500 Internal Server Error**
  - Unexpected errors during processing.

## 5. Data Flow
1. **Authenticate & Authorize**
   - Extract `userId` from JWT claims.
2. **Retrieve Generation Record**
   - `var generation = await recipesGenerationsRepo.GetByIdAsync(generationId, userId);`
3. **Validate State**
   - If `generation` is null → throw `NotFoundException`.
   - If `generation.GeneratedRecipeId` is not null → throw `InvalidOperationException("Already accepted")`.
   - If `generation.GeneratedRecipeText` is null or empty → throw `InvalidOperationException("No recipe text available")`.
4. **Persist Recipe**
   - Create new record via `recipeRepo.CreateAsync(new RecipesInsert { UserId = userId, RecipeText = generation.GeneratedRecipeText })`.
5. **Link Generation to Recipe**
   - Update generation: `recipesGenerationsRepo.MarkAsAcceptedAsync(generationId, newRecipeId)`.
6. **Commit Transaction**
   - Wrap steps 4–5 in a database transaction for atomicity.
7. **Return Response**
   - Map to `RecipeAcceptResponseDto` and return with 201 status.

## 6. Security Considerations
- **Authentication & Authorization**: Ensure only the owner (`userId`) can accept their own generation.
- **Input Validation**: Framework binding ensures valid GUID; guard clauses enforce state.
- **SQL Injection**: Use parameterized queries via repository layer (Supabase client).
- **Concurrency**: Prevent duplicate accepts by checking `GeneratedRecipeId` and using database transaction/row locking if supported.

## 7. Error Handling
| Scenario                                   | Exception                                 | HTTP Status | Action                                         |
|--------------------------------------------|-------------------------------------------|-------------|------------------------------------------------|
| Generation not found                       | `NotFoundException`                       | 404         | Log warning; return NotFound("Generation not found") |
| Already accepted                           | `InvalidOperationException("Already accepted")` | 409         | Log warning; return Conflict("Already accepted")    |
| Missing recipe text                        | `InvalidOperationException("No recipe text available")` | 400         | Log warning; return BadRequest("No recipe text available") |
| Unauthorized (missing/invalid JWT)         | Framework exception                       | 401         | Return Unauthorized                            |
| Database/transaction failure               | General `Exception`                       | 500         | Log error; return Problem(500)                 |

## 8. Performance Considerations
- **Indices**: Rely on `idx_gen_user_created` and primary keys for efficient lookups.
- **Transaction Scope**: Keep transaction narrowly scoped to insert and update.
- **Payload Size**: Return only IDs and timestamps to minimize payload.

## 9. Implementation Steps
1. **Define Service Method**
   - Extend `IRecipeService` with `Task<RecipeAcceptResponseDto> AcceptGeneratedRecipeAsync(Guid generationId, Guid userId)`.
2. **Implement Business Logic**
   - In `RecipeService.cs`, inject `IRecipesGenerationsRepository` and `IRecipeRepository`, implement method per data flow above, using a transaction.
3. **Repository Enhancements**
   - Add `GetByIdAsync(Guid id, Guid userId)` and `MarkAsAcceptedAsync(Guid id, Guid recipeId)` to `IRecipesGenerationsRepository`.
   - Ensure `CreateAsync(RecipesInsert insert)` exists on `IRecipeRepository` and returns inserted record with timestamp.
4. **Map Endpoint**
   - In `Program.cs`, add:
     ```csharp
     app.MapPost("/recipes/{generationId}/accept", async (Guid generationId, IRecipeService recipeService, ILogger<Program> logger) => { /* handle logic with try/catch */ })
         .RequireAuthorization();
     ```
5. **Error Mapping & Logging**
   - Use guard clauses and catch specific exceptions to return appropriate Results.* methods.
6. **Unit Tests**
   - Create tests in `tests/PantryPal.Api.UnitTests/Services/RecipeServiceTests.cs` covering all scenarios.
7. **API Spec Update**
   - Add entry to `PantryPal.Api.http` for automated runtime testing.
