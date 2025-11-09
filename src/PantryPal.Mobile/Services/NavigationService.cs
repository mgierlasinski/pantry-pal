namespace PantryPal.Mobile.Services;

public class NavigationService : INavigationService
{
    public async Task GoToAsync(string route, bool animate = false)
    {
        await Shell.Current.GoToAsync(route, animate);
    }

    public async Task GoToAsync(string route, IDictionary<string, object> parameters, bool animate = false)
    {
        await Shell.Current.GoToAsync(route, animate, parameters);
    }

    public async Task PopModalAsync(bool animate = false)
    {
        await Shell.Current.Navigation.PopModalAsync(animate);
    }
}
