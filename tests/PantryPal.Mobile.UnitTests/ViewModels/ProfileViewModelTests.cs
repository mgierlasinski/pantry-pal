using Moq;
using PantryPal.Data;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.ViewModels;
using System.Net;

namespace PantryPal.Mobile.UnitTests.ViewModels;

public class ProfileViewModelTests
{
    private readonly Mock<IUserPreferencesService> _mockUserPreferencesService;
    private readonly Mock<IDietTypesService> _mockDietTypesService;
    private readonly Mock<IPreferredCuisinesService> _mockPreferredCuisinesService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IDisplayService> _mockDisplayService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly ProfileViewModel _viewModel;

    public ProfileViewModelTests()
    {
        _mockUserPreferencesService = new Mock<IUserPreferencesService>();
        _mockDietTypesService = new Mock<IDietTypesService>();
        _mockPreferredCuisinesService = new Mock<IPreferredCuisinesService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockDisplayService = new Mock<IDisplayService>();
        _mockNavigationService = new Mock<INavigationService>();

        _viewModel = new ProfileViewModel(
            _mockUserPreferencesService.Object,
            _mockDietTypesService.Object,
            _mockPreferredCuisinesService.Object,
            _mockAuthService.Object,
            _mockDisplayService.Object,
            _mockNavigationService.Object);
    }

    // Test data
    private static readonly DietTypeDto[] SampleDietTypes =
    [
        new(1, "Vegetarian"),
        new(2, "Vegan"),
        new(3, "Keto")
    ];

    private static readonly PreferredCuisineDto[] SampleCuisines =
    [
        new(1, "Italian"),
        new(2, "Mexican"),
        new(3, "Asian")
    ];

    private static readonly UserPreferencesDto SamplePreferences = new(
        UserId: "user1",
        DietTypeId: 1,
        DietTypeName: "Vegetarian",
        PreferredCuisineId: 1,
        PreferredCuisineName: "Italian",
        DislikedIngredients: "Onions, Garlic",
        CreatedAt: "2024-01-01T00:00:00Z",
        UpdatedAt: "2024-01-01T00:00:00Z"
    );


