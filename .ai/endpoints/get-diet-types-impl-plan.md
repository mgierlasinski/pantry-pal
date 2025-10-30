# API Endpoint Implementation Plan: GET /diet-types

## 1. Endpoint Overview
This endpoint retrieves a list of all available diet types from the global `diet_types` dictionary table. It provides reference data for user preferences, enabling clients (e.g., the MAUI mobile app) to populate dropdowns or selections for diet preferences. The data is static and seeded with values like 'standard', 'vegetarian', 'vegan', and 'gluten-free'. No authentication is required as this is public reference information. The endpoint uses ASP.NET minimal APIs, Supabase for data retrieval, and follows clean code practices with early error handling.

## 2. Request Details
- HTTP Method: GET
- URL Structure: /diet-types
- Parameters:
  - Required: None
  - Optional: None (no query parameters for pagination or filtering, as the dataset is small and static)
- Request Body: None (GET request)

## 3. Used Types
- **DTOs**:
  - `DietTypeDto` (existing in `src/PantryPal.Data/ApiModels.cs`): Represents individual diet types with `short Id` and `string Name`.
  - New: `DietTypesResponseDto` – A simple wrapper for the response: `public record DietTypesResponseDto(IEnumerable<DietTypeDto> DietTypes);`. This encapsulates the list for a clean API response structure, consistent with other paginated responses (though no pagination here).

No Command Models needed, as there is no request body.

## 4. Response Details
- **Success (200 OK)**:
  - Body: JSON array of diet types wrapped in `DietTypesResponseDto`.
  - Example:
    ```json
    {
      \"dietTypes\": [
        { \"id\": 1, \"name\": \"standard\" },
        { \"id\": 2, \"name\": \"vegetarian\" },
        { \"id\": 3, \"name\": \"vegan\" },
        { \"id\": 4, \"name\": \"gluten-free\" }
      ]
    }
    ```
- **Error Responses**:
  - 500 Internal Server Error: For database or server issues, with a generic message like `{ \"error\": \"An unexpected error occurred\" }`.
- Content-Type: application/json
- No pagination metadata needed due to small dataset.

## 5. Data Flow
1. Client (MAUI app) sends GET request to `/diet-types`.
2. ASP.NET minimal API endpoint handler invokes `IDietTypesService.GetAllAsync()` (new service).
3. Service injects `IDietTypesRepository` (new repository) via DI (scoped lifetime).
4. Repository uses Supabase client to query `diet_types` table: `SELECT id, name FROM diet_types ORDER BY id;`.
5. Map results to `DietTypeDto` instances (handle nulls with guard clauses).
6. Service returns `DietTypesResponseDto`.
7. Endpoint maps to JSON response.
8. No external services (e.g., AI) involved; purely DB read.
9. If error in repository (e.g., Supabase connection fail), propagate to service for logging and throw exception for endpoint to catch.

## 6. Security Considerations
- **Authentication**: None required; this is a public endpoint for reference data. No user-specific filtering.
- **Authorization**: Not applicable; global read access.
- **Data Validation**: Server-side mapping ensures only `id` and `name` are exposed (no sensitive fields). Use Supabase's parameterized queries to prevent SQL injection.
- **Threats & Mitigations**:
  - Rate limiting: Implement via middleware or Supabase edge functions to prevent abuse (e.g., excessive calls).
  - CORS: Configure in `Program.cs` to allow only trusted origins (MAUI app domains).
  - Input Sanitization: No user input, but validate DB results for unexpected data (e.g., empty names via guard clauses).
  - HTTPS: Enforce via Supabase/ASP.NET config.
- Follow Supabase best practices: Use read-only policies if RLS enabled (though not needed here).

## 7. Error Handling
- **Potential Errors**:
  - Database query failure (e.g., Supabase outage): Catch in repository, log via `ILogger`, rethrow as `Exception` for service/endpoint to return 500. User-friendly message: \"Unable to retrieve diet types at this time.\"
  - Mapping errors (e.g., invalid DB data): Guard clauses in service return early with empty list or 500 if critical.
  - Network/Supabase client errors: Handled in repository with try-catch, log details (e.g., error code/message).
- **Global Handling**: Use middleware in `Program.cs` for unhandled exceptions (consistent 500 responses with logging).
- **Logging**: Inject `ILogger` in service/repository; log errors with context (e.g., endpoint name, timestamp). No insertion into `recipes_generations` error fields, as this isn't AI-related.
- **Status Codes**:
  - 200: Success.
  - 500: Server/DB errors.
- Early returns for edge cases (e.g., no data: return 200 with empty array).

## 8. Performance Considerations
- **Bottlenecks**: Minimal; small table (4 rows), fast Supabase query. No joins or complex filters.
- **Optimizations**:
  - Cache results: Since static, use in-memory caching (e.g., `IMemoryCache` in service, singleton lifetime) with 1-hour expiry to reduce DB hits.
  - Indexing: `diet_types` already has PRIMARY KEY on `id`; no additional needed for full scan.
  - Async: Use async/await throughout (e.g., `GetAllAsync`) for non-blocking I/O.
  - Response Size: Tiny JSON (~200 bytes), no performance issue.
  - Monitoring: Log query duration; if >50ms, investigate Supabase connection pooling.
- Scalability: Handles high traffic easily; caching ensures sub-1ms responses after first call.

## Implementation Steps
1. Create new repository: Add `IDietTypesRepository.cs` and `DietTypesRepository.cs` in `/src/PantryPal.Api/Repositories/`. Implement `Task<IEnumerable<DietTypeDto>> GetAllAsync();` using Supabase client query.
2. Create new service: Add `IDietTypesService.cs` and `DietTypesService.cs` in `/src/PantryPal.Api/Services/`. Inject repository and `ILogger`; implement logic with guard clauses, mapping, and caching if desired. Register in `Program.cs` (scoped).
3. Define response DTO: Add `DietTypesResponseDto` to `/src/PantryPal.Data/ApiModels.cs`.
4. Implement endpoint: In `Program.cs` or a dedicated endpoints file, add `app.MapGet(\"/diet-types\", async (IDietTypesService service) => { try { var result = await service.GetAllAsync(); return Results.Ok(new DietTypesResponseDto(result)); } catch { /* log and return 500 */ } });`.
5. Add error middleware: Ensure global exception handler in `Program.cs` for 500 responses.
6. Test: Use `.http` file in `/src/PantryPal.Api/` for manual testing. Add unit tests in `/tests/PantryPal.Api.UnitTests/Services/` mocking repository.
