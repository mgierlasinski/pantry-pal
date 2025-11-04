using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
