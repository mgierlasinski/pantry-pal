using FluentValidation;
using PantryPal.Api.Extensions;
using PantryPal.Api.Services;
using PantryPal.Data;

namespace PantryPal.Api.Endpoints;

public static class PantryItems
{
    public static void RegisterPantryItemsEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /pantry-items endpoint
        app.MapGet("/pantry-items", async (
            HttpContext httpContext,
            [AsParameters] PantryItemsPaginatedRequestDto request,
            IValidator<PantryItemsPaginatedRequestDto> validator,
            IPantryService pantryService,
            ILogger<Program> logger) =>
        {
            // Validate request using FluentValidation
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for GET /pantry-items: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var userId = httpContext.GetUserId();
            var result = await pantryService.GetPantryItemsAsync(
                userId,
                request.Page,
                request.PageSize,
                request.Sort);

            return Results.Ok(result);
        }).RequireAuthorization();

        // POST /pantry-items endpoint
        app.MapPost("/pantry-items", async (
            HttpContext httpContext,
            PantryItemCreateDto dto,
            IValidator<PantryItemCreateDto> validator,
            IPantryService pantryService,
            ILogger<Program> logger) =>
        {
            // Validate request body using FluentValidation
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for POST /pantry-items: {Errors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var userId = httpContext.GetUserId();
            var createdItem = await pantryService.CreatePantryItemAsync(userId, dto);

            logger.LogInformation("Successfully created pantry item {ItemId} for user {UserId}", createdItem.Id, userId);
            return Results.Created($"/pantry-items/{createdItem.Id}", createdItem);
        }).RequireAuthorization();

        // PATCH /pantry-items/{id} endpoint
        app.MapPatch("/pantry-items/{id}", async (
            HttpContext httpContext,
            Guid id,
            PantryItemUpdateDto dto,
            IValidator<PantryItemUpdateDto> validator,
            IPantryService pantryService,
            ILogger<Program> logger) =>
        {
            // Validate request body using FluentValidation
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for PATCH /pantry-items/{Id}: {Errors}",
                    id, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var userId = httpContext.GetUserId();
            var updatedItem = await pantryService.UpdatePantryItemAsync(id, userId, dto);

            logger.LogInformation("Successfully updated pantry item {ItemId} for user {UserId}", updatedItem.Id, userId);
            return Results.Ok(updatedItem);
        }).RequireAuthorization();

        // DELETE /pantry-items/{id} endpoint
        app.MapDelete("/pantry-items/{id}", async (
            HttpContext httpContext,
            Guid id,
            IPantryService pantryService,
            ILogger<Program> logger) =>
        {
            var userId = httpContext.GetUserId();
            await pantryService.DeletePantryItemAsync(id, userId);

            logger.LogInformation("Successfully deleted pantry item {ItemId} for user {UserId}", id, userId);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
