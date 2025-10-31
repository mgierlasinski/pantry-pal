using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is ProfileViewModel viewModel)
        {
            await viewModel.LoadPreferencesCommand.ExecuteAsync(null);
        }
    }
}
