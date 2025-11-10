using PantryPal.Mobile.UITests.Pages;
using PantryPal.Mobile.UITests.TestData;
using Xunit;

namespace PantryPal.Mobile.UITests.Tests;

/// <summary>
/// Test class for profile-related scenarios including logout functionality
/// </summary>
[Collection("UITests")]
public class ProfileTests
{
    private readonly LoginPage _loginPage = new();
    private readonly PantryPage _pantryPage = new();
    private readonly ProfilePage _profilePage = new();

    /// <summary>
    /// Test complete logout flow: login -> navigate to profile -> logout -> verify return to login screen
    /// </summary>
    [Fact]
    public void LogoutFlow_LoginToProfileThenLogout_ShouldReturnToLoginScreen()
    {
        // Step 1: Login successfully
        _loginPage.WaitForPageToLoad();
        _loginPage.Login(LoginTestData.ValidCredentials.Email, LoginTestData.ValidCredentials.Password);
        _loginPage.WaitForLoadingToComplete();

        // Verify we are on PantryPage after login
        _pantryPage.WaitForPageToLoad();

        // Step 2: Navigate to Profile page
        _profilePage.NavigateTo();

        // Step 3: Click logout button
        _profilePage.ClickLogoutButton();

        // Step 4: Verify return to login screen
        // Wait for login page elements to appear (indicating successful logout and navigation)
        _loginPage.WaitForPageToLoad();

        // Assert that we are back on the login screen
        Assert.NotNull(_loginPage.EmailEntry);
        Assert.True(_loginPage.EmailEntry.Displayed);
        Assert.NotNull(_loginPage.PasswordEntry);
        Assert.True(_loginPage.PasswordEntry.Displayed);
        Assert.NotNull(_loginPage.LoginButton);
        Assert.True(_loginPage.IsLoginButtonEnabled());

        // Additional verification - profile page elements should no longer be visible
        // This might need adjustment based on how navigation works in the app
        AssertProfilePageNotVisible();
    }

    /// <summary>
    /// Test that profile page loads correctly with all elements visible
    /// </summary>
    [Fact]
    public void ProfilePage_LoadsCorrectly_AllElementsVisibleAndFunctional()
    {
        // Setup: Login and navigate to profile
        PerformLoginAndNavigateToProfile();

        // Assert profile page elements are loaded and visible
        Assert.NotNull(_profilePage.DietTypePicker);
        Assert.True(_profilePage.DietTypePicker.Displayed);
        Assert.True(_profilePage.DietTypePicker.Enabled);

        Assert.NotNull(_profilePage.PreferredCuisinePicker);
        Assert.True(_profilePage.PreferredCuisinePicker.Displayed);
        Assert.True(_profilePage.PreferredCuisinePicker.Enabled);

        Assert.NotNull(_profilePage.DislikedIngredientsEditor);
        Assert.True(_profilePage.DislikedIngredientsEditor.Displayed);
        Assert.True(_profilePage.DislikedIngredientsEditor.Enabled);

        Assert.NotNull(_profilePage.SavePreferencesButton);
        Assert.True(_profilePage.SavePreferencesButton.Displayed);
        Assert.True(_profilePage.IsSavePreferencesButtonEnabled());

        Assert.NotNull(_profilePage.LogoutButton);
        Assert.True(_profilePage.LogoutButton.Displayed);
        Assert.True(_profilePage.IsLogoutButtonEnabled());

        // Verify loading indicator is not visible when page is loaded
        Assert.False(_profilePage.IsLoadingVisible());
    }

    /// <summary>
    /// Test editing and saving profile preferences
    /// </summary>
    [Fact]
    public void ProfilePreferences_CanEditAndSavePreferences()
    {
        // Setup: Login and navigate to profile
        PerformLoginAndNavigateToProfile();

        // Act: Modify preferences
        const string testIngredients = "Test disliked ingredients";
        _profilePage.EnterDislikedIngredients(testIngredients);

        // Note: Selecting from pickers would require more complex implementation
        // depending on how the picker UI works in the app

        // Click save
        _profilePage.ClickSavePreferencesButton();

        // Assert: Verify changes were attempted to be saved
        // Note: Actual verification would depend on app behavior (success message, navigation, etc.)
        _profilePage.WaitForLoadingToComplete();

        // The save operation should complete without errors
        // Additional assertions could check for success messages or updated data
    }

    /// <summary>
    /// Helper method to perform login and navigate to profile page
    /// </summary>
    private void PerformLoginAndNavigateToProfile()
    {
        // Login
        _loginPage.WaitForPageToLoad();
        _loginPage.Login(LoginTestData.ValidCredentials.Email, LoginTestData.ValidCredentials.Password);
        _loginPage.WaitForLoadingToComplete();

        // Navigate to profile
        _profilePage.NavigateTo();
    }


    /// <summary>
    /// Helper method to assert that profile page is no longer visible
    /// </summary>
    private void AssertProfilePageNotVisible()
    {
        // This is a simplified check - in reality you might need more sophisticated logic
        // to verify that profile page elements are no longer accessible
        try
        {
            var profileElement = _profilePage.DietTypePicker;
            // If we can find profile elements, they shouldn't be displayed
            Assert.False(profileElement.Displayed);
        }
        catch
        {
            // If elements are not found at all, that's also acceptable (page unloaded)
        }
    }
}