    [Fact]
    public async Task LoadPreferencesAsync_ExistingUser_Success_PopulatesCollectionsAndPreferences()
    {
        // Arrange
        var dietTypesResponse = new DietTypesResponseDto(SampleDietTypes);
        var cuisinesResponse = new PreferredCuisinesResponseDto(SampleCuisines);

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ReturnsAsync(dietTypesResponse);
        _mockPreferredCuisinesService.Setup(s => s.GetPreferredCuisinesAsync())
            .ReturnsAsync(cuisinesResponse);
        _mockUserPreferencesService.Setup(s => s.GetUserPreferencesAsync())
            .ReturnsAsync(SamplePreferences);

        // Act
        await _viewModel.LoadPreferencesAsync();

        // Assert
        Assert.Equal(3, _viewModel.DietTypes.Count);
        Assert.Equal(3, _viewModel.PreferredCuisines.Count);
        Assert.Equal(SampleDietTypes[0], _viewModel.SelectedDietType);
        Assert.Equal(SampleCuisines[0], _viewModel.SelectedPreferredCuisine);
        Assert.Equal("Onions, Garlic", _viewModel.DislikedIngredients);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadPreferencesAsync_NewUser_Success_PopulatesCollectionsAndClearsSelections()
    {
        // Arrange
        var dietTypesResponse = new DietTypesResponseDto(SampleDietTypes);
        var cuisinesResponse = new PreferredCuisinesResponseDto(SampleCuisines);

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ReturnsAsync(dietTypesResponse);
        _mockPreferredCuisinesService.Setup(s => s.GetPreferredCuisinesAsync())
            .ReturnsAsync(cuisinesResponse);
        _mockUserPreferencesService.Setup(s => s.GetUserPreferencesAsync())
            .ReturnsAsync((UserPreferencesDto?)null);

        // Act
        await _viewModel.LoadPreferencesAsync();

        // Assert
        Assert.Equal(3, _viewModel.DietTypes.Count);
        Assert.Equal(3, _viewModel.PreferredCuisines.Count);
        Assert.Null(_viewModel.SelectedDietType);
        Assert.Null(_viewModel.SelectedPreferredCuisine);
        Assert.Equal(string.Empty, _viewModel.DislikedIngredients);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadPreferencesAsync_IsLoadingTrue_PreventsMultipleCalls()
    {
        // Arrange
        _viewModel.IsLoading = true;

        // Act
        await _viewModel.LoadPreferencesAsync();

        // Assert
        _mockDietTypesService.Verify(s => s.GetDietTypesAsync(), Times.Never);
        _mockPreferredCuisinesService.Verify(s => s.GetPreferredCuisinesAsync(), Times.Never);
        _mockUserPreferencesService.Verify(s => s.GetUserPreferencesAsync(), Times.Never);
    }

    [Fact]
    public async Task LoadPreferencesAsync_UnauthorizedError_NavigatesToLogin()
    {
        // Arrange
        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ThrowsAsync(exception);
        _mockPreferredCuisinesService.Setup(s => s.GetPreferredCuisinesAsync())
            .ReturnsAsync(new PreferredCuisinesResponseDto(SampleCuisines));
        _mockUserPreferencesService.Setup(s => s.GetUserPreferencesAsync())
            .ReturnsAsync(SamplePreferences);

        // Act
        await _viewModel.LoadPreferencesAsync();

        // Assert
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.LoginRoute, It.IsAny<bool>()), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadPreferencesAsync_NetworkError_ShowsToast()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LoadPreferencesAsync();

        // Assert
        _mockDisplayService.Verify(s => s.ShowToast("Network error: Network error"), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoadPreferencesAsync_UnexpectedError_ShowsToast()
    {
        // Arrange
        var exception = new InvalidOperationException("Unexpected error");

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LoadPreferencesAsync();

        // Assert
        _mockDisplayService.Verify(s => s.ShowToast("Failed to load preferences: Unexpected error"), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }


    [Theory]
    [InlineData(null, null, "")]
    [InlineData(1, null, "")]
    public async Task SavePreferencesAsync_MissingRequiredFields_PreventsSave(
        int? dietTypeId, int? cuisineId, string dislikedIngredients)
    {
        // Arrange
        var dietTypesResponse = new DietTypesResponseDto(SampleDietTypes);
        var cuisinesResponse = new PreferredCuisinesResponseDto(SampleCuisines);

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ReturnsAsync(dietTypesResponse);
        _mockPreferredCuisinesService.Setup(s => s.GetPreferredCuisinesAsync())
            .ReturnsAsync(cuisinesResponse);

        await _viewModel.LoadPreferencesAsync();

        // Set selections based on test parameters
        _viewModel.SelectedDietType = dietTypeId.HasValue
            ? SampleDietTypes.First(dt => dt.Id == (short)dietTypeId.Value)
            : null;
        _viewModel.SelectedPreferredCuisine = cuisineId.HasValue
            ? SampleCuisines.First(pc => pc.Id == (short)cuisineId.Value)
            : null;
        _viewModel.DislikedIngredients = dislikedIngredients;

        // Act
        await _viewModel.SavePreferencesAsync();

        // Assert
        _mockUserPreferencesService.Verify(s => s.UpsertUserPreferencesAsync(It.IsAny<UserPreferencesCreateDto>()), Times.Never);
        Assert.True(_viewModel.HasErrors);
    }

    [Fact]
    public async Task SavePreferencesAsync_DislikedIngredientsTooLong_PreventsSave()
    {
        // Arrange
        var dietTypesResponse = new DietTypesResponseDto(SampleDietTypes);
        var cuisinesResponse = new PreferredCuisinesResponseDto(SampleCuisines);

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ReturnsAsync(dietTypesResponse);
        _mockPreferredCuisinesService.Setup(s => s.GetPreferredCuisinesAsync())
            .ReturnsAsync(cuisinesResponse);

        await _viewModel.LoadPreferencesAsync();

        _viewModel.SelectedDietType = SampleDietTypes[0];
        _viewModel.SelectedPreferredCuisine = SampleCuisines[0];
        _viewModel.DislikedIngredients = new string('a', 1001); // Exceeds MaxLength(1000)

        // Act
        await _viewModel.SavePreferencesAsync();

        // Assert
        _mockUserPreferencesService.Verify(s => s.UpsertUserPreferencesAsync(It.IsAny<UserPreferencesCreateDto>()), Times.Never);
        Assert.True(_viewModel.HasErrors);
    }

    [Theory]
    [InlineData("  Onions, Garlic  ", "Onions, Garlic")] // Trims whitespace
    [InlineData("", null)] // Empty string becomes null
    [InlineData("   ", null)] // Whitespace only becomes null
    public async Task SavePreferencesAsync_ValidData_Success_SendsCorrectDto(string input, string? expected)
    {
        // Arrange
        var dietTypesResponse = new DietTypesResponseDto(SampleDietTypes);
        var cuisinesResponse = new PreferredCuisinesResponseDto(SampleCuisines);
        var expectedDto = new UserPreferencesCreateDto(
            DietTypeId: 1,
            PreferredCuisineId: 2,
            DislikedIngredients: expected
        );

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ReturnsAsync(dietTypesResponse);
        _mockPreferredCuisinesService.Setup(s => s.GetPreferredCuisinesAsync())
            .ReturnsAsync(cuisinesResponse);
        _mockUserPreferencesService.Setup(s => s.UpsertUserPreferencesAsync(expectedDto))
            .ReturnsAsync(SamplePreferences);

        await _viewModel.LoadPreferencesAsync();

        _viewModel.SelectedDietType = SampleDietTypes[0]; // Id = 1
        _viewModel.SelectedPreferredCuisine = SampleCuisines[1]; // Id = 2
        _viewModel.DislikedIngredients = input;

        // Act
        await _viewModel.SavePreferencesAsync();

        // Assert
        _mockUserPreferencesService.Verify(s => s.UpsertUserPreferencesAsync(
            It.Is<UserPreferencesCreateDto>(dto =>
                dto.DietTypeId == expectedDto.DietTypeId &&
                dto.PreferredCuisineId == expectedDto.PreferredCuisineId &&
                dto.DislikedIngredients == expectedDto.DislikedIngredients)), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("Preferences saved successfully!"), Times.Once);
    }

    [Fact]
    public async Task SavePreferencesAsync_UnauthorizedError_NavigatesToLogin()
    {
        // Arrange
        var dietTypesResponse = new DietTypesResponseDto(SampleDietTypes);
        var cuisinesResponse = new PreferredCuisinesResponseDto(SampleCuisines);
        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ReturnsAsync(dietTypesResponse);
        _mockPreferredCuisinesService.Setup(s => s.GetPreferredCuisinesAsync())
            .ReturnsAsync(cuisinesResponse);
        _mockUserPreferencesService.Setup(s => s.UpsertUserPreferencesAsync(It.IsAny<UserPreferencesCreateDto>()))
            .ThrowsAsync(exception);

        await _viewModel.LoadPreferencesAsync();

        _viewModel.SelectedDietType = SampleDietTypes[0];
        _viewModel.SelectedPreferredCuisine = SampleCuisines[0];

        // Act
        await _viewModel.SavePreferencesAsync();

        // Assert
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.LoginRoute, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task SavePreferencesAsync_NetworkError_ShowsToast()
    {
        // Arrange
        var dietTypesResponse = new DietTypesResponseDto(SampleDietTypes);
        var cuisinesResponse = new PreferredCuisinesResponseDto(SampleCuisines);
        var exception = new HttpRequestException("Network error");

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ReturnsAsync(dietTypesResponse);
        _mockPreferredCuisinesService.Setup(s => s.GetPreferredCuisinesAsync())
            .ReturnsAsync(cuisinesResponse);
        _mockUserPreferencesService.Setup(s => s.UpsertUserPreferencesAsync(It.IsAny<UserPreferencesCreateDto>()))
            .ThrowsAsync(exception);

        await _viewModel.LoadPreferencesAsync();

        _viewModel.SelectedDietType = SampleDietTypes[0];
        _viewModel.SelectedPreferredCuisine = SampleCuisines[0];

        // Act
        await _viewModel.SavePreferencesAsync();

        // Assert
        _mockDisplayService.Verify(s => s.ShowToast("Network error: Network error"), Times.Once);
    }

    [Fact]
    public async Task SavePreferencesAsync_UnexpectedError_ShowsToast()
    {
        // Arrange
        var dietTypesResponse = new DietTypesResponseDto(SampleDietTypes);
        var cuisinesResponse = new PreferredCuisinesResponseDto(SampleCuisines);
        var exception = new InvalidOperationException("Unexpected error");

        _mockDietTypesService.Setup(s => s.GetDietTypesAsync())
            .ReturnsAsync(dietTypesResponse);
        _mockPreferredCuisinesService.Setup(s => s.GetPreferredCuisinesAsync())
            .ReturnsAsync(cuisinesResponse);
        _mockUserPreferencesService.Setup(s => s.UpsertUserPreferencesAsync(It.IsAny<UserPreferencesCreateDto>()))
            .ThrowsAsync(exception);

        await _viewModel.LoadPreferencesAsync();

        _viewModel.SelectedDietType = SampleDietTypes[0];
        _viewModel.SelectedPreferredCuisine = SampleCuisines[0];

        // Act
        await _viewModel.SavePreferencesAsync();

        // Assert
        _mockDisplayService.Verify(s => s.ShowToast("Failed to save preferences: Unexpected error"), Times.Once);
    }


    [Fact]
    public async Task LogoutAsync_Success_ShowsToast()
    {
        // Arrange
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = true };
        _mockAuthService.Setup(s => s.LogoutAsync())
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.LogoutAsync();

        // Assert
        _mockAuthService.Verify(s => s.LogoutAsync(), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("Logged out successfully"), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_Failure_ShowsAlert()
    {
        // Arrange
        var errorMessage = "Logout failed";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = false, ErrorMessage = errorMessage };
        _mockAuthService.Setup(s => s.LogoutAsync())
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.LogoutAsync();

        // Assert
        _mockAuthService.Verify(s => s.LogoutAsync(), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Logout Error", errorMessage, "OK"), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_Exception_ShowsAlert()
    {
        // Arrange
        var exception = new InvalidOperationException("Service unavailable");
        _mockAuthService.Setup(s => s.LogoutAsync())
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LogoutAsync();

        // Assert
        _mockAuthService.Verify(s => s.LogoutAsync(), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Error", "Logout failed: Service unavailable", It.IsAny<string>()), Times.Once);
    }


    [Fact]
    public async Task ChangePasswordAsync_PasswordsDoNotMatch_ShowsAlert()
    {
        // Arrange
        _viewModel.NewPassword = "password123";
        _viewModel.ConfirmNewPassword = "different123";

        // Act
        await _viewModel.ChangePasswordAsync();

        // Assert
        _mockAuthService.Verify(s => s.ChangePasswordAsync(It.IsAny<string>()), Times.Never);
        _mockDisplayService.Verify(s => s.DisplayAlert("Validation Error", "Passwords do not match.", "OK"), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task ChangePasswordAsync_NewPasswordTooShort_ShowsValidationError()
    {
        // Arrange
        _viewModel.NewPassword = "123"; // Less than 6 characters
        _viewModel.ConfirmNewPassword = "123";

        // Act
        await _viewModel.ChangePasswordAsync();

        // Assert
        _mockAuthService.Verify(s => s.ChangePasswordAsync(It.IsAny<string>()), Times.Never);
        Assert.True(_viewModel.HasErrors);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task ChangePasswordAsync_NewPasswordNull_ShowsValidationError()
    {
        // Arrange
        _viewModel.NewPassword = null;
        _viewModel.ConfirmNewPassword = "password123";

        // Act
        await _viewModel.ChangePasswordAsync();

        // Assert
        _mockAuthService.Verify(s => s.ChangePasswordAsync(It.IsAny<string>()), Times.Never);
        Assert.True(_viewModel.HasErrors);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task ChangePasswordAsync_ConfirmNewPasswordNull_ShowsValidationError()
    {
        // Arrange
        _viewModel.NewPassword = "password123";
        _viewModel.ConfirmNewPassword = null;

        // Act
        await _viewModel.ChangePasswordAsync();

        // Assert
        _mockAuthService.Verify(s => s.ChangePasswordAsync(It.IsAny<string>()), Times.Never);
        Assert.True(_viewModel.HasErrors);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidData_Success_ChangesPasswordAndClearsFields()
    {
        // Arrange
        var newPassword = "newpassword123";
        _viewModel.NewPassword = newPassword;
        _viewModel.ConfirmNewPassword = newPassword;

        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = true };
        _mockAuthService.Setup(s => s.ChangePasswordAsync(newPassword))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.ChangePasswordAsync();

        // Assert
        _mockAuthService.Verify(s => s.ChangePasswordAsync(newPassword), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("Password changed successfully!"), Times.Once);
        Assert.Null(_viewModel.NewPassword);
        Assert.Null(_viewModel.ConfirmNewPassword);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task ChangePasswordAsync_AuthServiceFailure_ShowsAlert()
    {
        // Arrange
        var newPassword = "newpassword123";
        var errorMessage = "Password change failed";
        _viewModel.NewPassword = newPassword;
        _viewModel.ConfirmNewPassword = newPassword;

        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = false, ErrorMessage = errorMessage };
        _mockAuthService.Setup(s => s.ChangePasswordAsync(newPassword))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.ChangePasswordAsync();

        // Assert
        _mockAuthService.Verify(s => s.ChangePasswordAsync(newPassword), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Error", errorMessage, "OK"), Times.Once);
        Assert.Equal(newPassword, _viewModel.NewPassword); // Fields should not be cleared on failure
        Assert.Equal(newPassword, _viewModel.ConfirmNewPassword);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task ChangePasswordAsync_AuthServiceFailure_NullErrorMessage_ShowsDefaultMessage()
    {
        // Arrange
        var newPassword = "newpassword123";
        _viewModel.NewPassword = newPassword;
        _viewModel.ConfirmNewPassword = newPassword;

        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = false, ErrorMessage = null };
        _mockAuthService.Setup(s => s.ChangePasswordAsync(newPassword))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.ChangePasswordAsync();

        // Assert
        _mockDisplayService.Verify(s => s.DisplayAlert("Error", "Failed to change password.", "OK"), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task ChangePasswordAsync_Exception_ShowsAlert()
    {
        // Arrange
        var newPassword = "newpassword123";
        var exceptionMessage = "Network error";
        _viewModel.NewPassword = newPassword;
        _viewModel.ConfirmNewPassword = newPassword;

        var exception = new HttpRequestException(exceptionMessage);
        _mockAuthService.Setup(s => s.ChangePasswordAsync(newPassword))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.ChangePasswordAsync();

        // Assert
        _mockAuthService.Verify(s => s.ChangePasswordAsync(newPassword), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Error", $"Failed to change password: {exceptionMessage}", "OK"), Times.Once);
        Assert.Equal(newPassword, _viewModel.NewPassword); // Fields should not be cleared on exception
        Assert.Equal(newPassword, _viewModel.ConfirmNewPassword);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task ChangePasswordAsync_SetsLoadingStateCorrectly()
    {
        // Arrange
        var newPassword = "newpassword123";
        _viewModel.NewPassword = newPassword;
        _viewModel.ConfirmNewPassword = newPassword;

        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = true };
        _mockAuthService.Setup(s => s.ChangePasswordAsync(newPassword))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.ChangePasswordAsync();

        // Assert - Loading state should be cleared after operation completes
        Assert.False(_viewModel.IsLoading);
    }

}
