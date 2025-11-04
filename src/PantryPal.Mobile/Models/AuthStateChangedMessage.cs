namespace PantryPal.Mobile.Models;

public class AuthStateChangedMessage
{
    public bool IsAuthenticated { get; }

    public AuthStateChangedMessage(bool isAuthenticated)
    {
        IsAuthenticated = isAuthenticated;
    }
}
