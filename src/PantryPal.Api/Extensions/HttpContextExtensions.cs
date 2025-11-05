using System.Security.Claims;

namespace PantryPal.Api.Extensions;

public static class HttpContextExtensions
{
    // Helper method to get user ID from JWT token
    public static Guid GetUserId(this HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Guid.Empty;
        }
        return userId;
    }
}
