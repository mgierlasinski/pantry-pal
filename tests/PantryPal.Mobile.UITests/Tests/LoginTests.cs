using PantryPal.Mobile.UITests.Pages;
using PantryPal.Mobile.UITests.TestData;
using Xunit;

namespace PantryPal.Mobile.UITests.Tests;

[Collection("UITests")]
public class LoginTests
{
    private readonly LoginPage _loginPage = new();

    /// <summary>
    /// Test successful login with valid credentials
    /// </summary>
    [Fact]
    public void SuccessfulLogin_WithValidCredentials_ShouldNavigateToPantryPage()
    {
        // Arrange
        _loginPage.WaitForPageToLoad();

        // Act
        _loginPage.Login(LoginTestData.ValidCredentials.Email, LoginTestData.ValidCredentials.Password);

        // Assert
        // Wait for loading to complete on login page
        _loginPage.WaitForLoadingToComplete();

        // Verify navigation to PantryPage by checking for PantryPage elements
        var pantryPage = new PantryPage();
        pantryPage.WaitForPageToLoad();

        // Verify PantryPage elements are present and visible
        Assert.NotNull(pantryPage.PantryItemsList);
        Assert.True(pantryPage.PantryItemsList.Displayed);

        Assert.NotNull(pantryPage.GenerateRecipeButton);
        Assert.True(pantryPage.GenerateRecipeButton.Displayed);

        // Additional verification - pantry should load without loading indicator
        pantryPage.WaitForLoadingToComplete();
        Assert.False(pantryPage.IsLoadingVisible());
    }

    /// <summary>
    /// Test that login form elements are properly displayed and enabled
    /// </summary>
    [Fact]
    public void LoginPage_LoadsCorrectly_AllElementsVisibleAndEnabled()
    {
        // Arrange & Act
        _loginPage.WaitForPageToLoad();

        // Assert
        Assert.NotNull(_loginPage.EmailEntry);
        Assert.True(_loginPage.EmailEntry.EditText.Displayed);
        Assert.True(_loginPage.EmailEntry.EditText.Enabled);

        Assert.NotNull(_loginPage.PasswordEntry);
        Assert.True(_loginPage.PasswordEntry.EditText.Displayed);
        Assert.True(_loginPage.PasswordEntry.EditText.Enabled);

        Assert.NotNull(_loginPage.LoginButton);
        Assert.True(_loginPage.LoginButton.Displayed);
        Assert.True(_loginPage.IsLoginButtonEnabled());

        Assert.NotNull(_loginPage.SignUpButton);
        Assert.True(_loginPage.SignUpButton.Displayed);

        Assert.NotNull(_loginPage.ForgotPasswordButton);
        Assert.True(_loginPage.ForgotPasswordButton.Displayed);
    }

    /// <summary>
    /// Test form validation with empty fields
    /// </summary>
    [Fact]
    public void Login_WithEmptyFields_DisplaysValidationErrors()
    {
        // Arrange
        _loginPage.WaitForPageToLoad();

        // Ensure fields are empty (clear any existing data)
        _loginPage.EnterEmail("");
        _loginPage.EnterPassword("");

        // Act - Click login button with empty fields
        _loginPage.ClickLoginButton();

        // Assert
        _loginPage.WaitForLoadingToComplete();

        // Check that validation errors are displayed
        Assert.True(_loginPage.IsEmailValidationErrorVisible(), "Email validation error should be visible");
        Assert.True(_loginPage.IsPasswordValidationErrorVisible(), "Password validation error should be visible");

        // Check validation error messages
        var emailError = _loginPage.GetEmailValidationErrorText();
        var passwordError = _loginPage.GetPasswordValidationErrorText();

        // The error text might be "Email" as a header/title, or the full message
        // Check that some error text is present for both fields
        Assert.False(string.IsNullOrWhiteSpace(emailError), $"Email validation error should have some text. Actual: '{emailError}'");
        Assert.False(string.IsNullOrWhiteSpace(passwordError), $"Password validation error should have some text. Actual: '{passwordError}'");

        // At minimum, we expect the field name to be mentioned in the error
        Assert.True(emailError.Contains("Email") || emailError.ToLowerInvariant().Contains("required"),
                   $"Email error should mention the field name or required. Actual: '{emailError}'");
        Assert.True(passwordError.Contains("Password") || passwordError.ToLowerInvariant().Contains("required"),
                   $"Password error should mention the field name or required. Actual: '{passwordError}'");

        // Login button should remain enabled (validation doesn't disable it)
        Assert.True(_loginPage.IsLoginButtonEnabled(), "Login button should remain enabled after validation errors");
    }

    /// <summary>
    /// Test form validation with invalid data formats
    /// </summary>
    [Theory]
    [InlineData("invalid-email", "password123")] // Invalid email format
    [InlineData("test@", "password123")] // Incomplete email
    [InlineData("test@example.com", "")] // Valid email but empty password
    [InlineData("test@example.com", "123")] // Valid email but too short password
    public void Login_WithInvalidData_DisplaysValidationErrors(string email, string password)
    {
        // Arrange
        _loginPage.WaitForPageToLoad();

        // Act - Enter invalid data and submit
        _loginPage.Login(email, password);

        // Assert
        _loginPage.WaitForLoadingToComplete();

        // Check that validation errors are displayed
        var emailError = _loginPage.GetEmailValidationErrorText();
        var passwordError = _loginPage.GetPasswordValidationErrorText();

        // For invalid email formats, email validation should show error
        if (!IsValidEmailFormat(email))
        {
            Assert.True(_loginPage.IsEmailValidationErrorVisible(), $"Email validation error should be visible for invalid email: {email}");
            Assert.False(string.IsNullOrWhiteSpace(emailError), $"Email validation error should have text for invalid email: {email}. Actual: '{emailError}'");
        }

        // For empty or too short password, password validation should show error
        if (string.IsNullOrEmpty(password) || password.Length < 4)
        {
            Assert.True(_loginPage.IsPasswordValidationErrorVisible(), $"Password validation error should be visible for invalid password: {password}");
            Assert.False(string.IsNullOrWhiteSpace(passwordError), $"Password validation error should have text for invalid password: {password}. Actual: '{passwordError}'");
        }

        // Login button should remain enabled
        Assert.True(_loginPage.IsLoginButtonEnabled(), "Login button should remain enabled after validation errors");
    }

    /// <summary>
    /// Helper method to check if email has basic valid format
    /// </summary>
    private static bool IsValidEmailFormat(string email)
    {
        return !string.IsNullOrEmpty(email) &&
               email.Contains("@") &&
               email.Contains(".") &&
               email.IndexOf("@") < email.LastIndexOf(".");
    }
}
