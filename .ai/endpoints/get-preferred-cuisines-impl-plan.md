# API Endpoint Implementation Plan: GET /preferred-cuisines

## 1. Endpoint Overview
The GET /preferred-cuisines endpoint retrieves a list of all available preferred cuisines from the system's dictionary table. This is a read-only operation that provides static reference data for user preferences, such as 'Polish', 'Italian', 'Asian', etc. The endpoint supports the user preference management feature by allowing the mobile app to populate cuisine selection options. No user-specific filtering is applied, as this is global dictionary data.

## 2. Request Details
- HTTP Method: GET
- URL Structure: /preferred-cuisines
- Parameters:
  - Required: None
  - Optional: None
- Request Body: None

## 3. Response Details
- **200 OK**: Successful response containing the list of preferred cuisines.
  - Body: 
    ```json
    {
      "PreferredCuisines": [
        {
          "Id": 1,
          "Name": "Polish"
        },
        {
          "Id": 2,
          "Name": "Italian"
        }
        // ... other cuisines
      ]
    }
    ```
- **401 Unauthorized**: If authentication is enforced (though not required for this endpoint).
- **500 Internal Server Error**: Server-side issues, such as database connectivity failures.

## 4. Used Types
- **PreferredCuisineDto** (existing in src/PantryPal.Data/ApiModels.cs): 
  ```csharp
  public record PreferredCuisineDto(
      short Id,
      string Name
  );
  ```
- **PreferredCuisinesResponseDto** (new, to be added in src/PantryPal.Data/ApiModels.cs):
  ```csharp
  public record PreferredCuisinesResponseDto(IEnumerable<PreferredCuisineDto> PreferredCuisines);
  ```
No Command Models are required, as this is a read-only GET endpoint with no input payload.

## 5. Data Flow
1. The request hits the minimal API endpoint in Program.cs.
2. The endpoint invokes the PreferredCuisinesService.GetAllAsync() method.
3. The service calls the IPreferredCuisinesRepository.GetAllAsync() to query the Supabase database.
4. The repository executes a SQL query: `SELECT id, name FROM preferred_cuisines ORDER BY name;`.
5. Results are mapped to PreferredCuisineDto instances.
6. The service wraps the list in PreferredCuisinesResponseDto and returns it.
7. The endpoint serializes the response and returns 200 OK.
No external services (e.g., AI models) are involved. The flow relies on Supabase's PostgreSQL for data retrieval.

## 6. Security Considerations
- **Authentication**: Not required, as this endpoint exposes public dictionary data. However, if global API authentication is enforced via Supabase JWT tokens, include optional auth middleware but allow anonymous access for this route.
- **Authorization**: No user-specific authorization needed, since data is non-sensitive and global.
- **Data Validation**: No input validation required due to lack of parameters or body. Output validation ensures DTOs are properly formed (e.g., non-null names).
- **Potential Threats**:
  - Denial-of-Service (DoS): Limit query frequency if needed, but low risk for small static table.
  - SQL Injection: Mitigated by using Supabase client with parameterized queries or RPCs.
  - Data Exposure: No PII or sensitive data; only cuisine names.
- Follow Supabase Row Level Security (RLS) policies, but since this is a dictionary table, set policies to allow public SELECT.

## 7. Error Handling
- **Validation Errors**: None applicable (no input).
- **Database Errors** (e.g., connection failure, query timeout):
  - Log the exception using ILogger (e.g., service/repository logs error details).
  - Return 500 Internal Server Error with a generic message: "An error occurred while retrieving preferred cuisines."
  - Do not log to a specific error table; use application logging (e.g., Serilog if configured) for monitoring.
- **Not Found**: Unlikely, as the table is seeded; if empty, return 200 with empty list.
- **Other Scenarios**:
  - 401: If auth is required and token is invalid/missing.
  - 429 Too Many Requests: If rate limiting is implemented.
- Use guard clauses in service methods for early error returns. Implement global exception middleware in Program.cs for consistent error responses (e.g., ProblemDetails format).
- Error Logging: In repository/service, use `logger.LogError(ex, "Error retrieving preferred cuisines");`.

## 8. Performance Considerations
- **Bottlenecks**: Minimal; the preferred_cuisines table is small (e.g., <10 rows from seed data) and static.
- **Optimization Strategies**:
  - Cache the response in memory (e.g., using IMemoryCache in service) with a long TTL (e.g., 1 hour) since data rarely changes.
  - Use efficient indexing: The table already has a UNIQUE index on name; ensure ORDER BY uses indexed columns.
  - Pagination: Not needed due to small dataset; if expanded, add query params for page/size.
  - Query Efficiency: Single SELECT query; avoid N+1 issues (none here).
  - Supabase Performance: Leverage connection pooling via Supabase client.

## 9. Implementation Steps
1. **Create DTO**: Add `PreferredCuisinesResponseDto` to src/PantryPal.Data/ApiModels.cs. Update the project reference if needed.
2. **Create Repository**:
   - Add IPreferredCuisinesRepository.cs in src/PantryPal.Api/Repositories with `Task<IEnumerable<PreferredCuisineDto>> GetAllAsync();`.
   - Implement PreferredCuisinesRepository.cs using Supabase client: Inject ISupabaseClient, execute `await client.From&lt;PreferredCuisine&gt;().Get();` (map to DTO).
3. **Create Service**:
   - Add IPreferredCuisinesService.cs in src/PantryPal.Api/Services with `Task<PreferredCuisinesResponseDto> GetAllAsync();`.
   - Implement PreferredCuisinesService.cs: Inject repository and ILogger, call repo, map to response DTO, handle errors with guard clauses.
4. **Dependency Injection**: In src/PantryPal.Api/Program.cs, register repository (scoped) and service (scoped): `builder.Services.AddScoped<IPreferredCuisinesRepository, PreferredCuisinesRepository>();` and similarly for service.
5. **Add Endpoint**: In Program.cs, map the route: `app.MapGet("/preferred-cuisines", async (IPreferredCuisinesService service) => await service.GetAllAsync()).Produces&lt;PreferredCuisinesResponseDto&gt;(200);`.
6. **Validation**: No FluentValidation needed; add output checks in service if required.
7. **Testing**:
   - Add unit test in tests/PantryPal.Api.UnitTests/Services/PreferredCuisinesServiceTests.cs: Mock repository, verify DTO mapping and error handling.
   - Integration test: Use .http file in PantryPal.Api.http to test endpoint.
