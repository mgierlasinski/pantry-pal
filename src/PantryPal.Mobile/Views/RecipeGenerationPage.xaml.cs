using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.Views;

public partial class RecipeGenerationPage : ContentPage
{
    public RecipeGenerationPage(RecipeGenerationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext is RecipeGenerationViewModel viewModel)
        {
            await viewModel.LoadDataCommand.ExecuteAsync(null);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is RecipeGenerationViewModel viewModel)
        {
            // Execute automatic reject with default reason when back button is pressed
            // This is async but we can't await in a sync method, so we'll fire and forget
            _ = Task.Run(() => MainThread.InvokeOnMainThreadAsync(async () => await viewModel.RejectCommand.ExecuteAsync(null)));

            // Return true to indicate we've handled the back button press
            return true;
        }

        // Fallback to default behavior if no view model
        return base.OnBackButtonPressed();
    }
}

