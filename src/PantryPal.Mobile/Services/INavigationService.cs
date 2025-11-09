namespace PantryPal.Mobile.Services;

public interface INavigationService
{
    Task GoToAsync(string route, bool animate = false);
    Task GoToAsync(string route, IDictionary<string, object> parameters, bool animate = false);
    Task PopModalAsync(bool animate = false);
}
