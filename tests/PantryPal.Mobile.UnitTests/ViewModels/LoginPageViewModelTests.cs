using Moq;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.UnitTests.ViewModels;

public class LoginPageViewModelTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IDisplayService> _mockDisplayService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly LoginPageViewModel _viewModel;

    public LoginPageViewModelTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockDisplayService = new Mock<IDisplayService>();
        _mockNavigationService = new Mock<INavigationService>();

        _viewModel = new LoginPageViewModel(
            _mockAuthService.Object,
            _mockDisplayService.Object,
            _mockNavigationService.Object);
    }

    [Fact]
    public async Task LoginAsync_IsLoadingTrue_PreventsMultipleCalls()
    {
        // Arrange
        _viewModel.IsLoading = true;
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";

        // Act
        await _viewModel.LoginAsync();

        // Assert
        _mockAuthService.Verify(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.True(_viewModel.IsLoading);
    }

    [Theory]
    [InlineData("", "password123", "Email is required.")]
    [InlineData("test@example.com", "", "Password is required.")]
    [InlineData("", "", "Email is required.", "Password is required.")]
    public async Task LoginAsync_InvalidInput_HasValidationErrors(string email, string password, params string[] expectedErrors)
    {
        // Arrange
        _viewModel.Email = email;
        _viewModel.Password = password;

        // Act
        _viewModel.LoginCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.HasErrors);
        var errors = _viewModel.GetErrors().Select(e => e.ErrorMessage).ToList();
        foreach (var expectedError in expectedErrors)
        {
            Assert.Contains(expectedError, errors);
        }
        _mockAuthService.Verify(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Theory]
    [InlineData("invalid-email", "password123", "Please enter a valid email address.")]
    [InlineData("invalid-email", "", "Please enter a valid email address.", "Password is required.")]
    public async Task LoginAsync_InvalidEmailFormat_HasValidationErrors(string email, string password, params string[] expectedErrors)
    {
        // Arrange
        _viewModel.Email = email;
        _viewModel.Password = password;

        // Act
        _viewModel.LoginCommand.Execute(null);
        
        // Assert
        Assert.True(_viewModel.HasErrors);
        var errors = _viewModel.GetErrors().Select(e => e.ErrorMessage).ToList();
        foreach (var expectedError in expectedErrors)
        {
            Assert.Contains(expectedError, errors);
        }
        _mockAuthService.Verify(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoginAsync_ValidInput_Success_NavigatesToDefaultRoute()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = true };

        _mockAuthService.Setup(s => s.LoginAsync("test@example.com", "password123"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.LoginAsync();

        // Assert
        _mockAuthService.Verify(s => s.LoginAsync("test@example.com", "password123"), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("Login successful!"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.DefaultRoute, It.IsAny<bool>()), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoginAsync_AuthServiceReturnsFailure_ShowsErrorAlert()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        var errorMessage = "Invalid credentials";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = false, ErrorMessage = errorMessage };

        _mockAuthService.Setup(s => s.LoginAsync("test@example.com", "password123"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.LoginAsync();

        // Assert
        _mockAuthService.Verify(s => s.LoginAsync("test@example.com", "password123"), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Login Failed", errorMessage, "OK"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoginAsync_AuthServiceReturnsFailureWithoutMessage_ShowsDefaultError()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = false };

        _mockAuthService.Setup(s => s.LoginAsync("test@example.com", "password123"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.LoginAsync();

        // Assert
        _mockAuthService.Verify(s => s.LoginAsync("test@example.com", "password123"), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Login Failed", "An unexpected error occurred.", "OK"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoginAsync_ExceptionThrown_ShowsErrorAlert()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        _viewModel.Password = "password123";
        var exception = new InvalidOperationException("Network error");

        _mockAuthService.Setup(s => s.LoginAsync("test@example.com", "password123"))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LoginAsync();

        // Assert
        _mockAuthService.Verify(s => s.LoginAsync("test@example.com", "password123"), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Error", "Login failed: Network error", "OK"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task LoginAsync_EmailWithWhitespace_TrimsEmailBeforeLogin()
    {
        // Arrange
        _viewModel.Email = "  test@example.com  ";
        _viewModel.Password = "password123";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = true };

        _mockAuthService.Setup(s => s.LoginAsync("test@example.com", "password123"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.LoginAsync();

        // Assert
        _mockAuthService.Verify(s => s.LoginAsync("test@example.com", "password123"), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("Login successful!"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.DefaultRoute, It.IsAny<bool>()), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task NavigateToRegisterAsync_CallsNavigationServiceWithRegisterPage()
    {
        // Act
        await _viewModel.NavigateToRegisterAsync();

        // Assert
        _mockNavigationService.Verify(s => s.GoToAsync(nameof(PantryPal.Mobile.Views.RegisterPage), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task NavigateToForgotPasswordAsync_CallsNavigationServiceWithForgotPasswordPage()
    {
        // Act
        await _viewModel.NavigateToForgotPasswordAsync();

        // Assert
        _mockNavigationService.Verify(s => s.GoToAsync(nameof(PantryPal.Mobile.Views.ForgotPasswordPage), It.IsAny<bool>()), Times.Once);
    }

}
