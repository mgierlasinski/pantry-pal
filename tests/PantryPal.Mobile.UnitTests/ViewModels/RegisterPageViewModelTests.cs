using Moq;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.UnitTests.ViewModels;

public class RegisterPageViewModelTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IDisplayService> _mockDisplayService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly RegisterPageViewModel _viewModel;

    public RegisterPageViewModelTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockDisplayService = new Mock<IDisplayService>();
        _mockNavigationService = new Mock<INavigationService>();

        _viewModel = new RegisterPageViewModel(
            _mockAuthService.Object,
            _mockDisplayService.Object,
            _mockNavigationService.Object);
    }

    [Fact]
    public async Task RegisterAsync_IsLoadingTrue_PreventsMultipleCalls()
    {
        // Arrange
        _viewModel.IsLoading = true;
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        _viewModel.ConfirmPassword = "password123";

        // Act
        await _viewModel.RegisterAsync();

        // Assert
        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.True(_viewModel.IsLoading);
    }

    [Theory]
    [InlineData("", "password123", "password123", "Email is required.")]
    [InlineData("test@example.com", "", "password123", "Password is required.")]
    [InlineData("test@example.com", "password123", "", "Please confirm your password.")]
    public async Task RegisterAsync_RequiredFieldsEmpty_ShowsValidationError(string email, string password, string confirmPassword, string expectedMessage)
    {
        // Arrange
        _viewModel.Email = email;
        _viewModel.Password = password;
        _viewModel.ConfirmPassword = confirmPassword;

        // Act
        await _viewModel.RegisterAsync();

        // Assert
        _mockDisplayService.Verify(s => s.DisplayAlert("Validation Error", It.IsAny<string>(), "OK"), Times.Once);
        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Theory]
    [InlineData("invalid-email", "password123", "password123", "Please enter a valid email address.")]
    [InlineData("invalid-email", "", "password123", "Please enter a valid email address.\nPassword is required.")]
    [InlineData("", "password123", "", "Email is required.\nPlease confirm your password.")]
    public async Task RegisterAsync_InvalidEmailFormat_ShowsValidationError(string email, string password, string confirmPassword, string expectedMessage)
    {
        // Arrange
        _viewModel.Email = email;
        _viewModel.Password = password;
        _viewModel.ConfirmPassword = confirmPassword;

        // Act
        await _viewModel.RegisterAsync();

        // Assert
        _mockDisplayService.Verify(s => s.DisplayAlert("Validation Error", It.IsAny<string>(), "OK"), Times.Once);
        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Theory]
    [InlineData("test@example.com", "short", "short", "Password must be at least 6 characters long.")]
    [InlineData("test@example.com", "", "", "Password is required.\nPlease confirm your password.")]
    public async Task RegisterAsync_PasswordTooShort_ShowsValidationError(string email, string password, string confirmPassword, string expectedMessage)
    {
        // Arrange
        _viewModel.Email = email;
        _viewModel.Password = password;
        _viewModel.ConfirmPassword = confirmPassword;

        // Act
        await _viewModel.RegisterAsync();

        // Assert
        _mockDisplayService.Verify(s => s.DisplayAlert("Validation Error", It.IsAny<string>(), "OK"), Times.Once);
        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task RegisterAsync_PasswordsDoNotMatch_ShowsValidationError()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        _viewModel.ConfirmPassword = "differentpassword";

        // Act
        await _viewModel.RegisterAsync();

        // Assert
        _mockDisplayService.Verify(s => s.DisplayAlert("Validation Error", "Passwords do not match.", "OK"), Times.Once);
        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task RegisterAsync_ValidInput_Success_ShowsToastAndNavigatesToLogin()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        _viewModel.ConfirmPassword = "password123";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = true };

        _mockAuthService.Setup(s => s.RegisterAsync("test@example.com", "password123"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.RegisterAsync();

        // Assert
        _mockAuthService.Verify(s => s.RegisterAsync("test@example.com", "password123"), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("Registration successful! Please check your email for verification."), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.LoginRoute, It.IsAny<bool>()), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task RegisterAsync_AuthServiceReturnsFailure_ShowsErrorAlert()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        _viewModel.ConfirmPassword = "password123";
        var errorMessage = "Email already exists";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = false, ErrorMessage = errorMessage };

        _mockAuthService.Setup(s => s.RegisterAsync("test@example.com", "password123"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.RegisterAsync();

        // Assert
        _mockAuthService.Verify(s => s.RegisterAsync("test@example.com", "password123"), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Registration Failed", errorMessage, "OK"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task RegisterAsync_AuthServiceReturnsFailureWithoutMessage_ShowsDefaultError()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        _viewModel.ConfirmPassword = "password123";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = false };

        _mockAuthService.Setup(s => s.RegisterAsync("test@example.com", "password123"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.RegisterAsync();

        // Assert
        _mockAuthService.Verify(s => s.RegisterAsync("test@example.com", "password123"), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Registration Failed", "An unexpected error occurred.", "OK"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task RegisterAsync_ExceptionThrown_ShowsErrorAlert()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        _viewModel.ConfirmPassword = "password123";
        var exception = new InvalidOperationException("Network error");

        _mockAuthService.Setup(s => s.RegisterAsync("test@example.com", "password123"))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.RegisterAsync();

        // Assert
        _mockAuthService.Verify(s => s.RegisterAsync("test@example.com", "password123"), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Error", "Registration failed: Network error", "OK"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task RegisterAsync_EmailWithWhitespace_TrimsEmailBeforeRegistration()
    {
        // Arrange
        _viewModel.Email = "  test@example.com  ";
        _viewModel.Password = "password123";
        _viewModel.ConfirmPassword = "password123";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = true };

        _mockAuthService.Setup(s => s.RegisterAsync("test@example.com", "password123"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.RegisterAsync();

        // Assert
        _mockAuthService.Verify(s => s.RegisterAsync("test@example.com", "password123"), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("Registration successful! Please check your email for verification."), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.LoginRoute, It.IsAny<bool>()), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task NavigateToLoginAsync_CallsNavigationServiceWithLoginRoute()
    {
        // Act
        await _viewModel.NavigateToLoginAsync();

        // Assert
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.LoginRoute, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public void TogglePasswordVisibility_FalseToTrue_ChangesToTrue()
    {
        // Arrange
        _viewModel.IsPasswordVisible = false;

        // Act
        _viewModel.TogglePasswordVisibility();

        // Assert
        Assert.True(_viewModel.IsPasswordVisible);
    }

    [Fact]
    public void TogglePasswordVisibility_TrueToFalse_ChangesToFalse()
    {
        // Arrange
        _viewModel.IsPasswordVisible = true;

        // Act
        _viewModel.TogglePasswordVisibility();

        // Assert
        Assert.False(_viewModel.IsPasswordVisible);
    }

    [Fact]
    public void ToggleConfirmPasswordVisibility_FalseToTrue_ChangesToTrue()
    {
        // Arrange
        _viewModel.IsConfirmPasswordVisible = false;

        // Act
        _viewModel.ToggleConfirmPasswordVisibility();

        // Assert
        Assert.True(_viewModel.IsConfirmPasswordVisible);
    }

    [Fact]
    public void ToggleConfirmPasswordVisibility_TrueToFalse_ChangesToFalse()
    {
        // Arrange
        _viewModel.IsConfirmPasswordVisible = true;

        // Act
        _viewModel.ToggleConfirmPasswordVisibility();

        // Assert
        Assert.False(_viewModel.IsConfirmPasswordVisible);
    }
}
