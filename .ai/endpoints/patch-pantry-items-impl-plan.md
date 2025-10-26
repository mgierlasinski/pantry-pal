# API Endpoint Implementation Plan: PATCH /pantry-items/{id}

## 1. Endpoint Overview
This endpoint allows an authenticated user to update the **name** and/or **favorite** status of an existing pantry item. Only the owner of the item may perform the update.

## 2. Request Details
- HTTP Method: PATCH
- URL Structure: `/pantry-items/{id}`
- Path Parameters:
  - `id` (UUID) - Identifier of the pantry item to update.
- Request Body (JSON):
  ```json
  {
    "name": "Cherry Tomato",       // optional, string 1–100 chars
    "isFavorite": true             // optional, boolean
  }
  ```
- Parameters:
  - Required: `id` in URL path.
  - Optional: `name`, `isFavorite` in request body. At least one must be provided.

## 3. Used Types
- `PantryItemUpdateDto` (request): carries `name` and `isFavorite` property
- `PantryItemDto` (response): contains `id`, `name`, `isFavorite`, `createdAt`, `updatedUt`

## 4. Response Details
- Success (200 OK): Returns updated `PantryItemDto`.
  ```json
  {
    "id": "...",
    "name": "Cherry Tomato",
    "isFavorite": true,
    "createdAt": "...",
    "updatedAt": "..."
  }
  ```
- Status Codes:
  - 200 OK – Update succeeded.
  - 400 Bad Request – Validation failure or no fields provided.
  - 401 Unauthorized – Missing/invalid JWT.
  - 404 Not Found – Item with `id` not found or not owned by user.
  - 409 Conflict – Duplicate name (case-insensitive) for this user.
  - 500 Internal Server Error – Unexpected errors.

## 5. Data Flow
1. **Routing & Binding**: Minimal API endpoint in `Program.cs` binds `id` and deserializes JSON to `PantryItemUpdateDto`.
2. **Authentication**: JWt middleware extracts `user_id` claim; failure returns 401 automatically.
3. **Validation**: Invoke `PantryItemUpdateDtoValidator` (FluentValidation) to ensure:
   - At least one of `Name` or `IsFavorite` is provided.
   - If `Name` provided, length between 1 and 100.
4. **Service Call**: Call `IPantryService.UpdatePantryItemAsync(id, userId, updateDto)` directly with `PantryItemUpdateDto`.
5. **Repository Operations**:
   - `GetByIdAsync(id, userId)`: fetch existing record or return null.
   - If not found → return null to service.
   - Apply changes on entity, set `UpdatedAt`=now.
   - `UpdateAsync(entity)`: persist updates to Supabase.
6. **Response Mapping**: Service returns updated `PantryItemDto`; return 200 OK with this DTO.

## 6. Security Considerations
- **Authentication**: Enforced via JWT Bearer minimal API middleware.
- **Authorization**: Filter by `user_id` when fetching the pantry item to ensure user owns resource.
- **Input Sanitization**: Parameterized queries via Supabase client prevent SQL injection.
- **Unique Constraint**: Database ensures case-insensitive unique names; handle violations gracefully.

## 7. Error Handling
| Scenario                                      | Status Code | Handling                                                                                      |
|-----------------------------------------------|-------------|-----------------------------------------------------------------------------------------------|
| Validation failure (missing fields, length)   | 400         | Return validation error details from FluentValidation.                                        |
| Unauthorized (missing/invalid JWT)            | 401         | JWT middleware returns 401 automatically.                                                     |
| Item not found or not owned by user           | 404         | Service returns null → endpoint returns 404 with message “Pantry item not found”.             |
| Duplicate name conflict                       | 409         | Catch unique-constraint exception, return 409 Conflict with message “Name already in use”.   |
| Unexpected database or service error          | 500         | Log exception via `ILogger`, return generic 500 with safe message.                            |

## 8. Performance Considerations
- Single-row update; minimal impact.
- Ensure Supabase client reuses connections (singleton injection).
- Lean DTOs and projections to avoid unnecessary data transfer.

## 9. Implementation Steps
1. **Define Service Method**:
   - Add method `Task<PantryItemDto> UpdatePantryItemAsync(Guid id, Guid userId, PantryItemUpdateDto updateDto)` to `IPantryService`.
   - No separate command model needed; service consumes `PantryItemUpdateDto` directly.
2. **Validator**:
   - Add `UpdatePantryItemRequestValidator` using FluentValidation.
3. **Service Interface**:
   - Add `Task<PantryItemDto> UpdatePantryItemAsync(Guid id, Guid userId, PantryItemUpdateDto updateDto);` to `IPantryService`.
4. **Service Implementation**:
   - Implement in `PantryService`:
     - Fetch existing item via repository.
     - If null, return null.
     - Apply fields from `PantryItemUpdateDto`, update timestamp.
     - Call `Repository.UpdateAsync`, map result to `PantryItemDto`, and return.
5. **Repository**:
   - Add `UpdateAsync(PantryItem entity)` to `IPantryRepository` and implement in `PantryRepository`.
   - Use Supabase client’s `Update` method with filter on `id` and `user_id`.
6. **Minimal API Endpoint** (`Program.cs`):
   - Add `app.MapPatch("/pantry-items/{id}", async (PantryItemUpdateDto updateDto, Guid id, IPantryService svc, HttpContext ctx) => { ... });`
   - Extract `userId` from `ctx.User` claims.
   - Validate `updateDto` via `PantryItemUpdateDtoValidator`.
   - Map `updateDto` to `UpdatePantryItemCommand` and call service; handle null → 404.
   - Map result to `PantryItemDto`; return Ok(result).
7. **Error Handling & Logging**:
   - Inject `ILogger` into service and endpoint.
   - Wrap repository calls in try/catch for unique-constraint violations and general exceptions.
8. **Documentation**:
   - Update `PantryPal.Api.http` with PATCH request examples.
   - Document in API plan.
