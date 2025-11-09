using Moq;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.ViewModels;

namespace PantryPal.Mobile.UnitTests.ViewModels;

public class ForgotPasswordPageViewModelTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IDisplayService> _mockDisplayService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly ForgotPasswordPageViewModel _viewModel;

    public ForgotPasswordPageViewModelTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockDisplayService = new Mock<IDisplayService>();
        _mockNavigationService = new Mock<INavigationService>();

        _viewModel = new ForgotPasswordPageViewModel(
            _mockAuthService.Object,
            _mockDisplayService.Object,
            _mockNavigationService.Object);
    }

    [Fact]
    public async Task SendResetEmailAsync_IsLoadingTrue_PreventsMultipleCalls()
    {
        // Arrange
        _viewModel.IsLoading = true;
        _viewModel.Email = "test@example.com";

        // Act
        await _viewModel.SendResetEmailAsync();

        // Assert
        _mockAuthService.Verify(s => s.SendPasswordResetEmailAsync(It.IsAny<string>()), Times.Never);
        Assert.True(_viewModel.IsLoading);
    }

    [Fact]
    public async Task SendResetEmailAsync_EmptyEmail_ShowsValidationError()
    {
        // Arrange
        _viewModel.Email = "";

        // Act
        await _viewModel.SendResetEmailAsync();

        // Assert
        _mockDisplayService.Verify(s => s.DisplayAlert("Validation Error", "Email is required.", "OK"), Times.Once);
        _mockAuthService.Verify(s => s.SendPasswordResetEmailAsync(It.IsAny<string>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    public async Task SendResetEmailAsync_InvalidEmailFormat_ShowsValidationError(string invalidEmail)
    {
        // Arrange
        _viewModel.Email = invalidEmail;

        // Act
        await _viewModel.SendResetEmailAsync();

        // Assert
        _mockDisplayService.Verify(s => s.DisplayAlert("Validation Error", "Please enter a valid email address.", "OK"), Times.Once);
        _mockAuthService.Verify(s => s.SendPasswordResetEmailAsync(It.IsAny<string>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task SendResetEmailAsync_ValidEmail_Success_ShowsToastAndNavigates()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = true };

        _mockAuthService.Setup(s => s.SendPasswordResetEmailAsync("test@example.com"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.SendResetEmailAsync();

        // Assert
        _mockAuthService.Verify(s => s.SendPasswordResetEmailAsync("test@example.com"), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("If an account with this email exists, a password reset link has been sent."), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.LoginRoute, It.IsAny<bool>()), Times.Once);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task SendResetEmailAsync_AuthServiceReturnsFailure_ShowsErrorAlert()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        var errorMessage = "Service temporarily unavailable";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = false, ErrorMessage = errorMessage };

        _mockAuthService.Setup(s => s.SendPasswordResetEmailAsync("test@example.com"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.SendResetEmailAsync();

        // Assert
        _mockAuthService.Verify(s => s.SendPasswordResetEmailAsync("test@example.com"), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Error", errorMessage, "OK"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task SendResetEmailAsync_AuthServiceReturnsFailureWithoutMessage_ShowsDefaultError()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = false };

        _mockAuthService.Setup(s => s.SendPasswordResetEmailAsync("test@example.com"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.SendResetEmailAsync();

        // Assert
        _mockAuthService.Verify(s => s.SendPasswordResetEmailAsync("test@example.com"), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Error", "An unexpected error occurred.", "OK"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task SendResetEmailAsync_ExceptionThrown_ShowsErrorAlert()
    {
        // Arrange
        _viewModel.Email = "test@example.com";
        var exception = new InvalidOperationException("Network connection failed");

        _mockAuthService.Setup(s => s.SendPasswordResetEmailAsync("test@example.com"))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.SendResetEmailAsync();

        // Assert
        _mockAuthService.Verify(s => s.SendPasswordResetEmailAsync("test@example.com"), Times.Once);
        _mockDisplayService.Verify(s => s.DisplayAlert("Error", $"Failed to send reset email: {exception.Message}", "OK"), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task SendResetEmailAsync_EmailWithWhitespace_TrimsEmailBeforeSending()
    {
        // Arrange
        _viewModel.Email = "  test@example.com  ";
        var authResult = new PantryPal.Mobile.Models.AuthResult { IsSuccess = true };

        _mockAuthService.Setup(s => s.SendPasswordResetEmailAsync("test@example.com"))
            .ReturnsAsync(authResult);

        // Act
        await _viewModel.SendResetEmailAsync();

        // Assert
        _mockAuthService.Verify(s => s.SendPasswordResetEmailAsync("test@example.com"), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("If an account with this email exists, a password reset link has been sent."), Times.Once);
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
}
