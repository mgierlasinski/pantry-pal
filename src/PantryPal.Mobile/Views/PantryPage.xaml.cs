using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.Views;

public partial class PantryPage : ContentPage
{
    public PantryPage(PantryPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is PantryPageViewModel viewModel)
        {
            await viewModel.LoadItemsCommand.ExecuteAsync(null);
        }
    }
}

