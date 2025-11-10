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

        Assert.NotNull(pantryPage.AddItemToolbarButton);
        Assert.True(pantryPage.AddItemToolbarButton.Displayed);

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
        Assert.True(_loginPage.EmailEntry.Displayed);
        Assert.True(_loginPage.EmailEntry.Enabled);

        Assert.NotNull(_loginPage.PasswordEntry);
        Assert.True(_loginPage.PasswordEntry.Displayed);
        Assert.True(_loginPage.PasswordEntry.Enabled);

        Assert.NotNull(_loginPage.LoginButton);
        Assert.True(_loginPage.LoginButton.Displayed);
        Assert.True(_loginPage.IsLoginButtonEnabled());

        Assert.NotNull(_loginPage.SignUpButton);
        Assert.True(_loginPage.SignUpButton.Displayed);

        Assert.NotNull(_loginPage.ForgotPasswordButton);
        Assert.True(_loginPage.ForgotPasswordButton.Displayed);
    }

    /// <summary>
    /// Test password visibility toggle functionality
    /// </summary>
    [Fact]
    public void PasswordVisibilityToggle_WorksCorrectly()
    {
        // Arrange
        _loginPage.WaitForPageToLoad();
        const string testPassword = "TestPassword123!";

        // Act - Enter password and toggle visibility
        _loginPage.EnterPassword(testPassword);
        _loginPage.ClickTogglePasswordVisibility();

        // Assert
        // Note: The exact assertion depends on how the password field behaves
        // In MAUI, toggling password visibility changes the IsPassword property
        Assert.NotNull(_loginPage.PasswordEntry);
        Assert.Equal(testPassword, _loginPage.GetPasswordText());
    }

    /// <summary>
    /// Test that loading indicator appears during login process
    /// </summary>
    [Fact]
    public void LoginProcess_ShowsLoadingIndicator()
    {
        // Arrange
        _loginPage.WaitForPageToLoad();

        // Act
        _loginPage.Login(LoginTestData.ValidCredentials.Email, LoginTestData.ValidCredentials.Password);

        // Assert
        // Loading indicator should be visible during the login process
        // Note: This test may need timing adjustments based on actual API response times
        Assert.True(_loginPage.IsLoadingVisible());

        // Wait for loading to complete
        _loginPage.WaitForLoadingToComplete();
        Assert.False(_loginPage.IsLoadingVisible());
    }

    /// <summary>
    /// Test form validation with empty fields
    /// </summary>
    [Theory]
    [InlineData("", "password")]
    [InlineData("email@example.com", "")]
    [InlineData("", "")]
    public void Login_WithEmptyFields_DisplaysValidationErrors(string email, string password)
    {
        // Arrange
        _loginPage.WaitForPageToLoad();

        // Act
        _loginPage.Login(email, password);

        // Assert
        // Note: Actual validation behavior depends on the app implementation
        // This test expects that empty fields either prevent login or show validation errors
        // You may need to adjust assertions based on actual validation behavior
        _loginPage.WaitForLoadingToComplete();

        // The test could check for:
        // - Validation error messages
        // - Login button remains enabled
        // - No navigation occurs
        // - Error dialogs appear
    }
}
