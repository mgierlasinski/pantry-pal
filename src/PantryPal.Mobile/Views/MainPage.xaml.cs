using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.Views;

public partial class MainPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}