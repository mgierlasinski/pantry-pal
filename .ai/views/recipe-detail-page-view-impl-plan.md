# View Implementation Plan: Recipe Detail Page

## 1. Overview
This document outlines the implementation plan for the `RecipeDetailPage`. This view will function as a modal page, responsible for displaying the full, formatted content of a single saved recipe. The recipe content will be provided in Markdown format. The primary user interaction is viewing the content and closing the modal to return to the previous screen.

## 2. View Routing
- **Path:** `src/PantryPal.Mobile/Views/RecipeDetailPage.xaml`
- **Registration:** The view and its corresponding view model must be registered in `MauiProgram.cs` for dependency injection and routing.
```csharp
// In MauiProgram.cs
builder.Services.AddTransient<RecipeDetailViewModel>();
builder.Services.AddTransient<RecipeDetailPage>();

// In AppShell.xaml.cs or a dedicated routing class
Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
```

## 3. Component Structure
The view will have a simple, hierarchical structure contained within a `ContentPage`.

```
RecipeDetailPage (ContentPage)
└── Grid (defines layout with a scrollable area and a fixed button)
    ├── ScrollView
    │   └── Indiko.Maui.Controls.MarkdownView (displays recipe markdown)
    └── Button (UraniumUI, for closing the modal)
```

## 4. Component Details

### RecipeDetailPage (`ContentPage`)
- **Component description:** The main container for the view. It sets the page title and hosts the layout grid for its content. It will be bound to the `RecipeDetailViewModel`.
- **Main elements:** A `Grid` with two rows: one for the scrollable content and a smaller, fixed-size row for the close button.
- **Handled interactions:** None directly. It delegates all logic and user interactions to its `ViewModel`.
- **Types:** Binds to `RecipeDetailViewModel`.
- **Props:** None.

### RecipeDetailViewModel (`ObservableObject`)
- **Component description:** The backing view model for the page. It receives the selected recipe object via navigation parameters, extracts the necessary data for display, and provides a command for closing the view.
- **Handled interactions:**
    - `Close`: Executes a command to navigate back (`..`).
- **Handled validation:**
    - Checks if the recipe object received via navigation is valid and not null.
    - Checks if the recipe's Markdown content is not null or empty before displaying it.
- **Types:** `RecipeDetailViewModel`, `SavedRecipeItemViewModel` (as input), `ICommand`.
- **Props:** Does not accept props directly, but receives data via the `IQueryAttributable` interface.

### Indiko.Maui.Controls.MarkdownView
- **Component description:** A third-party control responsible for parsing and rendering the Markdown recipe text into a readable format.
- **Main elements:** N/A (internal to the control).
- **Handled interactions:** None.
- **Types:** Binds to a `string` property (`RecipeMarkdownContent`) on the `ViewModel`.
- **Props:**
    - `Markdown`: The string containing the Markdown text to render.

### UraniumUI Button
- **Component description:** A styled button that allows the user to dismiss the modal view.
- **Main elements:** Standard button with text ("Close" or similar).
- **Handled interactions:**
    - `Clicked`: Triggers the `CloseCommand` on the `ViewModel`.
- **Types:** Binds to `ICommand` (`CloseCommand`).
- **Props:**
    - `Text`: "Close".
    - `Command`: ` {Binding CloseCommand}`.

## 5. Types
### ViewModel: `RecipeDetailViewModel.cs`
A new view model class needs to be created to manage the view's state and logic.

```csharp
// Located at: src/PantryPal.Mobile/ViewModels/RecipeDetailViewModel.cs
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

    private string ExtractTitleFromMarkdown(string markdown)
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
```

## 6. State Management
State management is self-contained within the `RecipeDetailViewModel`. The view model is initialized with data passed via navigation parameters.

- **`Recipe`**: An `ObservableProperty` of type `SavedRecipeItemViewModel` that holds the full recipe object. It's decorated with the `QueryProperty` attribute to receive data from the navigation service.
- **`RecipeTitle`**: An `ObservableProperty` that holds the title extracted from the Markdown. It is bound to the `ContentPage.Title`.
- **`RecipeMarkdownContent`**: An `ObservableProperty` that holds the raw Markdown string. It is bound to the `MarkdownView` control.

