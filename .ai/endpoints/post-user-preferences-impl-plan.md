# API Endpoint Implementation Plan: POST /user-preferences

## 1. Endpoint Overview
The POST /user-preferences endpoint is designed to create or update a user's dietary preferences in the PantryPal application. It allows users to specify their diet type, preferred cuisine, and any disliked ingredients. This endpoint supports upsert functionality, meaning it will insert a new record if none exists for the user or update the existing one. The endpoint leverages Supabase for database operations and follows minimal API patterns in ASP.NET. Authentication is required via Supabase's auth system to identify the current user.

## 2. Request Details
- HTTP Method: POST
- URL Structure: /user-preferences
- Parameters:
  - Required: None (all in body)
  - Optional: None
- Request Body: JSON object conforming to UserPreferencesCreateDto
  ```json
  {
    \"dietTypeId\": 2,
    \"preferredCuisineId\": 3,
    \"dislikedIngredients\": \"nuts, shellfish\"
  }
  ```
  - dietTypeId: short (integer), required, must reference a valid id in diet_types table
  - preferredCuisineId: short (integer), required, must reference a valid id in preferred_cuisines table
  - dislikedIngredients: string, optional, max length 1000 characters

## 3. Used Types
- DTOs and Command Models:
  - UserPreferencesCreateDto (from PantryPal.Data/ApiModels.cs): 
    ```csharp
    public record UserPreferencesCreateDto(
        short DietTypeId,
        short PreferredCuisineId,
        string? DislikedIngredients
    );
    ```
  - UserPreferencesDto (response, from ApiModels.cs): Includes resolved names for diet type and cuisine.
  - Existing: IUserPreferencesService and UserPreferencesService for business logic.
  - Existing: IUserPreferencesRepository and UserPreferencesRepository for data access.
  - Validation: Use FluentValidation with UserPreferencesCreateDtoValidator (create if not exists).

## 4. Response Details
- Success: 200 OK
  - Body: UserPreferencesDto representing the created/updated preferences
    ```json
    {
      \"userId\": \"uuid\",
      \"dietTypeId\": 2,
      \"dietTypeName\": \"vegetarian\",
      \"preferredCuisineId\": 3,
      \"preferredCuisineName\": \"Italian\",
      \"dislikedIngredients\": \"nuts, shellfish\",
      \"createdAt\": \"2025-10-29T10:00:00Z\",
      \"updatedAt\": \"2025-10-29T10:00:00Z\"
    }
    ```
- Errors:
  - 400 Bad Request: Invalid input (validation failures, e.g., invalid ids, too long dislikedIngredients)
  - 401 Unauthorized: Missing or invalid auth token
  - 404 Not Found: If dietTypeId or preferredCuisineId do not exist (handled in validation)
  - 409 Conflict: If unique constraint violation (unlikely due to upsert)
  - 500 Internal Server Error: Database errors or unexpected issues

## 5. Data Flow
1. Client sends POST request with JSON body to /user-preferences, including auth token.
2. API extracts user_id from Supabase auth context (via dependency injection).
3. Validate request body using FluentValidation (UserPreferencesCreateDtoValidator).
4. If valid, call IUserPreferencesService.UpsertPreferencesAsync(UserPreferencesCreateDto, user_id).
5. Service uses IUserPreferencesRepository to:
   - Check if dietTypeId and preferredCuisineId exist (query diet_types and preferred_cuisines).
   - Perform upsert on user_preferences table using Supabase client (INSERT ... ON CONFLICT (user_id) DO UPDATE).
6. Repository returns the updated UserPreferencesSelect entity.
7. Service maps to UserPreferencesDto, resolving DietTypeName and PreferredCuisineName via joins or separate queries.
8. Return 200 with UserPreferencesDto.
9. If error, log to console or appropriate logger and return error response.

No external services beyond Supabase. AI integration not involved here.

