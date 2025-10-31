using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.Views;

public partial class SavedRecipesPage : ContentPage
{
    public SavedRecipesPage(SavedRecipesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is SavedRecipesViewModel viewModel)
        {
            await viewModel.LoadItemsCommand.ExecuteAsync(null);
        }
    }
}