When the `Recipe` property is set by the navigation system, the `OnRecipeChanged` partial method automatically updates the `RecipeTitle` and `RecipeMarkdownContent` properties, which in turn updates the UI.

## 7. API Integration
The `RecipeDetailPage` does not directly integrate with any API endpoints. It operates on data that was previously fetched by the `SavedRecipesViewModel` (from the `GET /recipes` endpoint) and passed to it during navigation.

- **Data Flow:**
  1. `SavedRecipesViewModel` fetches a list of `RecipeDto` objects.
  2. The user selects a recipe.
  3. `SavedRecipesViewModel` initiates navigation to `RecipeDetailPage`, passing the selected `SavedRecipeItemViewModel`.
  4. `RecipeDetailViewModel` receives the object via the `QueryProperty` attribute.

## 8. User Interactions
- **View Recipe:** The user implicitly triggers this by navigating from the saved recipes list. The view loads and displays the recipe content automatically.
- **Close View:** The user clicks the "Close" button.
  - **Handler:** The `CloseCommand` in `RecipeDetailViewModel` is executed.
  - **Action:** The command calls `Shell.Current.GoToAsync("..")`, which pops the modal page from the navigation stack and returns the user to the saved recipes list.

## 9. Conditions and Validation
- **Navigation Data:** The primary condition is the presence and validity of the `Recipe` object passed during navigation.
  - **Component:** `RecipeDetailViewModel`.
  - **Verification:** The `OnRecipeChanged` method checks if the received `value` is not `null`.
  - **Effect:** If the data is invalid or `null`, the UI will display an error message ("Error: Could not load recipe.") within the `MarkdownView`.

## 10. Error Handling
- **Missing Navigation Parameter:** If the `Recipe` object is not passed correctly during navigation, the `Recipe` property in the view model will be `null`. The `OnRecipeChanged` handler will detect this and set the `RecipeMarkdownContent` to an error message, informing the user that the recipe could not be loaded.
- **Empty Recipe Content:** If the `Recipe.RecipeText` is null or empty, the `ExtractTitleFromMarkdown` will return null (falling back to the default "Recipe" title), and the `MarkdownView` will display an empty string, effectively showing a blank content area. The UI will not crash.

## 11. Implementation Steps
1. **Create ViewModel:** Create the `RecipeDetailViewModel.cs` file in the `src/PantryPal.Mobile/ViewModels/` directory with the code specified in the **Types** section.
2. **Create View:** Create the `RecipeDetailPage.xaml` and `RecipeDetailPage.xaml.cs` files in the `src/PantryPal.Mobile/Views/` directory.
3. **Implement View (XAML):**
    - Set the `x:Class` and `xmlns` namespaces.
    - Add the necessary `ContentPage` resources (e.g., `Color` definitions).
    - Bind the `Title` of the `ContentPage` to `RecipeTitle`.
    - Implement the `Grid` layout with a `ScrollView` and `MarkdownView` for content, and a `Button` for closing.
    - Bind the `MarkdownView`'s `Markdown` property to `RecipeMarkdownContent`.
    - Bind the `Button`'s `Command` to `CloseCommand`.
4. **Implement View (Code-behind):**
    - In `RecipeDetailPage.xaml.cs`, inject the `RecipeDetailViewModel` and set it as the `BindingContext`.
5. **Register for DI and Routing:**
    - In `MauiProgram.cs`, register `RecipeDetailPage` and `RecipeDetailViewModel` as transient services.
    - In `AppShell.xaml.cs` (or your routing configuration), register the route for `RecipeDetailPage`.
6. **Update Navigation:**
    - In `SavedRecipesViewModel`, modify the command that handles recipe selection to navigate to `RecipeDetailPage` and pass the selected `SavedRecipeItemViewModel` as a navigation parameter.
    ```csharp
    // Example in SavedRecipesViewModel
    await Shell.Current.GoToAsync(nameof(RecipeDetailPage), new Dictionary<string, object>
    {
        { "Recipe", selectedRecipeItem }
    });
    ```
7. **Testing:**
    - Run the application and navigate to the saved recipes list.
    - Tap on a recipe to ensure the detail modal opens correctly.
    - Verify that the recipe title and Markdown content are displayed as expected.
    - Confirm that the "Close" button dismisses the modal and returns to the list.
    - Test with recipes that have empty or null content to verify error handling.
