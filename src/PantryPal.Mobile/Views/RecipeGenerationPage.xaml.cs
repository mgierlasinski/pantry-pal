using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.Views;

public partial class RecipeGenerationPage : ContentPage
{
    private readonly RecipeGenerationViewModel _viewModel;

    public RecipeGenerationPage(RecipeGenerationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        _viewModel.LoadDataAsync();
    }
}

