# View Implementation Plan: Recipe Generation Modal

## 1. Overview

The Recipe Generation Modal is a modal page responsible for the entire AI-powered recipe creation lifecycle. It is initiated by the user, likely from the main pantry view. Upon opening, it immediately requests a new recipe from the backend. While the recipe is being generated, a loading indicator is displayed. Once the recipe is received, it is rendered from Markdown, and the user is presented with options to either "Accept" or "Reject" the recipe. Accepting saves the recipe to their collection, while rejecting prompts for a reason before discarding it. The modal is then closed, returning the user to the previous screen.

## 2. View Routing

-   **Path**: `src/PantryPal.Mobile/Views/RecipeGenerationPage.xaml`
-   **Navigation**: This view should be presented modally.

## 3. Component Structure

The view is a single `ContentPage` with its layout managed by a `VerticalStackLayout`. The visibility of its children is controlled by the ViewModel's state.

```
RecipeGenerationPage (ContentPage)
└── VerticalStackLayout
    ├── ActivityIndicator (Visible when IsLoading is true)
    ├── ScrollView (Visible when recipe is loaded)
    │   └── Indiko.Maui.Controls.MarkdownView (Renders the recipe)
    └── HorizontalStackLayout (Visible when recipe is loaded)
        ├── Button (UraniumUI, "Reject")
        └── Button (UraniumUI, "Accept")
```

## 4. Component Details

### RecipeGenerationPage

-   **Component description**: A modal `ContentPage` that orchestrates the recipe generation, display, and user decision (accept/reject) flow. Its `BindingContext` will be an instance of `RecipeGenerationViewModel`.
-   **Main elements**:
    -   `<ActivityIndicator>`: Centered on the page, its `IsRunning` and `IsVisible` properties are bound to the `IsLoading` property of the ViewModel.
    -   `<ScrollView>`: Contains the Markdown view to allow scrolling for longer recipes. Its `IsVisible` property is bound to a `ShowRecipeContent` property in the ViewModel.
    -   `<Indiko.Maui.Controls.MarkdownView>`: Renders the recipe text. Its `Markdown` property is bound to the `RecipeText` property of the ViewModel.
    -   `<Button>` (Reject): A UraniumUI button styled for a secondary/destructive action. It is bound to the `RejectCommand`.
    -   `<Button>` (Accept): A UraniumUI button styled as the primary action. It is bound to the `AcceptCommand`.
-   **Handled interactions**:
    -   **Page Appearing**: Triggers the initial recipe generation process.
    -   **Accept Button Click**: Executes the `AcceptCommand` in the ViewModel.
    -   **Reject Button Click**: Executes the `RejectCommand` in the ViewModel, which will trigger a popup.
-   **Handled validation**: This view has no user input, so it performs no validation itself. It relies on the ViewModel to handle API responses, including validation errors from the backend.
-   **Types**: `RecipeGenerationViewModel`
-   **Props**: This is a page, so it does not accept props. It can, however, accept navigation parameters if needed in the future.

## 5. Types

### ViewModel

A new ViewModel is required to manage the state and logic of this view.

**`RecipeGenerationViewModel.cs`**
Located at `src/PantryPal.Mobile/ViewModels/RecipeGenerationViewModel.cs`

```csharp
public class RecipeGenerationViewModel : BaseViewModel // Assumes a BaseViewModel with INotifyPropertyChanged
{
    // --- Properties for UI Binding ---
    
    // Controls the visibility of the loading spinner.
    public bool IsLoading { get; set; }

    // Stores the Markdown recipe content.
    public string RecipeText { get; set; }

    // Computed property to control visibility of recipe content and buttons.
    // True when IsLoading is false and RecipeText is not null or empty.
    public bool ShowRecipeContent { get; }

    // --- Commands ---
    public ICommand PageAppearingCommand { get; }
    public ICommand AcceptCommand { get; }
    public ICommand RejectCommand { get; }

    // --- Private State ---
    private string _generationId; // The unique ID for the generated recipe session.
    private List<RecipeRejectReasonDto> _rejectReasons; // Cached list of rejection reasons.

    // --- Services (Injected) ---
    private readonly IRecipeService _recipeService; // Handles API communication.
    private readonly IDialogService _dialogService; // Abstraction for displaying alerts/popups.
}
```

### DTOs (Data Transfer Objects)

The ViewModel will use existing DTOs from the `PantryPal.Data` project:
-   `RecipeGenerateResponseDto`: For the `/recipes/generate` response.
-   `RecipeAcceptResponseDto`: For the `/recipes/{generationId}/accept` response.
-   `RecipeRejectRequestDto`: For the `/recipes/{generationId}/reject` request body.
-   `RecipeRejectReasonDto`: For the items in the response from `/recipe-reject-reasons`.

## 6. State Management

State is managed entirely within the `RecipeGenerationViewModel`. No external state management is needed.

-   **`IsLoading` (bool)**: Manages the loading state.
    -   Initial state: `true`.
    -   Transitions to `false` when the `/generate` API call succeeds or fails.
-   **`RecipeText` (string)**: Holds the recipe content.
    -   Initial state: `null` or `string.Empty`.
    -   Updated upon successful response from `/generate`.
-   **`_generationId` (string)**: Caches the ID required for subsequent `accept` or `reject` calls. It is set once from the `/generate` response.
-   **`_rejectReasons` (List<RecipeRejectReasonDto>)**: Caches the list of rejection reasons fetched from the API to avoid multiple calls and to populate the rejection dialog.

## 7. API Integration

A new service, `IRecipeService`, should be created in the Mobile project to handle communication with the recipe-related endpoints.

