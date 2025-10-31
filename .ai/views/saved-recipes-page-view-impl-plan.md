# View Implementation Plan: Saved Recipes Page

## 1. Overview
The Saved Recipes Page displays a list of recipes that the user has previously generated and saved. The primary purpose is to allow users to view and manage their saved recipes. Key features include a chronological list of recipes, infinite scrolling for pagination, and a swipe-to-delete functionality for managing the list. The view will also handle empty and error states gracefully.

## 2. View Routing
The view will be accessible at the path `src/PantryPal.Mobile/Views/SavedRecipesPage.xaml`. It should be registered in `AppShell.xaml` to be navigable within the application, likely as a tab in the main tab bar.

## 3. Component Structure
The view will be composed of a main page containing a `CollectionView` to list the recipes. Each item in the list will be a `SwipeView` to allow for the delete action.

```
SavedRecipesPage (.xaml)
  └── Bindings: SavedRecipesViewModel (.cs)
      └── CollectionView
          ├── ItemsSource: ObservableCollection<SavedRecipeItemViewModel>
          ├── RemainingItemsThresholdReached -> LoadMoreItemsCommand
          ├── EmptyView
          │   └── Label (e.g., "You haven't saved any recipes yet.")
          └── ItemTemplate (DataTemplate)
              └── SwipeView
                  ├── RightItems -> Delete Action
                  │   └── SwipeItem (Invoked -> DeleteRecipeCommand)
                  └── Content
                      └── VerticalStackLayout
                          ├── Label (Text: RecipeTitle)
                          └── Label (Text: SavedDate)
```

## 4. Component Details

### SavedRecipesPage.xaml
-   **Component description:** The main container for the view. It sets up the page layout, title, and binds the `CollectionView` to the `SavedRecipesViewModel`. It will also manage visual states like loading.
-   **Main elements:** A `ContentPage` containing a `CollectionView`. A `ActivityIndicator` can be used for the initial loading state, bound to the `IsLoading` property of the ViewModel.
-   **Handled interactions:** `OnNavigatedTo` page lifecycle event, which triggers the initial data load via a command in the ViewModel.
-   **Handled validation:** None.
-   **Types:** `SavedRecipesViewModel`.
-   **Props:** None.

### CollectionView
-   **Component description:** The core component for displaying the list of recipes. It will be configured for infinite scrolling.
-   **Main elements:** `CollectionView` with its `ItemsSource` bound to the `Recipes` collection in the ViewModel. It will contain an `ItemTemplate` and an `EmptyView`.
-   **Handled interactions:** `RemainingItemsThresholdReached` event is bound to the `LoadMoreItemsCommand` to fetch subsequent pages of data.
-   **Handled validation:** The `LoadMoreItemsCommand` will only execute if not all items have been loaded.
-   **Types:** `ObservableCollection<SavedRecipeItemViewModel>`.
-   **Props:** `ItemsSource`, `RemainingItemsThreshold`, `RemainingItemsThresholdReachedCommand`, `EmptyView`.

### Recipe List Item (DataTemplate)
-   **Component description:** Defines the visual representation of a single recipe in the list. It includes the recipe's title, saved date, and the swipe-to-delete functionality.
-   **Main elements:** A `SwipeView` wrapping a `Grid` or `VerticalStackLayout` that contains two `Label` elements for the title and date. The `SwipeView.RightItems` will contain a `SwipeItem` for the delete action.
-   **Handled interactions:** The `SwipeItem`'s `Invoked` event is bound to the `DeleteRecipeCommand` in the ViewModel.
-   **Handled validation:** None.
-   **Types:** `SavedRecipeItemViewModel`.
-   **Props:** The component is a `DataTemplate`, so it receives its `BindingContext` from the `CollectionView`'s `ItemsSource`.

## 5. Types

### DTOs (from `PantryPal.Data`)
-   **`RecipeDto`**: Represents a recipe record from the API.
    -   `Id` (string): Unique identifier.
    -   `RecipeText` (string): The full recipe content in Markdown.
    -   `CreatedAt` (string): The timestamp when the recipe was saved.
    -   `UpdatedAt` (string): The timestamp of the last update.
-   **`RecipesPaginatedResponseDto`**: The wrapper object for the paginated recipe list response.
    -   `Items` (IEnumerable<`RecipeDto`>): The list of recipes for the current page.
    -   `Page` (int): The current page number.
    -   `PageSize` (int): The number of items per page.
    -   `Total` (int): The total number of saved recipes available on the server.

### ViewModels
-   **`SavedRecipesViewModel`**: The main ViewModel for the view.
    -   `Recipes` (ObservableCollection<`SavedRecipeItemViewModel`>): The collection of recipe items displayed in the list.
    -   `IsLoading` (bool): Indicates if the initial data is being loaded.
    -   `IsLoadingMore` (bool): Indicates if a subsequent page is being loaded.
    -   `IsBusy` (bool): General flag to prevent concurrent data operations.
    -   `PageAppearingCommand` (ICommand): Command to load the first page of recipes.
    -   `LoadMoreItemsCommand` (ICommand): Command to load the next page of recipes.
    -   `DeleteRecipeCommand` (ICommand): Command to delete a selected recipe.
-   **`SavedRecipeItemViewModel`**: Represents a single recipe item in the UI.
    -   `Id` (string): The recipe's unique ID.
    -   `Title` (string): The extracted title of the recipe (e.g., from the first line of the Markdown).
    -   `SavedDate` (string): A formatted, user-friendly date string derived from `CreatedAt`.

