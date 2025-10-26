# API Endpoint Implementation Plan: DELETE /pantry-items/{id}

## 1. Endpoint Overview
Deletes a pantry item belonging to the authenticated user. Returns HTTP 204 No Content on success.

## 2. Request Details
- HTTP Method: DELETE
- URL Structure: `/pantry-items/{id}`
- Parameters:
  - Required:
    - `id` (Guid): Path parameter representing the pantry item UUID.
  - Optional: None
- Authentication: Bearer token (JWT) required. Extract `userId` from the token claims.

## 3. Used Types
- No request DTO.
- No response DTO (204 No Content).
- Existing DTOs for reference:
  - `PantryItemDto` (not returned here).
- Error response (use built-in `ProblemDetails` for error payloads).

## 4. Response Details
- 204 No Content: Item successfully deleted.
- 400 Bad Request: Invalid `id` format.
- 401 Unauthorized: No or invalid authentication.
- 404 Not Found: Item does not exist or does not belong to the user.
- 500 Internal Server Error: Unexpected server/database error.

## 5. Data Flow
1. **Controller/Endpoint**: Minimal API route in `Program.cs`:
   - Bind `id` (Guid) and inject `IPantryService`.
   - Extract `userId` from `ClaimsPrincipal`.
   - Call `DeletePantryItemAsync(id, userId)`.
2. **Service Layer** (`PantryService`):
   - Guard clause: check parameters.
   - Invoke `IPantryRepository.DeletePantryItemAsync(id, userId)`.
   - If repository indicates zero rows deleted, throw `ArgumentException`.
3. **Repository Layer** (`PantryRepository`):
   - Execute Supabase delete on `pantry_items` filtering `id` and `user_id`.
   - Return number of rows affected.

## 6. Security Considerations
- **Authentication**: Enforce JWT-based authentication.
- **Authorization**: Verify that the `userId` from token matches the `user_id` filter in delete query.
- **Input Validation**: Rely on model binding for GUID; guard in service to prevent malicious input.
- **Transport**: Ensure endpoint served over HTTPS.

## 7. Error Handling
| Scenario                                | Exception / Check             | Response Code | Logging                   |
|-----------------------------------------|-------------------------------|---------------|---------------------------|
| Malformed GUID                          | Model binding failure         | 400           | Automatic via framework   |
| Unauthenticated                         | Missing/invalid JWT           | 401           | Automatic via middleware  |
| Item not found or wrong owner           | `ArgumentException`           | 404           | Log warning in service    |
| Database deletion failure               | `InvalidOperationException`   | 500           | Log error in service      |
| Other unexpected errors                 | `Exception` catch-all         | 500           | Log critical in service   |

## 8. Performance Considerations
- Single-row delete indexed by primary key and user_id—constant time.
- No pagination or bulk operations.
- Supabase client connection reused via DI.

## 9. Implementation Steps
1. **Repository Interface**: Add signature
   ```csharp
   Task DeletePantryItemAsync(Guid id, Guid userId);
   ```
2. **Repository Implementation**: Implement method using Supabase:
   - `await client.Table<...>().Where(...).Delete();`
   - Check `Count` of deleted rows.
3. **Service Interface**: Extend `IPantryService`:
   ```csharp
   Task DeletePantryItemAsync(Guid id, Guid userId);
   ```
4. **Service Implementation**:
   - Inject `IPantryRepository` and `ILogger<PantryService>`.
   - Call repository; throw `ArgumentException` if 0.
   - Wrap exceptions if needed.
5. **Endpoint Mapping**: In `Program.cs`:
   ```csharp
   app.MapDelete("/pantry-items/{id}", async (Guid id, ClaimsPrincipal user, IPantryService svc) => {
     var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
     await svc.DeletePantryItemAsync(id, userId);
     return Results.NoContent();
   })
   .RequireAuthorization();
   ```
6. **Validation & Error Handling**:
   - No custom FluentValidation needed.
   - Let minimal API produce 400 for invalid GUID.
   - Use global exception handler (if configured) to translate `ArgumentException` to 404.
7. **Documentation**:
   - Update `PantryPal.Api/http` file with DELETE sample.
