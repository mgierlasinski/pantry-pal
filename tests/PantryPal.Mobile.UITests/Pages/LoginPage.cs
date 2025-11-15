using OpenQA.Selenium.Appium;
using PantryPal.Mobile.UITests.Extensions;
using PantryPal.Mobile.UITests.Pages.Components;

namespace PantryPal.Mobile.UITests.Pages;

/// <summary>
/// Page Object class for LoginPage, implementing the Page Object pattern.
/// Provides methods and properties for interacting with login page elements.
/// </summary>
public class LoginPage
{
    private readonly AppiumDriver _driver = AppiumSetup.App;

    // Element identifiers (AutomationId values)
    private const string EmailEntryId = "EmailEntry";
    private const string PasswordEntryId = "PasswordEntry";
    private const string LoginButtonId = "LoginButton";
    private const string SignUpButtonId = "SignUpButton";
    private const string ForgotPasswordButtonId = "ForgotPasswordButton";
    private const string LoadingIndicatorId = "LoadingIndicator";

    // UI Element Properties
    public MaterialTextField EmailEntry => new(_driver.FindElementById(EmailEntryId));
    public MaterialTextField PasswordEntry => new(_driver.FindElementById(PasswordEntryId));

    public AppiumElement LoginButton => _driver.FindElementById(LoginButtonId);
    public AppiumElement SignUpButton => _driver.FindElementById(SignUpButtonId);
    public AppiumElement ForgotPasswordButton => _driver.FindElementById(ForgotPasswordButtonId);
    public AppiumElement LoadingIndicator => _driver.FindElementById(LoadingIndicatorId);

    /// <summary>
    /// Waits for the login page to be fully loaded
    /// </summary>
    public void WaitForPageToLoad()
    {
        _driver.WaitUntilVisible(EmailEntryId);
        _driver.WaitUntilVisible(PasswordEntryId);
        _driver.WaitUntilVisible(LoginButtonId);
    }

    /// <summary>
    /// Enters email address into the email field
    /// </summary>
    public void EnterEmail(string email)
    {
        // Click the actual EditText element within the TextField
        EmailEntry.EditText.Click();

        // Small delay to ensure focus
        System.Threading.Thread.Sleep(200);

        // Clear and enter text
        EmailEntry.EditText.Clear();
        if (!string.IsNullOrEmpty(email))
        {
            EmailEntry.EditText.SendKeys(email);
        }
    }

    /// <summary>
    /// Enters password into the password field
    /// </summary>
    public void EnterPassword(string password)
    {
        // Click the actual EditText element within the TextField
        PasswordEntry.EditText.Click();

        // Small delay to ensure focus
        Thread.Sleep(200);

        // Clear and enter text
        PasswordEntry.EditText.Clear();
        if (!string.IsNullOrEmpty(password))
        {
            PasswordEntry.EditText.SendKeys(password);
        }
    }

    /// <summary>
    /// Clicks the login button
    /// </summary>
    public void ClickLoginButton()
    {
        LoginButton.Click();
    }


    /// <summary>
    /// Clicks the sign up button to navigate to registration
    /// </summary>
    public void ClickSignUpButton()
    {
        SignUpButton.Click();
    }

    /// <summary>
    /// Clicks the forgot password button
    /// </summary>
    public void ClickForgotPasswordButton()
    {
        ForgotPasswordButton.Click();
    }

    /// <summary>
    /// Performs complete login flow with email and password
    /// </summary>
    public void Login(string email, string password)
    {
        WaitForPageToLoad();
        EnterEmail(email);
        EnterPassword(password);
        ClickLoginButton();
    }

    /// <summary>
    /// Checks if the loading indicator is visible
    /// </summary>
    public bool IsLoadingVisible()
    {
        try
        {
            return LoadingIndicator.Displayed;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for loading to complete
    /// </summary>
    public void WaitForLoadingToComplete()
    {
        _driver.WaitUntilInvisible(LoadingIndicatorId);
    }

    /// <summary>
    /// Checks if login button is enabled
    /// </summary>
    public bool IsLoginButtonEnabled()
    {
        return LoginButton.Enabled;
    }

    /// <summary>
    /// Gets the current text in email field
    /// </summary>
    public string GetEmailText()
    {
        return EmailEntry.EditText.Text;
    }

    /// <summary>
    /// Gets the current text in password field
    /// </summary>
    public string GetPasswordText()
    {
        return PasswordEntry.EditText.Text;
    }

    /// <summary>
    /// Checks if email validation error is visible
    /// </summary>
    public bool IsEmailValidationErrorVisible()
    {
        try
        {
            return EmailEntry.ValidationError.Displayed;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if password validation error is visible
    /// </summary>
    public bool IsPasswordValidationErrorVisible()
    {
        try
        {
            return PasswordEntry.ValidationError.Displayed;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the email validation error text
    /// </summary>
    public string GetEmailValidationErrorText()
    {
        try
        {
            return EmailEntry.ValidationError.Text;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the password validation error text
    /// </summary>
    public string GetPasswordValidationErrorText()
    {
        try
        {
            return PasswordEntry.ValidationError.Text;
        }
        catch
        {
            return string.Empty;
        }
    }
}
