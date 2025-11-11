using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PantryPal.Api.Exceptions;

internal sealed class InvalidOperationExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<InvalidOperationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not InvalidOperationException invalidOperationException)
        {
            return false;
        }

        logger.LogWarning(invalidOperationException, "Invalid operation exception occurred");

        var (statusCode, title, detail) = GetStatusCodeAndDetails(invalidOperationException.Message);

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = invalidOperationException,
            ProblemDetails = new ProblemDetails
            {
                Type = invalidOperationException.GetType().Name,
                Title = title,
                Detail = detail
            }
        });
    }

    private static (int statusCode, string title, string detail) GetStatusCodeAndDetails(string message)
    {
        var messageLower = message.ToLowerInvariant();

        if (messageLower.Contains("already exists"))
        {
            return (StatusCodes.Status409Conflict, "Conflict", "A resource with this identifier already exists.");
        }

        if (messageLower.Contains("already accepted") || messageLower.Contains("already rejected"))
        {
            return (StatusCodes.Status409Conflict, "Conflict", "This operation has already been performed.");
        }

        if (messageLower.Contains("preferences not set") ||
            messageLower.Contains("pantry is empty") ||
            messageLower.Contains("no recipe text available"))
        {
            return (StatusCodes.Status400BadRequest, "Bad Request", "Required conditions are not met for this operation.");
        }

        if (messageLower.Contains("failed to generate recipe"))
        {
            return (StatusCodes.Status500InternalServerError, "Recipe Generation Failed", "Recipe generation failed: AI service error.");
        }

        // Default case
        return (StatusCodes.Status400BadRequest, "Bad Request", "The operation cannot be performed.");
    }
}
