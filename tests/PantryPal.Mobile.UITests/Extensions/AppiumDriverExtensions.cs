using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace PantryPal.Mobile.UITests.Extensions;

/// <summary>
/// Extension methods for AppiumDriver to provide convenient waiting methods
/// </summary>
public static class AppiumDriverExtensions
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Waits until element with specified AutomationId is visible
    /// </summary>
    public static void WaitUntilVisible(this AppiumDriver driver, string automationId, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? DefaultTimeout);
        wait.Until(d => FindElementById(d as AppiumDriver, automationId).Displayed);
    }

    public static void WaitUntilVisible(this AppiumDriver driver, AppiumElement element, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? DefaultTimeout);
        wait.Until(d => element.Displayed);
    }

    /// <summary>
    /// Waits until element with specified AutomationId is clickable
    /// </summary>
    public static void WaitUntilClickable(this AppiumDriver driver, string automationId, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? DefaultTimeout);
        wait.Until(d =>
        {
            var element = FindElementById(d as AppiumDriver, automationId);
            return element.Enabled && element.Displayed;
        });
    }

    /// <summary>
    /// Waits until element with specified AutomationId is invisible
    /// </summary>
    public static void WaitUntilInvisible(this AppiumDriver driver, string automationId, TimeSpan? timeout = null)
    {
        var wait = new WebDriverWait(driver, timeout ?? DefaultTimeout);
        wait.Until(d =>
        {
            try
            {
                var element = FindElementById(d as AppiumDriver, automationId);
                return !element.Displayed;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        });
    }

    /// <summary>
    /// Finds UI element using AutomationId, compatible with Windows and Android drivers
    /// </summary>
    public static AppiumElement FindElementById(this AppiumDriver driver, string automationId)
    {
        if (driver is OpenQA.Selenium.Appium.Windows.WindowsDriver)
        {
            return driver.FindElement(MobileBy.AccessibilityId(automationId));
        }

        return driver.FindElement(MobileBy.Id(automationId));
    }

    public static AppiumElement FindElementByText(this AppiumDriver driver, string text)
    {
        return driver.FindElement(MobileBy.AndroidUIAutomator($"new UiSelector().text(\"{text}\")"));
    }

    /// <summary>
    /// Waits for element to be visible and returns it
    /// </summary>
    public static AppiumElement WaitAndFindElement(this AppiumDriver driver, string automationId, TimeSpan? timeout = null)
    {
        WaitUntilVisible(driver, automationId, timeout);
        return FindElementById(driver, automationId);
    }
}
