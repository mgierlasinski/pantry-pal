# API Endpoint Implementation Plan: GET /recipe-reject-reasons

## 1. Endpoint Overview
This endpoint retrieves a list of predefined recipe reject reasons, which are static dictionary values stored in the `recipe_reject_reasons` table. These reasons allow users to provide feedback when rejecting AI-generated recipes, such as \"I don't have these ingredients\" or \"I don't like this dish\". The endpoint returns all available reasons without user-specific filtering, as the data is global. It supports the MVP's recipe rejection flow by providing options for the mobile app to display in rejection dialogs.

## 2. Request Details
- HTTP Method: GET
- URL Structure: `/recipe-reject-reasons`
- Parameters:
  - Required: None
  - Optional: None
- Request Body: None (as this is a GET request)

## 3. Used Types
- **DTOs**:
  - `RecipeRejectReasonDto` (existing in `src/PantryPal.Data/ApiModels.cs`): 
    ```csharp
    public record RecipeRejectReasonDto(short Id, string Description);
    ```
    Represents a single reject reason with its ID and description.
  - `RecipeRejectReasonsResponseDto` (new, to be added in `src/PantryPal.Data/ApiModels.cs`):
    ```csharp
    public record RecipeRejectReasonsResponseDto(IEnumerable<RecipeRejectReasonDto> RejectReasons);
    ```
    Wraps the list of reject reasons for a consistent paginated/list response structure (though no pagination is needed here due to small dataset).
- **Command Models**: None required, as this is a read-only endpoint with no input.

## 4. Response Details
- **Success (200 OK)**:
  - Body: `RecipeRejectReasonsResponseDto` containing an array of `RecipeRejectReasonDto` objects.
  - Example:
    ```json
    {
      \"rejectReasons\": [
        { \"id\": 1, \"description\": \"I don't have these ingredients\" },
        { \"id\": 2, \"description\": \"I don't like this dish\" },
        { \"id\": 3, \"description\": \"Other\" }
      ]
    }
    ```
- **Error Status Codes**:
  - 401 Unauthorized: If the user is not authenticated.
  - 500 Internal Server Error: For server-side issues, such as database connection failures.

No 404 is expected, as the reasons are static and always present.

## 5. Data Flow
1. **Authentication**: Supabase client authenticates the request using the user's JWT token (via `Authorization: Bearer` header).
2. **Endpoint Handler** (in `Program.cs`): Extracts the authenticated user ID from the Supabase context. Calls the `IRecipeRejectReasonsService.GetAllAsync()` method.
3. **Service Layer** (`RecipeRejectReasonsService`): Implements business logic (minimal here, as data is static). Calls the repository to fetch data. Applies any necessary transformations (e.g., mapping DB entities to DTOs). Handles caching if implemented for performance.
4. **Repository Layer** (`RecipeRejectReasonsRepository`): Uses the Supabase client to execute a SELECT query on the `recipe_reject_reasons` table: `SELECT id, description FROM recipe_reject_reasons ORDER BY id;`. No RLS needed, as the table is global (no user_id column). Maps results to a list of entities.
5. **Database Interaction**: Supabase PostgreSQL query. Relies on seed data to ensure default rows exist.
6. **Response Serialization**: Minimal API automatically serializes the `RecipeRejectReasonsResponseDto` to JSON.

No external services (e.g., AI models) are involved. The flow emphasizes separation of concerns via repository and service patterns.

## 6. Security Considerations
- **Authentication**: Required. Use Supabase's built-in auth to validate JWT and extract `user_id`. Implement in the endpoint map using `MapGet` with auth middleware (e.g., `RequireAuthorization()` or custom Supabase auth handler in `Program.cs`).
- **Authorization**: No user-specific access control needed, as reject reasons are global dictionary data. However, ensure the authenticated user has access to the app (e.g., via Supabase RLS policies on related tables if extended later).
- **Data Validation**: No input validation required (no params/body). On the response side, validate that fetched data matches expected schema (e.g., non-null descriptions, valid IDs) in the service layer using guard clauses.
- **Potential Threats**:
  - **Unauthorized Access**: Mitigated by JWT validation.
  - **SQL Injection**: Prevented by using Supabase's parameterized queries (implicit in the client).
  - **DoS (Denial of Service)**: Small dataset (~3 rows), so low risk. Rate limiting can be added via middleware if needed.
  - **Data Exposure**: Only exposes `id` and `description`; no sensitive fields.
