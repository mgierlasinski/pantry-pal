# REST API Plan

## 1. Resources
- **PantryItem** (`pantry_items`)
- **Recipe** (`recipes`)
- **RecipeGeneration** (`recipes_generations`)
- **UserPreferences** (`user_preferences`)
- **DietType** (`diet_types`)
- **PreferredCuisine** (`preferred_cuisines`)
- **RecipeRejectReason** (`recipe_reject_reasons`)

## 2. Endpoints

### 2.1 Pantry Items
- **GET /pantry-items**
  - Description: List all pantry items for the authenticated user.
  - Query Parameters:
    - `page` (integer, default=1)
    - `pageSize` (integer, default=20)
    - `sort` (`created_at`, `name`, default=`created_at`)
  - Response: 200 OK
    ```json
    {
      "items": [ { "id": "...", "name": "...", "is_favorite": true, "created_at": "..." } ],
      "page": 1,
      "pageSize": 20,
      "total": 42
    }
    ```
- **POST /pantry-items**
  - Description: Create a new pantry item.
  - Request:
    ```json
    { "name": "Tomato" }
    ```
  - Validation:
    - `name` length between 1 and 100
  - Response: 201 Created
    ```json
    { "id": "...", "name": "Tomato", "is_favorite": false }
    ```
- **GET /pantry-items/{id}**
  - Description: Retrieve a single pantry item.
  - Response: 200 OK
- **PATCH /pantry-items/{id}**
  - Description: Update name or favorite status.
  - Request:
    ```json
    { "name": "Cherry Tomato", "is_favorite": true }
    ```
  - Response: 200 OK
- **DELETE /pantry-items/{id}**
  - Description: Remove pantry item.
  - Response: 204 No Content

### 2.2 Recipe Generation & Management
- **POST /recipes/generate**
  - Description: Generate an AI recipe based on pantry and preferences.
  - Request: (no body) server reads user pantry & preferences
  - Response: 200 OK
    ```json
    {
      "generationId": "...",
      "recipeText": "# Recipe in Markdown..."
    }
    ```
- **POST /recipes/{generationId}/accept**
  - Description: Accept and save generated recipe.
  - Response: 201 Created
    ```json
    { "recipeId": "...", "savedAt": "..." }
    ```
- **POST /recipes/{generationId}/reject**
  - Description: Reject recipe with reason.
  - Request:
    ```json
    { "rejectReasonId": 1 }
    ```
  - Validation: `rejectReasonId` exists
  - Response: 204 No Content

### 2.3 Saved Recipes
- **GET /recipes**
  - Description: List saved recipes.
  - Query Parameters: `page`, `pageSize`, `sort=created_at`
  - Response: 200 OK
- **GET /recipes/{id}**
  - Description: Get saved recipe details.
- **DELETE /recipes/{id}**
  - Description: Delete saved recipe.
  - Response: 204 No Content

### 2.4 Recipe Generation Logs
- **GET /recipes/generations**
  - Description: List past generation attempts.
  - Query Parameters: `page`, `pageSize`, `sort=created_at`
  - Response: 200 OK

### 2.5 User Preferences
- **GET /user-preferences**
  - Description: Retrieve current user preferences.
- **POST /user-preferences**
  - Description: Create or update preferences.
  - Request:
    ```json
    { "dietTypeId": 2, "preferredCuisineId": 3, "dislikedIngredients": "nuts" }
    ```
  - Validation:
    - `dislikedIngredients` length ≤ 1000
    - `dietTypeId`, `preferredCuisineId` exist
  - Response: 200 OK

### 2.6 Dictionary Endpoints (Read-Only)
- **GET /diet-types**
- **GET /preferred-cuisines**
- **GET /recipe-reject-reasons**

## 3. Authentication and Authorization
- Mechanism: JWT Bearer tokens issued by Supabase.
- Implementation: Use ASP.NET minimal API middleware to validate JWT, extract `user_id`.
- Enforcement: All resource routes require authentication; DB row-level security ensures users only see their own data.

## 4. Validation and Business Logic
- pantry_items.name must be 1–100 characters.
- user_preferences.disliked_ingredients max 1000 characters.
- References (`dietTypeId`, `preferredCuisineId`, `rejectReasonId`) must refer to existing dictionary rows.
- Generation endpoint:
  - Fetch pantry & preferences, call AI service.
  - Persist to `recipes_generations` 
    - generation metadata with `model`, `duration_ms`.
    - if AI error occurs, record `error_code` and `error_message`.
- Accept endpoint:
  - Insert into `recipes`, update `recipes_generations.generated_recipe_id`.
- Reject endpoint:
  - Update `recipes_generations.reject_reason_id`.
- Error handling:
  - Return 400 Bad Request for validation failures.
  - Return 401 Unauthorized if no valid JWT.
  - Return 404 Not Found for missing resources.
  - Return 500 Internal Server Error for unexpected failures.

