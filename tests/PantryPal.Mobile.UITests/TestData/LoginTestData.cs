namespace PantryPal.Mobile.UITests.TestData;

/// <summary>
/// Test data for login functionality tests
/// </summary>
public static class LoginTestData
{
    /// <summary>
    /// Valid login credentials for successful login scenario
    /// </summary>
    public static class ValidCredentials
    {
        public const string Email = "test@gmail.com";
        public const string Password = "Test1234";
    }

    /// <summary>
    /// Invalid login credentials for negative test scenarios
    /// </summary>
    public static class InvalidCredentials
    {
        public const string WrongEmail = "wrong@example.com";
        public const string WrongPassword = "WrongPassword123!";
        public const string EmptyEmail = "";
        public const string EmptyPassword = "";
        public const string InvalidEmailFormat = "invalid-email";
    }

    /// <summary>
    /// Test data for edge cases
    /// </summary>
    public static class EdgeCases
    {
        public const string VeryLongEmail = "verylongemailaddressfortestingpurposes@exampledomain.com";
        public const string VeryLongPassword = "ThisIsAVeryLongPasswordThatExceedsNormalLimitsAndShouldStillWork123!";
        public const string SpecialCharactersPassword = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        public const string UnicodePassword = "Пароль123!ñáéíóú";
    }
}