- Follow Supabase guidelines: Enable RLS on the table if future user-specific reasons are added. Use HTTPS for all requests.

## 7. Error Handling
- **Input Validation**: N/A (no inputs). Use FluentValidation if parameters are added later.
- **Potential Error Scenarios**:
  - **401 Unauthorized**: User not authenticated or invalid JWT. Return `Problem("Unauthorized access.")`.
  - **500 Internal Server Error**:
    - Database connection failure: Catch Supabase exceptions in repository, log details (e.g., using ILogger), and rethrow as `Problem("Failed to retrieve reject reasons.")`.
    - Empty result set (unexpected, due to seeds): Service checks if list is empty and returns 500 with "Configuration error: No reject reasons found."
    - Mapping/Transformation errors: Handle in service with try-catch, log stack trace.
- **Logging**: Use `ILogger` in service and repository for errors. Log error codes/messages to `recipes_generations` table only if tied to a generation (not applicable here); otherwise, use application logs. Implement consistent error responses with `IResult` using `Results.Problem`.
- **Early Returns/Guard Clauses**: In service methods, check for null/empty results early and return appropriate errors.
- No 400 (invalid input) or 404 (not found) expected for this endpoint.

## 8. Performance Considerations
- **Bottlenecks**: Minimal – single, fast SELECT on a small table (3 rows). No joins or complex computations.
- **Optimizations**:
  - **Caching**: Since data is static (dictionary), cache the response in service using IMemoryCache (singleton lifetime) with a long expiration (e.g., 1 hour) or eternal cache, invalidated only on app restart or manual trigger.
  - **Database**: Leverage existing indexes (none needed for full table scan on small table). Ensure Supabase connection pooling.
  - **Query Efficiency**: Use `SELECT id, description` to fetch only required columns. Avoid pagination as total rows are fixed and small.
  - **Scalability**: Endpoint is read-heavy but low-volume; no issues for MVP scale.
  - **Monitoring**: Add metrics (e.g., response time) via middleware if using observability tools.

## 9. Implementation Steps
1. **Update DTOs**: Add `RecipeRejectReasonsResponseDto` to `src/PantryPal.Data/ApiModels.cs` if not present. Ensure `RecipeRejectReasonDto` exists.
2. **Create Repository**:
   - Add `IRecipeRejectReasonsRepository.cs` in `src/PantryPal.Api/Repositories/` with `Task<IEnumerable<RecipeRejectReason>> GetAllAsync();`.
   - Implement `RecipeRejectReasonsRepository.cs` injecting `ISupabaseClient`, using `From<RecipeRejectReason>.Get()` or raw SQL for selection, mapping to entities (define `RecipeRejectReason` in `Db/DatabaseTypes.cs` if missing: record with Id and Description).
3. **Create Service**:
   - Add `IRecipeRejectReasonsService.cs` in `src/PantryPal.Api/Services/` with `Task<IEnumerable<RecipeRejectReasonDto>> GetAllAsync();`.
   - Implement `RecipeRejectReasonsService.cs` injecting repository and logger. Map entities to DTOs, add caching if desired, handle errors with guard clauses.
4. **Register Dependencies**: In `Program.cs`, add `builder.Services.AddScoped<IRecipeRejectReasonsRepository, RecipeRejectReasonsRepository>();` and `AddScoped<IRecipeRejectReasonsService, RecipeRejectReasonsService>();`.
5. **Add Endpoint**: In `Program.cs`, map `app.MapGet("/recipe-reject-reasons", async (IRecipeRejectReasonsService service, ClaimsPrincipal user) => { /* auth check */ return Results.Ok(new RecipeRejectReasonsResponseDto(await service.GetAllAsync())); }).RequireAuthorization();`.
6. **Validation & Error Handling**: Add try-catch in service/endpoint, use `Results.Problem` for errors. No FluentValidation needed.
7. **Testing**:
   - Add unit tests in `tests/PantryPal.Api.UnitTests/Services/RecipeRejectReasonsServiceTests.cs`: Mock repository, test happy path, error scenarios, caching.
   - Integration tests: Use `PantryPal.Api.http` to test endpoint with auth.
8. **Documentation**: Update `PantryPal.Api.http` with the new endpoint example. Update API plan in `.ai/api-plan.md` if needed.
9. **Review**: Follow coding practices – early returns, no nested ifs, descriptive names. Run linter and tests before commit.
