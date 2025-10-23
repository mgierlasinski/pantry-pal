# API Endpoint Implementation Plan: GET /pantry-items

## 1. Endpoint Overview
Retrieves a paginated, optionally filtered and sorted list of pantry items for the authenticated user.

## 2. Request Details
- HTTP Method: GET
- URL: `/pantry-items`
- Query Parameters:
  - Required:
    - `page` (integer, default = 1): page index (must be ≥ 1)
    - `pageSize` (integer, default = 20): items per page (must be ≥ 1, max 100)
  - Optional:
    - `favorite` (boolean): filter items marked as favorite
    - `sort` (string): sort field, allowed values `created_at`, `name` (default = `created_at`)
- Request Body: none

## 3. Used Types
- Request binding: use built-in minimal API parameter binding for primitives.
- DTOs:
  - `PantryItemDto` (id, name, is_favorite, created_at, updated_at)
  - `PantryItemsPaginatedResponseDto` (IEnumerable<PantryItemDto> Items, int Page, int PageSize, int Total)

## 4. Response Details
- Success (200 OK): JSON matching `PantryItemsPaginatedResponseDto`:
  ```json
  {
    "items": [ { "id": "...", "name": "...", "is_favorite": true, "created_at": "..." } ],
    "page": 1,
    "pageSize": 20,
    "total": 42
  }
  ```
- Errors:
  - 400 Bad Request: invalid query parameters
  - 401 Unauthorized: missing or invalid authentication
  - 500 Internal Server Error: unhandled exceptions

## 5. Data Flow
1. **Authentication middleware** validates JWT, populates `HttpContext.User`.
2. **Endpoint handler** binds query parameters and validates them.
3. **Service layer** method `GetPantryItemsAsync(userId, page, pageSize, favorite, sortField)`:
   - Calls `IPantryRepository.GetPantryItemsAsync(userId, page, pageSize, favorite, sortField)`
   - Receives raw list of `PantryItemsSelect` and total count.
   - Maps `PantryItemsSelect` to `PantryItemDto`.
   - Wraps list and metadata into `PantryItemsPaginatedResponseDto`.
4. **Minimal API** returns result or handles exceptions.

## 6. Security Considerations
- **Authentication**: Protect endpoint with `app.UseAuthentication()` and `app.UseAuthorization()`. Add `.RequireAuthorization()` on route.
- **Authorization**: Ensure `user_id` filter so users can only view their own items.
- **Parameter validation**: guard against invalid or malicious input (e.g., overly large `pageSize`).
- **Injection**: rely on Supabase client parameterization to avoid SQL injection.

## 7. Error Handling
- **Invalid parameters**: return 400 with descriptive message when
  - `page` or `pageSize` < 1 or > max
  - `sort` not in allowed set
  - `favorite` cannot be parsed as bool
- **Unauthenticated**: return 401
- **Database/Service errors**: catch exceptions in service or endpoint, log via `ILogger`, return 500 with generic error message

## 8. Performance Considerations
- **Indexes**: leverage `idx_pantry_user_created`, `uq_pantry_user_name` for user filtering and sorting.
- **Pagination**: use efficient range queries with LIMIT/OFFSET or Supabase Range.
- **Count**: separate count call may incur overhead; consider using window functions or caching if scale demands.

## 9. Implementation Steps
1. **Configure middleware**: Ensure authentication & authorization are enabled in `Program.cs`.
2. **Create repository interface** `IPantryRepository` in `src/PantryPal.Api/Repositories`:
   ```csharp
   public interface IPantryRepository
   {
       Task<(IEnumerable<PantryItemsSelect> Items, int Total)> GetPantryItemsAsync(Guid userId, int page, int pageSize, bool? favorite, string sortField);
   }
   ```
3. **Implement repository** `PantryRepository`:
   - Inject `Client` (Supabase) and `ILogger<PantryRepository>`.
   - Implement `GetPantryItemsAsync`:
     - Build Supabase query (filter by `user_id`, optional favorite, sort, range)
     - Execute query and separate count retrieval.
     - Return tuple of raw items and total.
4. **Register repository** in DI container:
   ```csharp
   builder.Services.AddScoped<IPantryRepository, PantryRepository>();
   ```
5. **Create service interface** `IPantryService` in `src/PantryPal.Api/Services`:
   ```csharp
   public interface IPantryService
   {
       Task<PantryItemsPaginatedResponseDto> GetPantryItemsAsync(Guid userId, int page, int pageSize, bool? favorite, string sortField);
   }
   ```
6. **Implement service** `PantryService`:
   - Inject `IPantryRepository` and `ILogger<PantryService>`.
   - In `GetPantryItemsAsync`, call repository, map raw models to DTOs, construct `PantryItemsPaginatedResponseDto`.
7. **Register service** in DI container:
   ```csharp
   builder.Services.AddScoped<IPantryService, PantryService>();
   ```
8. **Define endpoint** in `Program.cs`:
   ```csharp
   app.MapGet("/pantry-items", async (int page, int pageSize, bool? favorite, string sort, IHttpContextAccessor ctx, IPantryService svc) =>
   {
       // Validate inputs
       // Extract userId from ctx.HttpContext.User
       // Call svc.GetPantryItemsAsync
       // Return Results.Ok(dto)
   }).RequireAuthorization();
   ```
9. **Input validation**: Implement guard clauses for query parameters; return `Results.BadRequest()` if invalid.
10. **Mapping**: Use LINQ to map `PantryItemsSelect` to `PantryItemDto` within service.
11. **Testing**: Write unit tests mocking `IPantryRepository` for service logic, integration tests for endpoint covering paging, filtering, sorting, and error cases.
12. **Documentation**: Update OpenAPI/Swagger docs with endpoint spec, parameters, and response schema.
13. **Review & merge**: Conduct code review, ensure coding guidelines and clean code practices are followed.