## 6. Security Considerations
- Authentication: Require Supabase JWT token; extract user_id from claims. Use [Authorize] or middleware if needed, but minimal API uses app services.
- Authorization: Only allow users to update their own preferences (enforced by user_id from auth).
- Input Validation: FluentValidation for body; ensure ids exist to prevent invalid references.
- SQL Injection: Supabase client handles parameterization; use RPC or direct queries safely.
- Data Exposure: Response only includes non-sensitive data; no passwords or personal info beyond user_id.
- Rate Limiting: Consider implementing if high traffic expected, but not specified.
- HTTPS: Enforce in production.

Potential Threats:
- Unauthorized access: Mitigated by auth checks.
- Invalid data injection: Mitigated by validation and constraints.
- DoS via long inputs: Length check on dislikedIngredients.

## 7. Error Handling
- Validation Errors (400): Collect FluentValidation errors, return ProblemDetails with details.
- Invalid IDs (400): If dietTypeId or preferredCuisineId not found.
- Database Errors (500): Wrap Supabase exceptions, log details (e.g., unique violation unlikely).
- Auth Errors (401): Handled by Supabase middleware.
- Not Found (404): Not applicable for creation, but for invalid references use 400.
- Logging: Use ILogger in service/repository for errors; no specific error table here (recipes_generations has error logging, but not for this).

Implement global exception handler in Program.cs for consistent ProblemDetails responses.

Potential Scenarios:
- Missing required fields: 400
- Invalid id values: 400
- DislikedIngredients >1000 chars: 400
- Database connection failure: 500
- User not authenticated: 401

## 8. Performance Considerations
- Single database upsert: Efficient, O(1) time.
- Validation: Fast, in-memory.
- Queries for id existence: Simple SELECT COUNT or EXISTS, indexed.
- Potential Bottlenecks: Supabase latency; use connection pooling.
- Optimization: Batch if needed, but single operation. Cache dictionary tables if frequently accessed, but small static data.
- Pagination: N/A.

## 9. Implementation Steps
1. Ensure UserPreferencesCreateDto exists in PantryPal.Data/ApiModels.cs (already present).
2. Create FluentValidation validator: UserPreferencesCreateDtoValidator in src/PantryPal.Api/Validators/
   - Validate DietTypeId >0 and exists in diet_types (custom validator with repo injection or separate query).
   - Validate PreferredCuisineId >0 and exists.
   - Validate DislikedIngredients length <=1000 if present.
3. In UserPreferencesService.cs (create if not exists, but from recent files it seems partial):
   - Add UpsertPreferencesAsync method: Take dto and userId, validate ids via repo, perform upsert.
   - Map to UserPreferencesDto, join for names.
4. In UserPreferencesRepository.cs:
   - Add methods: bool DietTypeExistsAsync(short id), bool PreferredCuisineExistsAsync(short id).
   - Add UpsertUserPreferencesAsync: Use Supabase client to INSERT/UPDATE with ON CONFLICT.
5. In Program.cs: Add endpoint map:
   ```csharp
   app.MapPost(\"/user-preferences\", async (UserPreferencesCreateDto dto, IUserPreferencesService service, ClaimsPrincipal user) => {
       var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
       var result = await service.UpsertPreferencesAsync(dto, userId);
       return Results.Ok(result);
   }).Produces<UserPreferencesDto>(200).Produces(400).Produces(401).Produces(500);
   ```
   - Add validation: .AddValidator<UserPreferencesCreateDtoValidator>()
6. Update interfaces: IUserPreferencesService, IUserPreferencesRepository accordingly.
7. Add unit tests in PantryPal.Api.UnitTests/Services/UserPreferencesServiceTests.cs:
   - Test valid upsert.
   - Test invalid ids.
   - Test length validation.
8. Update PantryPal.Api.http for testing the endpoint.
9. Ensure Supabase client is injected (from Program.cs services).
10. Review for clean code: Guard clauses, early returns, error handling.
