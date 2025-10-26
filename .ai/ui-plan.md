# UI Architecture for PantryPal

## 1. UI Structure Overview

The PantryPal mobile application uses MAUI Shell with a bottom TabBar to organize three primary sections: Pantry, Saved Recipes, and Profile. Authentication, recipe generation, and detail views are implemented as modal pages. All layouts leverage Grid definitions for responsive UI. SecureStorage and a delegated HttpClient handler manage authentication tokens. Error handling, accessibility attributes, and global styles ensure a robust user experience.

## 2. View List

- **Pantry Page**  
  - View path: `src/PantryPal.Mobile/Views/PantryPage.xaml`  
  - Purpose: Display and manage the user's pantry items and trigger recipe generation.  
  - Key information: List of items (name, favorite flag, swipe-to-delete action), “Add” button, “Generate Recipe” button, empty state with CTA.  
  - Key components: `CollectionView` with incremental loading, `Button`, `EmptyView` (Material icon + CTA).  
  - Considerations: SemanticProperties.Name, AutomationId for list items and actions; handle 401 redirect to Login.

- **Recipe Generation Modal**  
  - View path: `src/PantryPal.Mobile/Views/RecipeGenerationPage.xaml`  
  - Purpose: Invoke AI recipe generation and display results.  
  - Key information: Loading spinner; recipe content in Markdown; Accept and Reject controls.  
  - Key components: `ActivityIndicator` bound to `IsLoading`, `Indiko.Maui.Controls.MarkdownView`, UraniumUI `Button`.  
  - On tapping Reject: display a popup dialog with three buttons for rejection reasons ([I don’t have these ingredients], [I don’t like this dish], [Other]), log selection, then close popup and return to modal or dismiss.
  - Considerations: Accessible labels and hints; focus management; map API errors to user alerts.

- **Saved Recipes Page**  
  - View path: `src/PantryPal.Mobile/Views/SavedRecipesPage.xaml`  
  - Purpose: List user’s saved recipes with pagination and deletion.  
  - Key information: Recipe title, saved timestamp, swipe-to-delete action.  
  - Key components: `CollectionView` with `RemainingItemsThreshold`, `SwipeView` for delete, `EmptyView`.  
  - Considerations: Provide semantic names; confirm deletion; handle API 404/500 responses gracefully.

- **Recipe Detail Modal**  
  - View path: `src/PantryPal.Mobile/Views/RecipeDetailPage.xaml`  
  - Purpose: Show full recipe content in Markdown.  
  - Key information: Recipe Markdown text; close button.  
  - Key components: `Indiko.Maui.Controls.MarkdownView`, UraniumUI `Button`.  
  - Considerations: Ensure high contrast; support screen readers via SemanticProperties.

- **Profile Page**  
  - View path: `src/PantryPal.Mobile/Views/ProfilePage.xaml`  
  - Purpose: Create or update user dietary preferences.  
  - Key information: DietType picker, PreferredCuisine picker, disliked ingredients text editor, Save button.  
  - Key components: UraniumUI `Picker`, `EditorField`, `Button`.  
  - Considerations: Validate inputs (max lengths); display inline error messages; secure 401 handling.

- **Login Modal**  
  - View path: `src/PantryPal.Mobile/Views/LoginPage.xaml`  
  - Purpose: Authenticate the user via email/password.  
  - Key information: Email and password fields; Login button; error alerts.  
  - Key components: UraniumUI `TextField`, `PasswordField`, `Button`.  
  - Considerations: Masked input; SemanticProperties for assistive tech; store JWT in SecureStorage.

## 3. User Journey Map

1. App launch → if unauthenticated, display **Login Modal**.  
2. On successful login, navigate to **Pantry Page** within Shell.  
3. On **Pantry Page**, user sees items or empty state.  
   - Empty: CTA prompts to add first item.  
   - Non-empty: view favorite and non-favorite items; tap “Generate Recipe”.  
4. **Recipe Generation Modal** opens: spinner → AI returns Markdown recipe.  
   - Accept → 201 saved, switch to **Saved Recipes Page**.  
   - Reject → select reason via radio buttons → log reason → return to generation or close.  
5. In **Saved Recipes Page**, scroll loads more or tap a recipe → open **Recipe Detail Modal**.  
6. Navigate to **Profile Page** via TabBar to update preferences; save and reflect in future generations.

## 4. Layout and Navigation Structure

- **Shell (AppShell.xaml)** with `<TabBar>`:  
  • Tab “Pantry” → `//PantryPage`  
  • Tab “Saved” → `//SavedRecipesPage`  
  • Tab “Profile” → `//ProfilePage`  
- **Modal routes** registered in Shell:  
  • `///LoginPage`  
  • `///RecipeGenerationPage`  
  • `///RecipeDetailPage`  
- Navigation methods: `GoToAsync("///RecipeGenerationPage")`, `GoToAsync("//SavedRecipesPage")`, handle 401 by redirecting to `///LoginPage`.

## 5. Key Components

- `CollectionView` with `RemainingItemsThreshold` for incremental loading.  
- UraniumUI form controls: `TextField`, `EditorField`, `Picker`, `Button`.  
- `Indiko.Maui.Controls.MarkdownView` for recipe rendering.  
- `ActivityIndicator` bound to `IsLoading`/`IsBusy`.  
- SemanticProperties.Name/Hint and `AutomationId` for accessibility.  
- Global ResourceDictionaries (`Colors.xaml`, `Styles.xaml`, `Templates.xaml`) in `App.xaml`.  
- SecureStorage for JWT plus HTTP message handler for Bearer tokens.

> **Note:** All UI text is routed through `Resources.resx` for future i18n.
