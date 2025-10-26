# API Endpoint Implementation Plan: Create Pantry Item

## 1. Endpoint Overview
- Purpose: Create a new pantry item for an authenticated user.
- HTTP Method: POST
- URL: `/pantry-items`
- Authentication: Bearer JWT required

## 2. Request Details
- HTTP Method: POST
- URL Structure: `/pantry-items`
- Parameters:
  - Required: `name` (string, length 1–100)
  - Optional: none
- Request Body:
```json
{
  "name": "Tomato"
}
```

## 3. Used Types
- **PantryItemCreateDto** (request): carries the `name` property
- **PantryItemDto** (response): contains `id`, `name`, `isFavorite`, `createdAt`, `updatedAt`

## 4. Response Details
- 201 Created
  - Response Body:
  ```json
  {
    "id": "<uuid>",
    "name": "Tomato",
    "isFavorite": false,
    "createdAt": "<timestamp>",
    "updatedAt": "<timestamp>"
  }
  ```
- 400 Bad Request: validation failure (missing or invalid `name`)
- 401 Unauthorized: missing or invalid authentication token
- 409 Conflict: item name already exists for the user
- 500 Internal Server Error: unexpected errors

## 5. Data Flow
1. **Handler** (Program.cs minimal API)
   - Apply `[Authorize]` middleware.
   - Bind and validate request body to `PantryItemCreateDto`.
   - Extract `userId` from `HttpContext.User` claims.
   - Call `IPantryService.CreatePantryItemAsync(userId, dto)`.
2. **Service** (`PantryService`)
   - Guard clause: ensure `name` is not null or empty.
   - Validate `name` length (1–100) using guard clause.
   - Construct `PantryItemsInsert` model with `UserId`, `Name`.
   - Call `IPantryRepository.CreatePantryItemAsync(model)`.
   - Map returned `PantryItemsSelect` to `PantryItemDto`.
   - Log success via `ILogger<PantryService>`.
3. **Repository** (`PantryRepository`)
   - Use `Supabase.Client.From<PantryItemsInsert>().Insert()` to insert record.
   - Handle database errors (e.g., unique constraint violation).
   - Return inserted `PantryItemsSelect` model.
4. **Response**
   - Handler returns `Results.Created("/pantry-items/{id}", dto)`.

## 6. Security Considerations
- Enforce authentication with JWT and `[Authorize]`.
- Retrieve and trust `userId` only from validated token claims.
- Prevent overposting: bind only the `name` field.
- Validate input at boundary to mitigate malformed requests.

## 7. Error Handling
| Scenario                              | Action                                                           | Status Code |
|---------------------------------------|------------------------------------------------------------------|-------------|
| Missing or empty `name`               | Return `Results.BadRequest("Name is required")`                | 400         |
| `name` length out of range            | Return `Results.BadRequest("Name must be 1–100 characters")`   | 400         |
| Duplicate `name` for user             | Catch unique constraint error, log, return `Results.Conflict()`  | 409         |
| Unauthenticated request               | Handled by auth middleware, return `401 Unauthorized`            | 401         |
| Unexpected exception                  | Log exception, return `Results.Problem()`                        | 500         |

## 8. Performance Considerations
- Single-row insert; minimal overhead.
- Reuse `Supabase.Client` via DI to benefit from connection pooling.
- No heavy processing; ensure asynchronous calls await properly.

## 9. Implementation Steps
1. **Program.cs**
   - Map `POST /pantry-items` endpoint.
   - Apply `.RequireAuthorization()` and JSON body binding.
2. **IPantryService**
   - Add method `Task<PantryItemDto> CreatePantryItemAsync(Guid userId, PantryItemCreateDto dto)`.
3. **PantryService**
   - Implement `CreatePantryItemAsync`, including guard clauses and logging.
4. **IPantryRepository**
   - Add method `Task<PantryItemsSelect> CreatePantryItemAsync(PantryItemsInsert model)`.
5. **PantryRepository**
   - Implement `CreatePantryItemAsync` using Supabase insert.
   - Handle Postgrest exceptions for uniqueness.
6. **Dependency Injection**
   - Register new methods in DI container (`builder.Services.AddScoped<IPantryService, PantryService>();`).
