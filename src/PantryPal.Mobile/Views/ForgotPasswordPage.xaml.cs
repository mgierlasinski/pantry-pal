using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.Views;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage(ForgotPasswordPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
