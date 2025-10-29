# API Endpoint Implementation Plan: POST /recipes/{generationId}/reject

## 1. Endpoint Overview
This endpoint allows authenticated users to reject a previously generated recipe by providing a reason for the rejection. It updates the `recipes_generations` table with the specified `reject_reason_id`, marking the generation as rejected. The endpoint ensures the generation belongs to the current user and that the reject reason is valid. Upon successful rejection, it returns a 204 No Content response, indicating the operation completed without returning any data.

## 2. Request Details
- HTTP Method: POST
- URL Structure: `/recipes/{generationId}/reject`
- Parameters:
  - Required: 
    - `generationId` (path parameter): A UUID string identifying the recipe generation to reject.
  - Optional: None
- Request Body: 
  ```json
  {
    "rejectReasonId": 1
  }
  ```
  The body contains a single property `rejectReasonId` (short integer) referencing an ID from the `recipe_reject_reasons` table.

## 3. Used Types
- **Request DTO**: `RecipeRejectRequestDto` (from `PantryPal.Data/ApiModels.cs`)
  ```csharp
  public record RecipeRejectRequestDto(short RejectReasonId);
  ```
- **Validator**: New `RecipeRejectRequestDtoValidator` (FluentValidation) to validate `RejectReasonId` exists in `recipe_reject_reasons`.
- **No Response DTO**: Since the response is 204 No Content, no DTO is needed.
- **Internal Types**: 
  - `RecipesGenerationsSelect` or similar from `Db/DatabaseTypes.cs` for querying the generation.
  - Repository: `IRecipesGenerationsRepository` for data access.

## 4. Response Details
- Success: 204 No Content (no body returned).
- Error Responses:
  - 400 Bad Request: Invalid input (e.g., invalid `rejectReasonId` or `generationId` format).
  - 401 Unauthorized: User not authenticated.
  - 404 Not Found: Generation not found or does not belong to the user.
  - 409 Conflict: Generation already rejected or accepted.
  - 500 Internal Server Error: Unexpected server-side issues.

## 5. Data Flow
1. Authenticate the user via Supabase (extract `user_id` from JWT).
2. Validate the request body using `RecipeRejectRequestDtoValidator`.
3. Call `IRecipeService.RejectRecipeGenerationAsync(generationId, request.RejectReasonId)`:
   - Use `IRecipesGenerationsRepository.GetByIdAsync(generationId)` to fetch the generation.
   - Verify it belongs to the current user and is not already rejected/accepted (check `reject_reason_id` is null and `generated_recipe_id` may or may not be set, but focus on rejection status).
   - Use repository to update `reject_reason_id` in `recipes_generations`.
4. If the generation has a `generated_recipe_id`, optionally handle the linked recipe (but per spec, just reject the generation).
5. Return 204 on success.

Interactions:
- Supabase client for authentication and database operations (via repository).
- No external AI calls (Openrouter.ai) needed for this endpoint.

## 6. Security Considerations
- **Authentication**: Require JWT token from Supabase; extract `user_id` using Supabase's auth context.
- **Authorization**: Ensure the `generationId` record in `recipes_generations` has `user_id` matching the authenticated user to prevent cross-user access.
- **Input Validation**: Use FluentValidation to check `rejectReasonId` exists in `recipe_reject_reasons` (query dictionary table). Validate `generationId` as valid UUID.
- **Data Exposure**: No sensitive data returned (204 response).
- **Potential Threats**:
  - Injection: Sanitized via Supabase parameterized queries.
  - Unauthorized Access: Enforced via user_id check.
  - Rate Limiting: Consider adding to prevent abuse (e.g., multiple rejections), though not specified.
  - SQL Injection: Mitigated by using Supabase SDK with prepared statements.

## 7. Error Handling
- **Validation Errors (400)**: Invalid `rejectReasonId` (not exists) or malformed `generationId`. Return problem details with FluentValidation errors.
- **Unauthorized (401)**: Missing or invalid JWT.
- **Not Found (404)**: Generation does not exist or user mismatch. Use early return in service.
- **Conflict (409)**: If `reject_reason_id` already set (already rejected). Check in service before update.
- **Server Errors (500)**: Database update failures (e.g., concurrency issues). Log exceptions using ILogger; do not expose details to client.
- **Logging**: Use `ILogger` in service for all errors. Do not log to `recipes_generations.error_code` as this is for generation errors, not user rejections.
- Follow clean code: Guard clauses for validations, early returns for errors.

## 8. Performance Considerations
- **Bottlenecks**: Database queries (get by ID, validate reason, update). Use indexes on `recipes_generations.id` and `user_id`.
- **Optimizations**:
  - Single transaction for get/update to ensure consistency.
  - Cache dictionary tables (e.g., reject reasons) if frequently validated, but since small, query is fine.
  - Pagination/indexes already in place for generations.
- **Scalability**: Supabase handles scaling; minimal API keeps overhead low.
- **Monitoring**: Track update durations; aim for <100ms.

## 9. Implementation Steps
1. **Add Validator**: Create `RecipeRejectRequestDtoValidator.cs` in `src/PantryPal.Api/Validators/` using FluentValidation. Rule: `RejectReasonId` must be >0 and exist in `recipe_reject_reasons` (inject repository or query Supabase).
2. **Extend Service**: In `IRecipeService` and `RecipeService.cs` (`src/PantryPal.Api/Services/`), add `Task RejectRecipeGenerationAsync(string generationId, short rejectReasonId, string userId)`. Implement with guards: validate generation ownership, check not already rejected, update via repository.
3. **Extend Repository**: In `IRecipesGenerationsRepository` and `RecipesGenerationsRepository.cs` (`src/PantryPal.Api/Repositories/`), add `UpdateRejectReasonAsync(string id, short rejectReasonId)`.
4. **Add Endpoint**: In `Program.cs` (`src/PantryPal.Api/`), add minimal API route: `app.MapPost("/recipes/{generationId}/reject", async (string generationId, RecipeRejectRequestDto request, IRecipeService service, SupabaseClient client) => { ... })`. Handle auth, validation, call service, return Results.
5. **Update .http File**: Add request example to `PantryPal.Api.http` for testing.
6. **Add Unit Tests**: In `tests/PantryPal.Api.UnitTests/Services/`, add tests for `RecipeService.RejectRecipeGenerationAsync` covering success, invalid reason, not found, already rejected.
7. **Database**: Ensure `reject_reason_id` index if needed; no migration required as column exists.
8. **Review**: Ensure early returns, no nested ifs, proper logging.
