using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryPal.Mobile.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PantryPal.Mobile.ViewModels;

[QueryProperty(nameof(Recipe), "Recipe")]
public partial class RecipeDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private SavedRecipeItemViewModel _recipe;

    [ObservableProperty]
    private string _recipeTitle = "Recipe";

    [ObservableProperty]
    private string _recipeMarkdownContent = "Loading recipe...";

    partial void OnRecipeChanged(SavedRecipeItemViewModel value)
    {
        if (value != null)
        {
            RecipeMarkdownContent = value.RecipeText;
            RecipeTitle = ExtractTitleFromMarkdown(value.RecipeText) ?? "Recipe";
        }
        else
        {
            RecipeMarkdownContent = "Error: Could not load recipe.";
        }
    }

    private string? ExtractTitleFromMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return null;

        var firstLine = markdown.Split('\n').FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));
        return firstLine?.Trim().TrimStart('#').Trim();
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
