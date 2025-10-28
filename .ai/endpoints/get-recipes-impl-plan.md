# API Endpoint Implementation Plan: GET /recipes

## 1. Endpoint Overview
A secure, paginated, and sortable endpoint to list saved recipes for the authenticated user.

## 2. Request Details
- HTTP Method: GET
- URL Structure: `/recipes`
- Query Parameters:
  - Optional:
    - `page` (int): Page number, default = 1. Must be > 0.
    - `pageSize` (int): Items per page, default = 20, max = 100.
    - `sort` (string): Sort field, only `created_at` supported, default = `created_at`.
- Request Body: None

## 3. Used Types
- `RecipeDto` (PantryPal.Data.ApiModels)
- `RecipesPaginatedResponseDto` (PantryPal.Data.ApiModels)

## 4. Response Details
- Status Codes:
  - 200 OK: Successful retrieval.
  - 400 Bad Request: Invalid query parameters.
  - 401 Unauthorized: Missing/invalid authentication.
  - 500 Internal Server Error: Unhandled exceptions.
- Response Body (200):
  ```json
  {
    "items": [
      { "id": "guid", "recipeText": "...", "createdAt": "...", "updatedAt": "..." },
      // ...
    ],
    "page": 1,
    "pageSize": 20,
    "total": 42
  }
  ```

## 5. Data Flow
1. Client sends GET `/recipes?page=&pageSize=&sort=` with bearer token.
2. Minimal API endpoint in `Program.cs` binds and validates parameters.
3. Extract `userId` from JWT claims.
4. Call `IRecipeService.GetRecipesAsync(userId, page, pageSize, sort)`.
5. Service calls `IRecipeRepository.GetRecipesAsync(userId, page, pageSize, sort)`.
6. Repository queries Supabase using parameterized filters and sorting:
   ```sql
   SELECT id, recipe_text, created_at, updated_at
   FROM recipes
   WHERE user_id = @userId
   ORDER BY created_at DESC
   LIMIT @pageSize OFFSET (@page-1)*@pageSize;
   ```
7. Repository also retrieves total count for pagination metadata.
8. Service maps database results to `RecipeDto` and assembles `RecipesPaginatedResponseDto`.
9. Endpoint returns 200 with DTO.

## 6. Security Considerations
- Authentication: Require JWT bearer authentication on endpoint.
- Authorization: Scope to authenticated user; always filter by `userId` from token.
- Input Validation: Guard against invalid `page`, `pageSize`, and `sort` values.
- SQL Injection: Use parameterized queries via Supabase client.

## 7. Error Handling
- Input Validation Errors: Return 400 with detailed messages.
- Authentication Failures: Handled by middleware, return 401.
- Repository/Service Exceptions:
  - Catch in endpoint handler.
  - Log via `ILogger<Program>`.
  - Return 500 Internal Server Error with generic message.

## 8. Performance Considerations
- Ensure B-Tree index `idx_recipes_user_created` on `(user_id, created_at)` is used for pagination.
- Enforce reasonable `pageSize` max (e.g., 100).
- Consider caching count queries or using estimated counts for large datasets.

## 9. Implementation Steps
1. Define `IRecipeRepository` in `src/PantryPal.Api/Repositories`:
   ```csharp
   Task<(IEnumerable<RecipeEntity> Items, int Total)> GetRecipesAsync(Guid userId, int page, int pageSize);
   ```
2. Implement `RecipeRepository` to query Supabase:
   - Use Supabase client to run parameterized SQL.
   - Return both items and total count.
3. Define `IRecipeService` in `src/PantryPal.Api/Services`:
   ```csharp
   Task<RecipesPaginatedResponseDto> GetRecipesAsync(Guid userId, int page, int pageSize);
   ```
4. Implement `RecipeService`:
   - Call repository.
   - Map `RecipeEntity` to `RecipeDto`.
5. Register in DI in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
   builder.Services.AddScoped<IRecipeService, RecipeService>();
   ```
6. Add minimal API endpoint in `Program.cs`:
   ```csharp
   app.MapGet("/recipes", async (int? page, int? pageSize, string? sort, IRecipeService svc, HttpContext ctx, ILogger<Program> log) =>
   {
     // Validate inputs
     // Extract userId
     // Call service
     // Return Results.Ok(response);
   })
   .RequireAuthorization();
   ```
7. Implement validation guard clauses for `page`, `pageSize`, and `sort` at the top of endpoint.
8. Add try/catch around service call to log errors and return 500.
9. Document example requests/responses in `PantryPal.Api.http`.
