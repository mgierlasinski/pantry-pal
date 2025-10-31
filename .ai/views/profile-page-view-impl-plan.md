# View Implementation Plan: Profile Page

## 1. Overview

The Profile Page allows users to define and persist their dietary preferences. This includes selecting a diet type, choosing a preferred cuisine, and listing any disliked ingredients. These preferences are crucial as they directly influence the AI-powered recipe generation, ensuring the suggestions are tailored to the user's needs. The view will fetch existing preferences and available options from the API, present them in an editable form, and submit any changes back to the server.

## 2. View Routing

The view will be accessible via standard MAUI navigation. The relevant files will be located at:
-   **View**: `src/PantryPal.Mobile/Views/ProfilePage.xaml`
-   **ViewModel**: `src/PantryPal.Mobile/ViewModels/ProfileViewModel.cs`

## 3. Component Structure

The view will be a `ContentPage` containing a `ScrollView` to ensure content is accessible on all screen sizes. The primary layout will be a `VerticalStackLayout` that organizes the UI controls vertically.

```
ProfilePage (ContentPage)
  - ScrollView
    - VerticalStackLayout
      - ActivityIndicator (Visible during initial data load)
      - PickerField (For Diet Type selection)
      - PickerField (For Preferred Cuisine selection)
      - EditorField (For Disliked Ingredients input)
      - Button (To save preferences)
      - Label (To display status messages, e.g., success or error)
```

## 4. Component Details

### ProfilePage (`ContentPage`)
-   **Component Description**: This is the main container for the user preferences UI. It will be bound to the `ProfileViewModel`.
-   **Main Elements**:
    -   `uranium:PickerField` for "Diet Type": Displays a list of diet types fetched from the API.
    -   `uranium:PickerField` for "Preferred Cuisine": Displays a list of cuisines fetched from the API.
    -   `uranium:EditorField` for "Disliked Ingredients": A multi-line text input for the user to list ingredients they dislike.
    -   `Button` for "Save": A standard button to trigger the save action. Its `IsEnabled` property will be bound to a ViewModel property to prevent multiple submissions.
    -   `ActivityIndicator`: Provides visual feedback when the page is loading initial data.
    -   `Label`: Displays feedback to the user after an action (e.g., "Preferences Saved!").
-   **Handled Interactions**: The view will primarily delegate user interactions to the `ProfileViewModel` through command binding (`OnAppearing` for loading, button `Command` for saving).

### ProfileViewModel
-   **Component Description**: The ViewModel contains the logic for the Profile Page. It manages state, handles API communication, and exposes properties and commands for the View to bind to. It will be implemented using the MVVM Community Toolkit.
-   **Handled Events**:
    -   `PageAppearing`: Triggers a command to load diet types, cuisines, and the user's current preferences from the API.
    -   `SaveButtonClicked`: Triggers a command to validate the user's input and send the updated preferences to the API.
-   **Validation Conditions**:
    -   `DislikedIngredients`: The input string must not exceed 1000 characters. This will be enforced using validation attributes from the MVVM Community Toolkit, and the error will be displayed on the `EditorField`.
-   **Types**:
    -   DTOs: `UserPreferencesDto`, `UserPreferencesCreateDto`, `DietTypeDto`, `PreferredCuisineDto`.
    -   ViewModel Properties: `ObservableCollection<DietTypeDto>`, `ObservableCollection<PreferredCuisineDto>`, and properties for selected items and input strings.

## 5. Types

### DTOs (Data Transfer Objects)
The view will use existing DTOs from the `PantryPal.Data` project for API communication:
-   `UserPreferencesDto`: For receiving the user's current preferences.
-   `UserPreferencesCreateDto(short DietTypeId, short PreferredCuisineId, string? DislikedIngredients)`: For sending created or updated preferences.
-   `DietTypeDto(short Id, string Name)`: Represents a single diet type option.
-   `PreferredCuisineDto(short Id, string Name)`: Represents a single cuisine option.
-   `DietTypesResponseDto(IEnumerable<DietTypeDto> DietTypes)`: Wrapper for the list of diet types.
-   `PreferredCuisinesResponseDto(IEnumerable<PreferredCuisineDto> PreferredCuisines)`: Wrapper for the list of cuisines.

### ViewModel (`ProfileViewModel`)
This new class will be created with the following properties:
-   `ObservableCollection<DietTypeDto> DietTypes { get; }`: Holds the list of diet types for the picker.
-   `[ObservableProperty] DietTypeDto selectedDietType`: The currently selected diet type.
-   `ObservableCollection<PreferredCuisineDto> PreferredCuisines { get; }`: Holds the list of cuisines for the picker.
-   `[ObservableProperty] PreferredCuisineDto selectedPreferredCuisine`: The currently selected cuisine.
-   `[ObservableProperty] [MaxLength(1000)] string dislikedIngredients`: The user-entered text for disliked ingredients.
-   `[ObservableProperty] bool isLoading`: Controls the visibility of the loading indicator.
-   `[ObservableProperty] string statusMessage`: A message displayed to the user.
-   `IAsyncRelayCommand LoadPreferencesCommand { get; }`: Command to fetch all initial data.
-   `IAsyncRelayCommand SavePreferencesCommand { get; }`: Command to validate and save preferences.

## 6. State Management

State will be managed entirely within the `ProfileViewModel`. No external state management library is required.
-   **Loading State**: An `isLoading` boolean property will be used. It will be set to `true` when `LoadPreferencesCommand` begins and `false` when it completes or fails. The View's `ActivityIndicator` and form controls will be bound to this property to show a loading spinner and disable input.
-   **Form State**: The ViewModel will hold the user's selections and input in its observable properties (`SelectedDietType`, `SelectedPreferredCuisine`, `DislikedIngredients`).
-   **Submission State**: The `SavePreferencesCommand` from the MVVM Community Toolkit has a built-in `IsRunning` property, which will be used to disable the "Save" button while a request is in progress, preventing duplicate submissions.

