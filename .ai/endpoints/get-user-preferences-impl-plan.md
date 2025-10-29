# API Endpoint Implementation Plan: GET /user-preferences

## 1. Endpoint Overview
This endpoint retrieves the authenticated user's preferences, including diet type, preferred cuisine, and disliked ingredients. It enables personalization of recipe suggestions in the PantryPal application by fetching data from the user_preferences table, optionally enriched with dictionary names from diet_types and preferred_cuisines. The endpoint assumes the user is authenticated via Supabase JWT and returns user-specific data only.

## 2. Request Details
- HTTP Method: GET
- URL Structure: /user-preferences
- Parameters:
  - Required: None
  - Optional: None
- Request Body: None (as this is a GET request)

## 3. Used Types
- **UserPreferencesDto** (existing in PantryPal.Data/ApiModels.cs): Response DTO with UserId, DietTypeId, PreferredCuisineId, DislikedIngredients, CreatedAt, UpdatedAt. Enhance in service to include DietType.Name and PreferredCuisine.Name by mapping from joins.
- **DietTypeDto** and **PreferredCuisineDto** (existing): Used internally in the service/repository for joining and mapping names to IDs.
- No new DTOs required, but consider extending UserPreferencesDto if needed for names (or create UserPreferencesResponseDto for clarity).
- No Command Models needed.

## 4. Response Details
- **Success (200 OK)**: Returns a single UserPreferencesDto object (JSON). If preferences do not exist, return 404 or an empty/null object (recommend 404 for strictness).
  Example response:
  ```json
  {
    "userId": "uuid-string",
    "dietTypeId": 1,
    "preferredCuisineId": 1,
    "dislikedIngredients": "nuts, shellfish",
    "createdAt": "2025-10-29T10:00:00Z",
    "updatedAt": "2025-10-29T10:00:00Z"
  }
  ```
  (Note: Service should resolve and include "dietTypeName": "vegetarian", "preferredCuisineName": "Italian" for usability.)
- **Error Responses**:
  - 401 Unauthorized: { "error": "Authentication required" }
  - 404 Not Found: { "error": "User preferences not found" }
  - 500 Internal Server Error: { "error": "Internal server error" } (avoid exposing details)

## 5. Data Flow
1. Incoming request hits the minimal API endpoint in Program.cs, which extracts the authenticated user_id from Supabase JWT context.
2. Endpoint calls IUserPreferencesService.GetUserPreferencesAsync(user_id).
3. Service injects IUserPreferencesRepository and calls GetByUserIdAsync(user_id), which executes a Supabase query:
   - SELECT from user_preferences JOIN diet_types ON diet_type_id = diet_types.id JOIN preferred_cuisines ON preferred_cuisine_id = preferred_cuisines.id WHERE user_id = @user_id.
   - Maps result to UserPreferencesDto, enriching with names.
4. If no record found, service returns null/throws NotFoundException.
5. Service handles any DB exceptions, logs via ILogger, and propagates to endpoint.
6. Endpoint maps to HTTP response.
No external services (e.g., AI) involved; purely DB read via Supabase client.

## 6. Security Considerations
- **Authentication**: Require Supabase JWT; validate in API middleware (e.g., using Supabase's AuthHelper to get current user_id).
- **Authorization**: Strictly filter by authenticated user_id in repository query to prevent data leakage. Use parameterized queries to avoid injection.
- **Data Validation**: No input, but validate user_id is a valid UUID in service guard clause.
- **Supabase Integration**: Leverage Supabase client for secure DB access; enable RLS on user_preferences table for defense-in-depth (policy: user_id = auth.uid()).
- **Threats Mitigated**: No PII exposure beyond user-specific prefs; rate-limit endpoint (e.g., via middleware) to ~10 req/min per user. Use HTTPS for all traffic.

## 7. Performance Considerations
- **Query Optimization**: Use existing indexes on user_id (primary key). Joins to dictionary tables are low-cost (small static tables). Limit to single row fetch.
- **Potential Bottlenecks**: Supabase latency on cold starts; cache user preferences in-memory (e.g., Redis) if frequently accessed, but unnecessary for MVP (user prefs change infrequently).
- **Scalability**: Paginated? No, single record. Monitor query performance via Supabase dashboard; aim for <50ms response.
- **Best Practices**: Use async/await throughout; avoid N+1 queries (single join suffices).

## 8. Implementation Steps
1. Create IUserPreferencesService.cs and UserPreferencesService.cs in /src/PantryPal.Api/Services:
   - Interface: Task<UserPreferencesDto?> GetUserPreferencesAsync(string userId);
   - Implementation: Inject ILogger and IUserPreferencesRepository; use guard clauses for null userId; call repository, map to DTO with name resolution; log errors; early return on exceptions.
2. Implement UserPreferencesRepository.cs (if not fully implemented) in /src/PantryPal.Api/Repositories:
   - Inject SupabaseClient; implement GetByUserIdAsync with RPC or direct query using From<user_preferences>().Join... .Where(x => x.UserId == userId).SingleOrDefaultAsync(); map to DTO.
3. Add endpoint to Program.cs minimal API:
   - MapGet("/user-preferences", async (IUserPreferencesService service, ClaimsPrincipal user) => { var userId = GetUserIdFromClaims(user); var prefs = await service.GetUserPreferencesAsync(userId); return prefs ?? Results.NotFound(); });
   - Integrate auth middleware to ensure user is authenticated.
4. Add to PantryPal.Api.http for testing: GET {{host}}/user-preferences with auth header.
5. Update ApiModels.cs if extending DTO for names (e.g., add DietTypeName, PreferredCuisineName properties).
6. Write unit tests in /tests/PantryPal.Api.UnitTests/Services: Mock repository/service, test success, not found, invalid userId scenarios.
7. Handle errors: In service, catch SupabaseException, log with ILogger.LogError, rethrow as custom ApiException for consistent 500 responses.
8. Verify: Run endpoint tests; ensure RLS compatibility if enabled; profile query performance.