-   **`Task<List<RecipeRejectReasonDto>> GetRejectReasonsAsync()`**
    -   Endpoint: `GET /recipe-reject-reasons`
    -   Response: A list of `RecipeRejectReasonDto`.
-   **`Task<RecipeGenerateResponseDto> GenerateRecipeAsync()`**
    -   Endpoint: `POST /recipes/generate`
    -   Request: No body.
    -   Response: `RecipeGenerateResponseDto`.
-   **`Task<RecipeAcceptResponseDto> AcceptRecipeAsync(string generationId)`**
    -   Endpoint: `POST /recipes/{generationId}/accept`
    -   Request: `generationId` is passed in the URL.
    -   Response: `RecipeAcceptResponseDto`.
-   **`Task RejectRecipeAsync(string generationId, RecipeRejectRequestDto payload)`**
    -   Endpoint: `POST /recipes/{generationId}/reject`
    -   Request: `generationId` in URL, `RecipeRejectRequestDto` as JSON body.
    -   Response: `204 No Content`, so the method can be `Task`.

## 8. User Interactions

-   **User navigates to the modal**: The `PageAppearingCommand` is triggered. The ViewModel sets `IsLoading` to `true`, fetches reject reasons, then calls `GenerateRecipeAsync`.
-   **User clicks "Accept"**: The `AcceptCommand` is triggered. The ViewModel calls `AcceptRecipeAsync` with the cached `_generationId`. On success, it shows a confirmation toast and closes the modal.
-   **User clicks "Reject"**: The `RejectCommand` is triggered. The ViewModel uses a dialog service (e.g., `Page.DisplayActionSheet`) to show the rejection reasons from `_rejectReasons`.
-   **User selects a reason**: The ViewModel identifies the chosen `RecipeRejectReasonDto`, creates a `RecipeRejectRequestDto`, and calls `RejectRecipeAsync`. On success, it closes the modal.

## 9. Conditions and Validation

-   **Pantry/Preferences Status**: The API validates if the user's pantry is empty or if preferences are not set. The ViewModel must handle the `400 Bad Request` response by displaying the specific error message from the API to the user and closing the modal.
-   **Recipe Session Validity**: The ViewModel ensures the `_generationId` is captured and used correctly. If the API returns a `404 Not Found` or `409 Conflict`, it indicates a state mismatch. The ViewModel should inform the user that the session is invalid or already completed and close the modal.

## 10. Error Handling

-   **API Request Failures**: All API calls within the `RecipeService` should be wrapped in `try-catch` blocks.
-   **Specific HTTP Errors**:
    -   `400 Bad Request` on `/generate`: Display the error from the response body (e.g., "Pantry is empty.") and close the view.
    -   `404 Not Found` / `409 Conflict` on `accept`/`reject`: Display a user-friendly message like "This recipe has expired or already been processed." and close the view.
    -   `500 Internal Server Error`: Display a generic error message like "An unexpected error occurred. Please try again later." and close the view.
-   **Network Errors**: Handle `HttpRequestException` by showing a "Please check your network connection" message.

## 11. Implementation Steps

1.  **Project Setup**:
    -   Add the `Indiko.Maui.Controls.MarkdownView` NuGet package to the `PantryPal.Mobile` project.
    -   Register the Markdown View handler in `MauiProgram.cs` as per its documentation.
2.  **Service Layer**:
    -   Define an `IRecipeService` interface in `src/PantryPal.Mobile/Services/`.
    -   Implement the `RecipeService` class, which uses `HttpClient` to communicate with the four required endpoints (`/recipe-reject-reasons`, `/recipes/generate`, `/recipes/{generationId}/accept`, `/recipes/{generationId}/reject`).
    -   Register `IRecipeService` and `RecipeService` for dependency injection in `MauiProgram.cs`.
3.  **ViewModel Creation**:
    -   Create the `RecipeGenerationViewModel.cs` file in `src/PantryPal.Mobile/ViewModels/`.
    -   Implement the properties (`IsLoading`, `RecipeText`, `ShowRecipeContent`).
    -   Inject `IRecipeService` and a dialog service abstraction into the constructor.
    -   Implement the `PageAppearingCommand` to orchestrate the initial data loading and recipe generation.
    -   Implement the `AcceptCommand` and `RejectCommand`. The `RejectCommand` should use the dialog service to present the choices.
4.  **View Creation**:
    -   Create the `RecipeGenerationPage.xaml` and `RecipeGenerationPage.xaml.cs` files in `src/PantryPal.Mobile/Views/`.
    -   In the code-behind (`.xaml.cs`), inject the `RecipeGenerationViewModel` and set it as the `BindingContext`.
    -   In the XAML, lay out the `ActivityIndicator`, `ScrollView`, `MarkdownView`, and `Button`s.
    -   Bind the `IsVisible`, `IsRunning`, `Markdown`, and `Command` properties of the UI elements to the corresponding properties in the `RecipeGenerationViewModel`.
5.  **Navigation**:
    -   Ensure that the page is registered for navigation in `AppShell.xaml.cs` or `MauiProgram.cs` if it's not already.
    -   Update the code that triggers the recipe generation (e.g., a button on the pantry page) to navigate to this new page modally (`Shell.Current.GoToAsync("RecipeGenerationPage")`).
6.  **Testing**:
    -   Run the app and test all user flows: successful generation and accept, successful generation and reject.
    -   Test all documented error conditions by mocking API responses or adjusting backend state (e.g., emptying the pantry, clearing preferences) to ensure they are handled gracefully.