## 7. API Integration

The `ProfileViewModel` will depend on services to interact with the API. These services will be injected via dependency injection.

-   **Initial Load (`LoadPreferencesCommand`)**:
    1.  `GET /diet-types`: Fetches the list of all available diet types. Response type is `DietTypesResponseDto`.
    2.  `GET /preferred-cuisines`: Fetches the list of all available cuisines. Response type is `PreferredCuisinesResponseDto`.
    3.  `GET /user-preferences`: Fetches the current user's saved preferences. Response type is `UserPreferencesDto`. If it returns a 404 error, it means the user has not set their preferences yet.

-   **Save Preferences (`SavePreferencesCommand`)**:
    1.  `POST /user-preferences`: Creates or updates the user's preferences. The request body will be of type `UserPreferencesCreateDto`, constructed from the ViewModel's state. The response will be the updated `UserPreferencesDto`.

## 8. User Interactions

-   **User opens the page**: The `LoadPreferencesCommand` is triggered automatically. An `ActivityIndicator` is shown while data is fetched.
-   **User selects a Diet Type**: The `PickerField` updates the `SelectedDietType` property in the ViewModel.
-   **User selects a Preferred Cuisine**: The `PickerField` updates the `SelectedPreferredCuisine` property in the ViewModel.
-   **User types in Disliked Ingredients**: The `EditorField` updates the `DislikedIngredients` property in the ViewModel.
-   **User clicks "Save"**:
    -   The `SavePreferencesCommand` is executed.
    -   The button becomes disabled.
    -   Input is validated. If invalid, an error message is shown next to the `EditorField`.
    -   If valid, an API call is made to `POST /user-preferences`.
    -   On success, a confirmation message is displayed in the `StatusMessage` label (e.g., "Preferences saved successfully!").
    -   On failure, an error message is displayed.
    -   The button is re-enabled.

## 9. Conditions and Validation

-   **API Data Loading**: The form will be disabled until the initial data (diet types, cuisines) has been successfully loaded to prevent interaction with an incomplete UI.
-   **Input Validation**:
    -   **Disliked Ingredients Length**: The `DislikedIngredients` property in the ViewModel will be decorated with `[MaxLength(1000)]`. The `EditorField` in the view will be configured to display the validation error message from the ViewModel. The "Save" button's command will check for validation errors before executing.
    -   **Required Selections**: The `SavePreferencesCommand` will verify that both `SelectedDietType` and `SelectedPreferredCuisine` are not null before making the API call.

## 10. Error Handling

-   **API Fetch Errors**: If any of the initial `GET` requests fail, the `IsLoading` state will be set to `false`, and the `StatusMessage` label will display a user-friendly error (e.g., "Could not load preferences. Please try again later."). A "Retry" button could be shown.
-   **API Save Errors**: If the `POST /user-preferences` call fails, the error will be caught, and the `StatusMessage` label will display a relevant error (e.g., "Failed to save preferences. Please check your connection and try again.").
-   **Unauthorized (401) Errors**: The underlying HTTP client service should be configured to handle 401 responses globally by navigating the user to the login page.
-   **Not Found (404) on GET /user-preferences**: This is not an error condition. It will be handled gracefully by presenting the user with an empty form to fill in their preferences for the first time.

## 11. Implementation Steps

1.  **Create ViewModel**: Create `ProfileViewModel.cs` in `src/PantryPal.Mobile/ViewModels/`.
    -   Inherit from `ObservableValidator` from the MVVM Community Toolkit.
    -   Define the observable properties for state (`IsLoading`, `SelectedDietType`, etc.) and the `[MaxLength(1000)]` validation attribute.
    -   Define `ObservableCollection` properties for `DietTypes` and `PreferredCuisines`.
2.  **Inject Services**: Inject `IUserPreferencesService`, `IDietTypesService`, and `IPreferredCuisinesService` into the `ProfileViewModel` constructor.
3.  **Implement Load Command**: Create the `LoadPreferencesCommand`. Inside, implement the logic to call the three GET endpoints in parallel, populate the collections, handle the 404 case for user preferences, and set the `IsLoading` state appropriately.
4.  **Implement Save Command**: Create the `SavePreferencesCommand`. Implement the logic to first validate the model, then construct a `UserPreferencesCreateDto` and call the `upsert` service method. Handle success and error cases by updating the `StatusMessage`.
5.  **Create View**: Create `ProfilePage.xaml` in `src/PantryPal.Mobile/Views/`.
    -   Set the `BindingContext` to an instance of `ProfileViewModel`.
    -   Lay out the UI using UraniumUI's `PickerField` and `EditorField`, and a standard `Button`.
    -   Bind the `ItemsSource` and `SelectedItem` of the pickers to the corresponding ViewModel properties.
    -   Bind the `Text` of the `EditorField` and configure it to show validation errors.
    -   Bind the `Command` of the "Save" button and its `IsEnabled` property to `!SavePreferencesCommand.IsRunning`.
    -   Add an `ActivityIndicator` bound to the `IsLoading` property.
6.  **Register DI**: Register the `ProfilePage` and `ProfileViewModel` for dependency injection in `MauiProgram.cs`.
7.  **Add Navigation**: Ensure there is a way to navigate to the `ProfilePage` from elsewhere in the app (e.g., a settings icon or a tab).
8.  **Test**: Manually test all user interaction flows, including the first-time user experience (404 case), loading, saving, validation, and error states.
