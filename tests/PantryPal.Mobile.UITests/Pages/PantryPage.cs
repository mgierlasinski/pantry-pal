using OpenQA.Selenium.Appium;
using PantryPal.Mobile.UITests.Extensions;

namespace PantryPal.Mobile.UITests.Pages;

/// <summary>
/// Page Object class for PantryPage, implementing the Page Object pattern.
/// Provides methods and properties for interacting with pantry page elements.
/// </summary>
public class PantryPage
{
    private readonly AppiumDriver _driver = AppiumSetup.App;

    // Element identifiers (AutomationId values)
    private const string AddItemToolbarButtonId = "AddItemToolbarButton";
    private const string PantryItemsListId = "PantryItemsList";
    private const string EmptyStateAddButtonId = "EmptyStateAddButton";
    private const string GenerateRecipeButtonId = "GenerateRecipeButton";
    private const string PantryLoadingIndicatorId = "PantryLoadingIndicator";

    // UI Element Properties
    public AppiumElement AddItemToolbarButton => _driver.FindElementById(AddItemToolbarButtonId);
    public AppiumElement PantryItemsList => _driver.FindElementById(PantryItemsListId);
    public AppiumElement GenerateRecipeButton => _driver.FindElementById(GenerateRecipeButtonId);

    // Properties that may not always exist
    public AppiumElement? EmptyStateAddButton => TryFindElement(EmptyStateAddButtonId);
    public AppiumElement? PantryLoadingIndicator => TryFindElement(PantryLoadingIndicatorId);

    /// <summary>
    /// Waits for the pantry page to be fully loaded
    /// </summary>
    public void WaitForPageToLoad()
    {
        _driver.WaitUntilVisible(PantryItemsListId);
        _driver.WaitUntilVisible(GenerateRecipeButtonId);
    }

    /// <summary>
    /// Clicks the add item toolbar button
    /// </summary>
    public void ClickAddItemToolbarButton()
    {
        AddItemToolbarButton.Click();
    }

    /// <summary>
    /// Clicks the generate recipe button
    /// </summary>
    public void ClickGenerateRecipeButton()
    {
        GenerateRecipeButton.Click();
    }

    /// <summary>
    /// Clicks the empty state add button (only visible when pantry is empty)
    /// </summary>
    public void ClickEmptyStateAddButton()
    {
        EmptyStateAddButton?.Click();
    }

    /// <summary>
    /// Finds a pantry item by its ID
    /// </summary>
    public AppiumElement? FindPantryItemById(string itemId)
    {
        return TryFindElement($"PantryItemName_{itemId}");
    }

    /// <summary>
    /// Finds the delete button for a specific pantry item
    /// </summary>
    public AppiumElement? FindDeleteButtonForItem(string itemId)
    {
        return TryFindElement($"DeleteItem_{itemId}");
    }

    /// <summary>
    /// Finds the favorite toggle button for a specific pantry item
    /// </summary>
    public AppiumElement? FindFavoriteToggleForItem(string itemId)
    {
        return TryFindElement($"FavoriteToggle_{itemId}");
    }

    /// <summary>
    /// Deletes a pantry item by its ID
    /// </summary>
    public void DeletePantryItem(string itemId)
    {
        var deleteButton = FindDeleteButtonForItem(itemId);
        deleteButton?.Click();
    }

    /// <summary>
    /// Toggles favorite status for a pantry item
    /// </summary>
    public void ToggleFavoriteForItem(string itemId)
    {
        var favoriteToggle = FindFavoriteToggleForItem(itemId);
        favoriteToggle?.Click();
    }

    /// <summary>
    /// Gets the text of a pantry item by its ID
    /// </summary>
    public string? GetPantryItemText(string itemId)
    {
        var itemElement = FindPantryItemById(itemId);
        return itemElement?.Text;
    }

    /// <summary>
    /// Checks if the pantry is empty (empty state is visible)
    /// </summary>
    public bool IsPantryEmpty()
    {
        return EmptyStateAddButton != null && EmptyStateAddButton.Displayed;
    }

    /// <summary>
    /// Checks if the loading indicator is visible
    /// </summary>
    public bool IsLoadingVisible()
    {
        return PantryLoadingIndicator?.Displayed ?? false;
    }

    /// <summary>
    /// Waits for loading to complete
    /// </summary>
    public void WaitForLoadingToComplete()
    {
        if (PantryLoadingIndicator != null)
        {
            _driver.WaitUntilInvisible(PantryLoadingIndicatorId);
        }
    }

    /// <summary>
    /// Checks if the generate recipe button is enabled
    /// </summary>
    public bool IsGenerateRecipeButtonEnabled()
    {
        return GenerateRecipeButton.Enabled;
    }

    /// <summary>
    /// Gets the count of visible pantry items (approximate)
    /// Note: This is a simplified approach - actual implementation may need to be more sophisticated
    /// </summary>
    public int GetVisibleItemsCount()
    {
        try
        {
            // This is a placeholder - actual implementation would need to query the collection view
            // For now, we'll use a simple check if any items are visible
            return IsPantryEmpty() ? 0 : 1; // Simplified logic
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Safely tries to find an element, returns null if not found
    /// </summary>
    private AppiumElement? TryFindElement(string automationId)
    {
        try
        {
            return _driver.FindElementById(automationId);
        }
        catch
        {
            return null;
        }
    }
}
