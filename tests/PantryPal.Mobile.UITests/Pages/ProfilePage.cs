using OpenQA.Selenium.Appium;
using PantryPal.Mobile.UITests.Extensions;

namespace PantryPal.Mobile.UITests.Pages;

/// <summary>
/// Page Object class for ProfilePage, implementing the Page Object pattern.
/// Provides methods and properties for interacting with profile page elements.
/// </summary>
public class ProfilePage
{
    private readonly AppiumDriver _driver = AppiumSetup.App;
    private readonly TabBar _tabBar = new();

    // Element identifiers (AutomationId values)
    private const string ProfileLoadingIndicatorId = "ProfileLoadingIndicator";
    private const string DietTypePickerId = "DietTypePicker";
    private const string PreferredCuisinePickerId = "PreferredCuisinePicker";
    private const string DislikedIngredientsEditorId = "DislikedIngredientsEditor";
    private const string SavePreferencesButtonId = "SavePreferencesButton";
    private const string LogoutButtonText = "Logout";

    // UI Element Properties
    public AppiumElement DietTypePicker => _driver.FindElementById(DietTypePickerId);
    public AppiumElement PreferredCuisinePicker => _driver.FindElementById(PreferredCuisinePickerId);
    public AppiumElement DislikedIngredientsEditor => _driver.FindElementById(DislikedIngredientsEditorId);
    public AppiumElement SavePreferencesButton => _driver.FindElementById(SavePreferencesButtonId);
    public AppiumElement LogoutButton => _driver.FindElementByText(LogoutButtonText);

    // Properties that may not always exist
    public AppiumElement? ProfileLoadingIndicator => TryFindElement(ProfileLoadingIndicatorId);

    /// <summary>
    /// Waits for the profile page to be fully loaded
    /// </summary>
    public void WaitForPageToLoad()
    {
        _driver.WaitUntilVisible(LogoutButton);
    }

    /// <summary>
    /// Selects a diet type from the picker
    /// </summary>
    public void SelectDietType(string dietType)
    {
        DietTypePicker.Click();
        // Note: Actual implementation would depend on how the picker works
        // This is a simplified version - you may need to select from dropdown options
        var option = _driver.FindElement(OpenQA.Selenium.By.Name(dietType));
        option.Click();
    }

    /// <summary>
    /// Selects a preferred cuisine from the picker
    /// </summary>
    public void SelectPreferredCuisine(string cuisine)
    {
        PreferredCuisinePicker.Click();
        // Note: Actual implementation would depend on how the picker works
        // This is a simplified version - you may need to select from dropdown options
        var option = _driver.FindElement(OpenQA.Selenium.By.Name(cuisine));
        option.Click();
    }

    /// <summary>
    /// Enters disliked ingredients in the editor
    /// </summary>
    public void EnterDislikedIngredients(string ingredients)
    {
        DislikedIngredientsEditor.Clear();
        DislikedIngredientsEditor.SendKeys(ingredients);
    }

    /// <summary>
    /// Clicks the save preferences button
    /// </summary>
    public void ClickSavePreferencesButton()
    {
        SavePreferencesButton.Click();
    }

    /// <summary>
    /// Clicks the logout button
    /// </summary>
    public void ClickLogoutButton()
    {
        LogoutButton.Click();
    }

    /// <summary>
    /// Checks if the loading indicator is visible
    /// </summary>
    public bool IsLoadingVisible()
    {
        return ProfileLoadingIndicator?.Displayed ?? false;
    }

    /// <summary>
    /// Waits for loading to complete
    /// </summary>
    public void WaitForLoadingToComplete()
    {
        if (ProfileLoadingIndicator != null)
        {
            _driver.WaitUntilInvisible(ProfileLoadingIndicatorId);
        }
    }

    /// <summary>
    /// Checks if the save preferences button is enabled
    /// </summary>
    public bool IsSavePreferencesButtonEnabled()
    {
        return SavePreferencesButton.Enabled;
    }

    /// <summary>
    /// Checks if the logout button is enabled
    /// </summary>
    public bool IsLogoutButtonEnabled()
    {
        return LogoutButton.Enabled;
    }

    /// <summary>
    /// Gets the current text in disliked ingredients editor
    /// </summary>
    public string GetDislikedIngredientsText()
    {
        return DislikedIngredientsEditor.Text;
    }

    /// <summary>
    /// Gets the currently selected diet type
    /// </summary>
    public string GetSelectedDietType()
    {
        // Note: This might need adjustment based on how the picker displays selected value
        return DietTypePicker.Text;
    }

    /// <summary>
    /// Gets the currently selected preferred cuisine
    /// </summary>
    public string GetSelectedPreferredCuisine()
    {
        // Note: This might need adjustment based on how the picker displays selected value
        return PreferredCuisinePicker.Text;
    }

    /// <summary>
    /// Navigates to the Profile page by clicking the Profile tab
    /// </summary>
    public void NavigateTo()
    {
        _tabBar.NavigateToProfile();

        // Wait for navigation to complete
        WaitForPageToLoad();
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
