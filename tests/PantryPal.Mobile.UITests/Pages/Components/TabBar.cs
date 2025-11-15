using OpenQA.Selenium.Appium;

namespace PantryPal.Mobile.UITests.Pages;

public class TabBar
{
    private readonly AppiumDriver _driver = AppiumSetup.App;

    public AppiumElement PantryTab => _driver.FindElement(MobileBy.AccessibilityId("Pantry"));
    public AppiumElement SavedRecipesTab => _driver.FindElement(MobileBy.AccessibilityId("Saved Recipes"));
    public AppiumElement ProfileTab => _driver.FindElement(MobileBy.AccessibilityId("Profile"));

    public void NavigateToPantry()
    {
        PantryTab.Click();
    }

    public void NavigateToSavedRecipes()
    {
        SavedRecipesTab.Click();
    }

    public void NavigateToProfile()
    {
        ProfileTab.Click();
    }
}
