namespace PantryPal.Mobile.Models;

public record AuthResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static AuthResult Success() => new() { IsSuccess = true };
    public static AuthResult Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
