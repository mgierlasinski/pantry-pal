using CommunityToolkit.Mvvm.ComponentModel;
using PantryPal.Data;
using System.Windows.Input;

namespace PantryPal.Mobile.Models;

/// <summary>
/// ViewModel representing a single saved recipe item in the UI
/// </summary>
public partial class SavedRecipeItemViewModel : ObservableObject
{
    /// <summary>
    /// The recipe's unique ID
    /// </summary>
    [ObservableProperty]
    private string _id = string.Empty;

    /// <summary>
    /// The extracted title of the recipe (e.g., from the first line of the Markdown)
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// A formatted, user-friendly date string derived from CreatedAt
    /// </summary>
    [ObservableProperty]
    private string _savedDate = string.Empty;

    /// <summary>
    /// Command to delete this recipe item
    /// </summary>
    public ICommand? DeleteCommand { get; set; }

    /// <summary>
    /// Constructor that takes a RecipeDto and performs the necessary mapping
    /// </summary>
    public SavedRecipeItemViewModel(RecipeDto recipeDto)
    {
        ArgumentNullException.ThrowIfNull(recipeDto);

        Id = recipeDto.Id;
        Title = ExtractTitle(recipeDto.RecipeText);
        SavedDate = FormatSavedDate(recipeDto.CreatedAt);
    }

    /// <summary>
    /// Extracts the title from the recipe text (first line of Markdown)
    /// </summary>
    private static string ExtractTitle(string recipeText)
    {
        if (string.IsNullOrWhiteSpace(recipeText))
            return "Untitled Recipe";

        var lines = recipeText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstLine = lines.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(firstLine))
            return "Untitled Recipe";

        // Remove common Markdown heading prefixes (# ## ###)
        var title = firstLine.TrimStart('#', ' ').Trim();

        return string.IsNullOrWhiteSpace(title) ? "Untitled Recipe" : title;
    }

    /// <summary>
    /// Formats the CreatedAt timestamp into a user-friendly date string
    /// </summary>
    private static string FormatSavedDate(string createdAt)
    {
        if (string.IsNullOrWhiteSpace(createdAt))
            return "Unknown date";

        try
        {
            if (DateTime.TryParse(createdAt, out var dateTime))
            {
                var now = DateTime.Now;
                var diff = now - dateTime;

                if (diff.TotalDays < 1)
                {
                    return dateTime.ToString("h:mm tt");
                }
                else if (diff.TotalDays < 7)
                {
                    return dateTime.ToString("ddd");
                }
                else if (diff.TotalDays < 365)
                {
                    return dateTime.ToString("MMM d");
                }
                else
                {
                    return dateTime.ToString("MMM d, yyyy");
                }
            }
        }
        catch
        {
            // If parsing fails, return the original string
        }

        return createdAt;
    }
}
