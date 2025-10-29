# API Endpoint Implementation Plan: DELETE /recipes/{id}

## 1. Endpoint Overview
This endpoint allows authenticated users to delete a saved recipe from their personal collection. It targets the 'recipes' table in Supabase, ensuring the recipe belongs to the requesting user before performing the deletion. Upon success, it returns 204 No Content to indicate the operation completed without issues. The implementation follows ASP.NET minimal API patterns, leverages dependency injection for services and repositories, and integrates with Supabase for secure database operations. No response body is returned, aligning with REST best practices for DELETE operations.

## 2. Request Details
- HTTP Method: DELETE
- URL Structure: /recipes/{id}
- Parameters:
  - Required: id (string, UUID format) - The unique identifier of the recipe to delete.
  - Optional: None
- Request Body: None (DELETE operations typically do not include a body)

## 3. Used Types
- Existing DTOs from PantryPal.Data/ApiModels.cs:
  - RecipeDto: Used internally in the service to fetch and verify the recipe details (e.g., to check user_id ownership) before deletion.
- No new DTOs or Command Models required, as there is no request body and the response is status-only.
- Internal types:
  - Leverage DatabaseTypes.cs for recipe entity mapping (e.g., RecipesSelect for querying).

## 4. Response Details
- Success: 204 No Content (no body returned).
- Error Status Codes:
  - 400 Bad Request: Invalid UUID format for the 'id' parameter.
  - 401 Unauthorized: Missing or invalid authentication token.
  - 404 Not Found: Recipe does not exist or does not belong to the authenticated user.
  - 500 Internal Server Error: Server-side issues, such as database connectivity failures or unexpected exceptions.

## 5. Data Flow
1. The request hits the minimal API endpoint in Program.cs or a dedicated routes file.
2. Extract the 'id' path parameter and validate its UUID format (early guard clause).
3. Authenticate the user via Supabase's JWT middleware (inject SupabaseClient and extract user_id from claims).
4. Inject and call IRecipeService.DeleteRecipeAsync(id, userId):
   - Service uses IRecipeRepository to query the recipe by id (using RecipesSelect or equivalent Supabase query).
   - Verify the recipe's user_id matches the authenticated user_id.
   - If valid, perform the delete operation via Supabase client (e.g., client.From&lt;Recipe&gt;().Where(x =&gt; x.Id == id).Delete()).
   - No cascading deletes needed beyond Supabase's ON DELETE CASCADE for related recipes_generations.generated_recipe_id.
5. If successful, return 204; otherwise, propagate errors with appropriate status codes.
6. Logging: Use ILogger&lt;RecipeService&gt; to log delete attempts, successes, and failures (e.g., &quot;Recipe {id} deleted by user {userId}&quot; or error details).

## 6. Security Considerations
- **Authentication**: Require a valid Supabase JWT token. Use middleware or endpoint filter to extract and validate the user_id from the token claims. Reject unauthenticated requests with 401.
- **Authorization**: In the service layer, enforce ownership by comparing the recipe's user_id (from DB query) with the authenticated user_id. Return 404 for unauthorized access to avoid leaking existence info.
- **Input Sanitization**: Validate 'id' as a valid UUID using Guid.TryParse or regex to prevent injection attacks. Supabase's parameterized queries inherently protect against SQL injection.
- **Row-Level Security (RLS)**: If enabled on the Supabase 'recipes' table, configure policies to allow deletes only for matching user_id (e.g., CREATE POLICY &quot;Users can delete own recipes&quot; ON recipes FOR DELETE USING (auth.uid() = user_id);). This acts as a secondary safeguard.
- **Rate Limiting**: Implement API-level rate limiting (e.g., via middleware or Supabase edge functions) to prevent abuse, such as bulk deletes.
- **Data Privacy**: Avoid logging sensitive recipe_text content; log only IDs and high-level events.

## 7. Performance Considerations
- **Query Efficiency**: Use indexed queries on recipes.id (primary key) and user_id for fast lookups. The B-Tree index on (user_id, created_at) supports efficient filtering, but for single-ID deletes, the PK index suffices.
- **Potential Bottlenecks**: Supabase query latency for cross-region access; mitigate by ensuring the API and DB are in the same region. Deletes are lightweight (O(1) operation), but if cascades trigger (e.g., to recipes_generations), monitor for any performance impact on large datasets.
- **Optimization Strategies**: 
  - Avoid fetching full recipe_text unless necessary (just select id and user_id for verification).
  - Use async/await throughout to handle I/O-bound DB calls without blocking.
  - Cache user preferences or dict data if expanded later, but not needed for this endpoint.
  - Test with Supabase's query performance analyzer for any slow queries during deletes.

## 8. Implementation Steps
1. **Update IRecipeService Interface**: Add a method `Task DeleteRecipeAsync(string recipeId, string userId);` in /src/PantryPal.Api/Services/IRecipeService.cs. Include XML docs for the method.

2. **Implement Service Logic**: In /src/PantryPal.Api/Services/RecipeService.cs:
   - Inject ILogger&lt;RecipeService&gt; and IRecipeRepository.
   - Implement DeleteRecipeAsync: Use guard clauses to check if recipeId is valid UUID; query repository for recipe (select id, user_id); verify ownership; if valid, call repository.DeleteAsync(recipeId); log success. Throw custom exceptions (e.g., NotFoundException, UnauthorizedException) for errors.
   - Follow clean code rules: Early returns for errors, no deep nesting.

3. **Update Repository**: In /src/PantryPal.Api/Repositories/IRecipeRepository.cs and RecipeRepository.cs:
   - Add `Task&lt;RecipeDto?&gt; GetByIdAsync(string id);` for fetching (reuse or implement if missing).
   - Add `Task DeleteAsync(string id);` using Supabase client: `await _client.From&lt;Recipe&gt;().Where(x =&gt; x.Id == id).DeleteAsync();`.
   - Ensure queries filter by user_id where possible for security.

4. **Add Endpoint in Minimal API**: In Program.cs or a routes file (e.g., /src/PantryPal.Api/routes/recipes.cs):
   - Map DELETE(&quot;/recipes/{id}&quot;, async (string id, string userId, IRecipeService service) =&gt; { ... });
   - Extract userId from HttpContext (Supabase auth); validate id; call service.DeleteRecipeAsync(id, userId); return Results.NoContent();
   - Add input validation: If (!Guid.TryParse(id, out _)) return Results.BadRequest(&quot;Invalid recipe ID format.&quot;);

5. **Handle Exceptions Globally**: In Program.cs, add exception middleware or use Results to catch and map exceptions: NotFoundException -&gt; 404, UnauthorizedException -&gt; 401, general -&gt; 500. Log all exceptions via ILogger.

6. **Validation Setup**: If using FluentValidation, create a validator for path params (though minimal for single string). Integrate via endpoint filter if needed.

7. **Testing Integration**: Update /tests/PantryPal.Api.UnitTests/Services/RecipeServiceTests.cs with unit tests for DeleteRecipeAsync (mock repository, cover success, not found, unauthorized cases). Add integration tests using .http file in PantryPal.Api.http for end-to-end delete flow.

8. **Documentation and RLS Check**: Update API docs (e.g., in .http file or Swagger if added). Verify Supabase RLS policies for recipes table to enforce user_id matching on deletes. Run migration if schema changes needed (none anticipated).

9. **Review and Clean Code**: Ensure all code follows C# conventions (PascalCase, async patterns), error handling (early guards), and backend rules (DI, minimal API). Test for edge cases like non-existent ID or concurrent deletes.
