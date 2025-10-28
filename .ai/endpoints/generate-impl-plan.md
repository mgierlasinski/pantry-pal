# API Endpoint Implementation Plan: POST /recipes/generate

## 1. Endpoint Overview
This endpoint generates an AI-powered recipe based on the authenticated user's pantry items and dietary preferences.

## 2. Request Details
- HTTP Method: POST
- URL Structure: `/recipes/generate`
- Headers:
  - Required: `Authorization: Bearer <token>`
  - Implicit: `Content-Type: application/json` for responses
- Request Body: None

## 3. Used Types
- Response DTO: `RecipeGenerateResponseDto` (contains `GenerationId`, `RecipeText`)
- Internal Command (optional): `GenerateRecipeCommand` with `UserId` property

## 4. Response Details
- Success (200 OK):
  ```json
  {
    "generationId": "<guid>",
    "recipeText": "# Recipe in Markdown..."
  }
  ```
- Errors:
  - 400 Bad Request: missing preferences or pantry data
  - 401 Unauthorized: authentication failure
  - 500 Internal Server Error: AI or database failures

## 5. Data Flow
1. **Authentication**: Validate JWT/Supabase session and extract `userId`.
2. **Validation**:
   - Retrieve user preferences; if not found, return 400.
   - Retrieve pantry items; if empty, return 400 or allow AI prompt with no ingredients.
3. **Start Generation Record**:
   - Insert into `recipes_generations` with `user_id`, `model`, `created_at`.
4. **AI Invocation**:
   - Build prompt including pantry item names and preference settings.
   - Call `IAIRecipeGeneratorService.GenerateAsync(prompt)` and measure duration.
5. **Persist Results**:
   - On success:
     - Insert new `recipes` record with `user_id` and `recipe_text`, capture `recipeId`.
     - Update `recipes_generations` record with `duration_ms` and `generated_recipe_id`.
     - Return `RecipeGenerateResponseDto` with `generationId` and `recipeText`.
   - On failure:
     - Update `recipes_generations` with `error_code` and `error_message`.
     - Return 500 with generic error message.

## 6. Security Considerations
- **Authentication**: Secure with JWT or Supabase session middleware.
- **Authorization**: Ensure operations limited to the authenticated `userId`.
- **Input Sanitization**: Escape any user-provided strings inserted into AI prompt.
- **Rate Limiting**: Apply per-user rate limit to prevent abuse.
- **Data Privacy**: Only include pantry items and preferences for the calling user.

## 7. Error Handling
| Scenario                                   | Action                                                                          |
|--------------------------------------------|---------------------------------------------------------------------------------|
| Missing or invalid token                   | Return 401 Unauthorized                                                         |
| No user preferences found                  | Return 400 Bad Request with message “User preferences not set.”                 |
| Empty pantry (business rule)               | Return 400 Bad Request with message “Pantry is empty.” (or allow default flow)  |
| AI service exception                       | Update generation record; return 500 Internal Server Error                     |
| Database insertion/update failure          | Log error; return 500 Internal Server Error                                      |

## 8. Performance Considerations
- Use asynchronous calls for database and AI service operations.
- Batch fetch pantry and preferences in a single repository call if possible.
- Indexes on `user_id` and `created_at` support efficient lookups and inserts.

## 9. Implementation Steps
1. **Define Service Interface**: Add `Task<RecipeGenerateResponseDto> GenerateRecipeAsync(Guid userId)` to `IRecipeService`.
2. **Implement Service**: In `RecipeService`, implement `GenerateRecipeAsync`:
   - Inject `IPantryRepository`, `IUserPreferencesRepository`, `IRecipeRepository`, `IRecipesGenerationsRepository`, and `IAIRecipeGeneratorService`.
   - Follow data flow: validate, start generation, call AI, persist results.
3. **Register Repositories and AI Service** in DI container with scoped/singleton lifetimes.
4. **Add Minimal API Endpoint**: In `Program.cs`, map POST `/recipes/generate`:
   - Use `[Authorize]` or `RequireAuthorization()`.
   - Extract `userId` from claims.
   - Call `IRecipeService.GenerateRecipeAsync(userId)` and return result.
5. **Add FluentValidation** (if needed): create `RecipeGenerateRequestValidator` (empty body case) or input-less guard.
6. **Write Unit Tests**:
   - Mock repositories and AI service to test success, missing preferences, AI failure scenarios.
7. **Integration Testing**:
   - Use `PantryPal.Api.http` to simulate real requests with valid token.
8. **Logging**:
   - Ensure any exceptions are logged via ASP.NET logger and persisted in `recipes_generations`.
9. **Documentation**:
   - Update `PantryPal.Api.http` with example POST call.
