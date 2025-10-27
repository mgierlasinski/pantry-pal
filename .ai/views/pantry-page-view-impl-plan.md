# View Implementation Plan PantryPage

## 1. Overview
PantryPage is a MAUI/XAML view that displays and manages a user’s pantry items. It allows users to add, edit, delete, and mark items as favorite, and trigger AI-powered recipe generation. It supports incremental loading, an empty state with a call-to-action, and proper accessibility attributes.

## 2. View Routing
Path: `src/PantryPal.Mobile/Views/PantryPage.xaml`

## 3. Component Structure
- PantryPage (ContentPage)
  - Toolbar (Add button)
  - CollectionView (Paginated list of PantryItemCell)
    - EmptyView (when no items)
  - Generate Recipe Button
  - Add/Edit Dialogs (Popup or Modal)
  - Confirm Delete Dialog

## 4. Component Details

### PantryPage (ContentPage)
- Description: Root page for pantry management.
- Main elements:
  - `CollectionView` ItemsSource bound to `Items` in ViewModel.
  - `ToolbarItem` for Add action.
  - `Button` at bottom for Generate Recipe.
  - `EmptyView` template with icon and CTA when `Items.Count == 0`.
- Handled interactions:
  - Appearing: triggers `LoadItemsCommand`.
  - Add toolbar tap: opens `ShowAddDialog`.
  - Generate button tap: invokes `GenerateRecipeCommand`.
- Validation: none directly; delegates to ViewModel and API.
- Types:
  - ViewModel: `PantryPageViewModel`.
  - DTO: `PantryItemDto`.
- Props: none.

### PantryItemCell (DataTemplate)
- Description: Displays single pantry item.
- Main elements:
  - `Label` for `Name` (AutomationId: `PantryItemName_{Id}`).
  - `ImageButton` for favorite toggle (AutomationId: `FavoriteToggle_{Id}`).
  - SwipeView with `SwipeItem` for Delete (AutomationId: `DeleteItem_{Id}`).
- Handled interactions:
  - Favorite tap: `ToggleFavoriteCommand` with `Id`.
  - Swipe delete: `DeleteItemCommand` with `Id`.
  - Item tap (optional): `EditItemCommand` opens edit dialog.
- Validation: none in template.
- Types:
  - ViewModel: uses item wrapper `PantryItemViewModel`.
- Props:
  - Bound `PantryItemViewModel`.

### Add/Edit Item Dialog
- Description: Popup/modal for entering or editing item name.
- Main elements:
  - `Entry` bound to `DialogItemName`.
  - `Button` Save (AutomationId: `SaveItemButton`).
  - `Button` Cancel.
- Handled interactions:
  - Save tap: validates name length (1–100) then calls `AddItemCommand` or `EditItemCommand`.
  - Cancel tap: closes dialog.
- Validation: local name length check.
- Types:
  - ViewModel fields: `DialogItemName`, `DialogIsEdit`, `SelectedItemId`.
- Props: none.

### Confirm Delete Dialog
- Description: Confirm deletion of item.
- Main elements:
  - `Label` confirmation text.
  - `Button` Confirm (AutomationId: `ConfirmDeleteButton`).
  - `Button` Cancel.
- Handled interactions:
  - Confirm tap: calls `ConfirmDeleteCommand`.
  - Cancel tap: closes dialog.
- Validation: none.
- Types: uses `SelectedItemId`.

## 5. Types

### PantryItemDto (existing)
- Fields: `string Id`, `string Name`, `bool IsFavorite`, `string CreatedAt`, `string UpdatedAt`

### PantryItemViewModel (new)
```csharp
public class PantryItemViewModel {
  public string Id { get; set; }
  public string Name { get; set; }
  public bool IsFavorite { get; set; }
  public ICommand ToggleFavoriteCommand { get; }
  public ICommand DeleteItemCommand { get; }
  public ICommand EditItemCommand { get; }
}
```

### PantryPageViewModel (new)
Fields and properties:
- `ObservableCollection<PantryItemViewModel> Items`
- `int Page`, `int PageSize`, `string SortField`
- `bool IsBusy`, `bool IsEmpty`, `string ErrorMessage`
- `string DialogItemName`, `bool DialogIsEdit`, `string SelectedItemId`
- Commands:
  - `LoadItemsCommand`, `AddItemCommand`, `EditItemCommand`, `ConfirmDeleteCommand`, `ToggleFavoriteCommand`, `GenerateRecipeCommand`
- Services:
  - `IPantryService` injected via DI

## 6. State Management
- `Items`: bound to CollectionView
- `IsBusy`: disables UI during API calls
- `IsEmpty`: toggles EmptyView
- `DialogItemName`, `DialogIsEdit`, `SelectedItemId`: drive add/edit dialogs
- `ErrorMessage`: displays toast or alert on error

## 7. API Integration

| Action           | HTTP Call                                    | Request Type                 | Response Type                  |
|------------------|----------------------------------------------|------------------------------|--------------------------------|
| Load items       | GET `/pantry-items?page=...&pageSize=...`    | N/A                          | `PantryItemsPaginatedResponseDto` |
| Add item         | POST `/pantry-items`                         | `PantryItemCreateDto`        | `PantryItemDto`                |
| Edit item        | PATCH `/pantry-items/{id}`                   | `PantryItemUpdateDto`        | `PantryItemDto`                |
| Delete item      | DELETE `/pantry-items/{id}`                  | N/A                          | NoContent                      |
| Toggle favorite  | PATCH `/pantry-items/{id}`                   | `{ is_favorite: bool }`       | `PantryItemDto`                |
| Generate recipe  | POST `/recipes/generate` (future)            | N/A                          | recipe markdown string (navigate) |

Use `PantryService` wrapper in Mobile.Services to call endpoints, update `Items` collection accordingly.

## 8. User Interactions
- Page load: load and display items
- Tap Add: show dialog, validate, add, reload or insert into `Items`
- Tap Edit on item: show dialog pre-filled, validate, patch, update item in `Items`
- Swipe delete: show confirmation, delete, remove from `Items`
- Tap favorite icon: toggle UI state, patch, update `Items`
- Tap Generate Recipe: navigate to `RecipePage` with selected pantry list or handle empty

## 9. Conditions and Validation
- Name length 1–100 on add/edit
- Only enable Generate when `Items.Count > 0`
- Prevent duplicate names by catching `409 Conflict` and showing alert
- Redirect to Login on `401 Unauthorized`

## 10. Error Handling
- Wrap API calls in try/catch; set `ErrorMessage` and show alert
- On network failure: show retry snackbar
- On validation errors: show inline message in dialog
- On unauthorized: Shell.GoToAsync("//LoginPage")

## 11. Implementation Steps
1. Create `PantryPage.xaml` with `CollectionView`, `EmptyView`, toolbar Add, Generate button.
2. Define `PantryItemCell` DataTemplate in XAML.
3. Implement `PantryItemViewModel` and `PantryPageViewModel` with properties, commands, state.
4. Register `PantryPageViewModel` in DI in `MauiProgram.cs`.
5. Bind ViewModel to `PantryPage` in code-behind.
6. Implement XAML dialogs for add/edit and delete confirmation.
7. Implement API calls in `PantryPageViewModel` using `IPantryService`.
8. Add validation logic in commands for name length.
9. Handle responses (insert, update, remove) to update `Items`.
10. Implement 401 redirect logic in catch block.
11. Style with UraniumUI Material controls and accessibility attributes.
12. Test each user story: add, edit, delete, favorite, empty state, generate button behavior.
