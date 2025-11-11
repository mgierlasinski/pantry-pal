using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PantryPal.Api.Exceptions;

internal sealed class ArgumentExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ArgumentExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ArgumentException argumentException)
        {
            return false;
        }

        logger.LogWarning(argumentException, "Argument exception occurred");

        var (statusCode, title, detail) = argumentException.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? (StatusCodes.Status404NotFound, "Not Found", "The requested resource was not found.")
            : (StatusCodes.Status400BadRequest, "Bad Request", "Invalid request parameters.");

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = argumentException,
            ProblemDetails = new ProblemDetails
            {
                Type = argumentException.GetType().Name,
                Title = title,
                Detail = detail
            }
        });
    }
}