## 6. State Management
State will be managed entirely within the `SavedRecipesViewModel` using the `CommunityToolkit.Mvvm` library.
-   **`_currentPage`**: An `int` field to track the current page for API requests.
-   **`_totalItems`**: An `int` field to store the total number of recipes to determine if more pages can be loaded.
-   **`IsBusy`**: A boolean property to prevent multiple commands from running simultaneously (e.g., loading more items while a delete is in progress). All commands should check `!IsBusy` before executing.
-   The `Recipes` list is an `ObservableCollection`, so any additions or removals will automatically update the UI.

No custom hooks are required; standard MVVM patterns suffice.

## 7. API Integration
The `SavedRecipesViewModel` will interact with the API via an injected `IRecipeService`.

-   **Fetching Recipes:**
    -   **Action:** `PageAppearingCommand` and `LoadMoreItemsCommand`.
    -   **Endpoint:** `GET /recipes`
    -   **Request:** The service will be called with pagination parameters: `page` (from the `_currentPage` state) and `pageSize` (e.g., 20).
    -   **Response Type:** `RecipesPaginatedResponseDto`. The ViewModel will map the `RecipeDto` items into `SavedRecipeItemViewModel` instances and add them to the `Recipes` collection.

-   **Deleting a Recipe:**
    -   **Action:** `DeleteRecipeCommand`.
    -   **Endpoint:** `DELETE /recipes/{id}`
    -   **Request:** The service will be called with the `Id` of the `SavedRecipeItemViewModel` to be deleted.
    -   **Response:** `204 No Content` on success. The ViewModel will then remove the corresponding item from the `Recipes` collection.

## 8. User Interactions
-   **Navigate to Page:** User navigates to the saved recipes view. The `PageAppearingCommand` fires, showing a loading indicator and fetching the first page of recipes.
-   **Scroll to Bottom:** User scrolls to the end of the list. The `LoadMoreItemsCommand` fires, fetching and appending the next page of recipes. A loading indicator appears at the bottom of the list during the fetch.
-   **Swipe and Delete:** User swipes an item to reveal the delete button and taps it.
    -   A confirmation dialog is displayed.
    -   If confirmed, the `DeleteRecipeCommand` is executed, the item is removed from the UI, and the API call is made.

## 9. Conditions and Validation
-   **Load More:** The `LoadMoreItemsCommand` will have a `CanExecute` condition that returns `false` if `IsBusy` is true or if `Recipes.Count >= _totalItems`, preventing unnecessary API calls.
-   **Delete:** The `DeleteRecipeCommand` will have a `CanExecute` condition that returns `false` if `IsBusy` is true. It will also require a confirmation from the user via a dialog before proceeding.

## 10. Error Handling
-   **Initial Load Failure:** If the first `GET /recipes` call fails, the loading indicator is hidden, and an error message with a "Retry" button is displayed in place of the list.
-   **Load More Failure:** If a subsequent `GET /recipes` call fails, a non-intrusive toast message is shown (e.g., "Failed to load more recipes"). The `_currentPage` counter is not incremented, allowing the user to trigger the load again by scrolling.
-   **Delete Failure (Server Error):** If the `DELETE /recipes/{id}` call fails with a 5xx error, a toast message is shown ("Failed to delete. Please try again."). The item remains in the UI.
-   **Delete Failure (Not Found):** If the delete call fails with a 404 error, it means the item was already deleted. The item is removed from the UI to synchronize the state, and a toast can optionally inform the user.

## 11. Implementation Steps
1.  **Create `SavedRecipeItemViewModel.cs`**: In the `ViewModels` folder, create a new `ObservableObject` class. It should have `Id`, `Title`, and `SavedDate` properties. Its constructor will take a `RecipeDto` and perform the necessary mapping (e.g., extracting the title from `RecipeText`).
2.  **Create `SavedRecipesViewModel.cs`**: In the `ViewModels` folder, create the main ViewModel inheriting from `ObservableObject`.
    -   Inject `IRecipeService` in the constructor.
    -   Define the `ObservableProperty` fields: `Recipes`, `IsLoading`, `IsLoadingMore`, `IsBusy`.
    -   Implement the `PageAppearingCommand`, `LoadMoreItemsCommand`, and `DeleteRecipeCommand` using the `[RelayCommand]` attribute. Add the logic for API calls, state management, and error handling as described above.
3.  **Create `SavedRecipesPage.xaml`**: In the `Views` folder, create a new `ContentPage`.
    -   Set its `BindingContext` to `SavedRecipesViewModel`.
    -   Add a `CollectionView` and bind its `ItemsSource` to the `Recipes` property.
    -   Configure the `RemainingItemsThreshold` and bind the `RemainingItemsThresholdReachedCommand`.
    -   Define the `CollectionView.ItemTemplate` with a `SwipeView` and labels bound to the properties of `SavedRecipeItemViewModel`.
    -   Implement the `SwipeItem` for the delete action and bind it to the `DeleteRecipeCommand`.
    -   Define the `CollectionView.EmptyView` with a user-friendly message.
4.  **Register View and ViewModel**: In `MauiProgram.cs`, register `SavedRecipesPage` and `SavedRecipesViewModel` for dependency injection.
5.  **Add Navigation**: In `AppShell.xaml` or another appropriate navigation location, add an entry for `SavedRecipesPage` so that users can navigate to it.
6.  **Test**: Thoroughly test all functionalities: initial load, infinite scroll, deleting items, empty state, and error handling for all API calls.
