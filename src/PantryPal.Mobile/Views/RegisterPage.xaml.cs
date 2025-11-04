using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
