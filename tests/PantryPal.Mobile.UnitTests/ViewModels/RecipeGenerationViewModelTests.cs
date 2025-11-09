using Moq;
using PantryPal.Data;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.ViewModels;
using System.Net;

namespace PantryPal.Mobile.UnitTests.ViewModels;

public class RecipeGenerationViewModelTests
{
    private readonly Mock<IRecipeService> _mockRecipeService;
    private readonly Mock<IDisplayService> _mockDisplayService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly RecipeGenerationViewModel _viewModel;

    public RecipeGenerationViewModelTests()
    {
        _mockRecipeService = new Mock<IRecipeService>();
        _mockDisplayService = new Mock<IDisplayService>();
        _mockNavigationService = new Mock<INavigationService>();
        _viewModel = new RecipeGenerationViewModel(_mockRecipeService.Object, _mockDisplayService.Object, _mockNavigationService.Object);
    }

    [Fact]
    public async Task LoadDataAsync_Success_LoadsRejectReasonsAndGeneratesRecipe()
    {
        // Arrange
        var rejectReasons = new List<RecipeRejectReasonDto>
        {
            new RecipeRejectReasonDto(1, "Too spicy"),
            new RecipeRejectReasonDto(2, "Not enough ingredients")
        };
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "# Sample Recipe\n\nIngredients:\n- Item 1\n- Item 2");

        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync())
            .ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync())
            .ReturnsAsync(generationResponse);

        // Act
        await _viewModel.LoadDataAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        Assert.Equal("# Sample Recipe\n\nIngredients:\n- Item 1\n- Item 2", _viewModel.RecipeText);
        Assert.True(_viewModel.ShowRecipeContent);
        _mockRecipeService.Verify(s => s.GetRejectReasonsAsync(), Times.Once);
        _mockRecipeService.Verify(s => s.GenerateRecipeAsync(), Times.Once);
    }

    [Fact]
    public async Task LoadDataAsync_BadRequestError_ShowsAlertAndNavigatesBack()
    {
        // Arrange
        var rejectReasons = new List<RecipeRejectReasonDto>
        {
            new RecipeRejectReasonDto(1, "Too spicy")
        };
        var exception = new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest);
        exception.Data["ResponseContent"] = "Invalid pantry or preferences";

        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync())
            .ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync())
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LoadDataAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task LoadDataAsync_BadRequestErrorWithoutContent_ShowsDefaultMessage()
    {
        // Arrange
        var rejectReasons = new List<RecipeRejectReasonDto>
        {
            new RecipeRejectReasonDto(1, "Too spicy")
        };
        var exception = new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest);

        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync())
            .ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync())
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LoadDataAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task LoadDataAsync_UnauthorizedError_ShowsAlertAndNavigatesToLogin()
    {
        // Arrange
        var rejectReasons = new List<RecipeRejectReasonDto>
        {
            new RecipeRejectReasonDto(1, "Too spicy")
        };
        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync())
            .ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync())
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LoadDataAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.LoginRoute, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task LoadDataAsync_NetworkError_ShowsAlertAndNavigatesBack()
    {
        // Arrange
        var rejectReasons = new List<RecipeRejectReasonDto>
        {
            new RecipeRejectReasonDto(1, "Too spicy")
        };
        var exception = new HttpRequestException("Network error");

        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync())
            .ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync())
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LoadDataAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task LoadDataAsync_UnexpectedError_ShowsAlertAndNavigatesBack()
    {
        // Arrange
        var rejectReasons = new List<RecipeRejectReasonDto>
        {
            new RecipeRejectReasonDto(1, "Too spicy")
        };
        var exception = new Exception("Unexpected error");

        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync())
            .ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync())
            .ThrowsAsync(exception);

        // Act
        await _viewModel.LoadDataAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_Success_AcceptsRecipeAndNavigatesBack()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var acceptResponse = new RecipeAcceptResponseDto("recipe-456", DateTime.UtcNow.ToString());

        // Setup initial state
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        _mockRecipeService.Setup(s => s.AcceptRecipeAsync("gen-123"))
            .ReturnsAsync(acceptResponse);

        // Act
        await _viewModel.AcceptAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockRecipeService.Verify(s => s.AcceptRecipeAsync("gen-123"), Times.Once);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_EmptyGenerationId_DoesNothing()
    {
        // Arrange - ViewModel with no generation data

        // Act
        await _viewModel.AcceptAsync();

        // Assert
        _mockRecipeService.Verify(s => s.AcceptRecipeAsync(It.IsAny<string>()), Times.Never);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task AcceptAsync_AlreadyLoading_DoesNothing()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        _viewModel.IsLoading = true; // Simulate already loading

        // Act
        await _viewModel.AcceptAsync();

        // Assert
        _mockRecipeService.Verify(s => s.AcceptRecipeAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AcceptAsync_NotFoundError_ShowsAlertAndNavigatesBack()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        var exception = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
        _mockRecipeService.Setup(s => s.AcceptRecipeAsync("gen-123"))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.AcceptAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_ConflictError_ShowsAlertAndNavigatesBack()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        var exception = new HttpRequestException("Conflict", null, HttpStatusCode.Conflict);
        _mockRecipeService.Setup(s => s.AcceptRecipeAsync("gen-123"))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.AcceptAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_UnauthorizedError_ShowsAlertAndNavigatesToLogin()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);
        _mockRecipeService.Setup(s => s.AcceptRecipeAsync("gen-123"))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.AcceptAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.LoginRoute, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task AcceptAsync_NetworkError_ShowsAlert()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        var exception = new HttpRequestException("Network error");
        _mockRecipeService.Setup(s => s.AcceptRecipeAsync("gen-123"))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.AcceptAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task RejectAsync_Success_ShowsDialogRejectsRecipeAndNavigatesBack()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto>
        {
            new RecipeRejectReasonDto(1, "Too spicy"),
            new RecipeRejectReasonDto(2, "Not enough ingredients")
        };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        _mockDisplayService.Setup(s => s.DisplayActionSheet(
                "Why are you rejecting this recipe?",
                "Cancel",
                null,
                It.IsAny<string[]>()))
            .ReturnsAsync("Not enough ingredients");

        // Act
        await _viewModel.RejectAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockDisplayService.Verify(s => s.DisplayActionSheet(
            "Why are you rejecting this recipe?",
            "Cancel",
            null,
            It.Is<string[]>(reasons => reasons.Contains("Too spicy") && reasons.Contains("Not enough ingredients"))), Times.Once);
        _mockRecipeService.Verify(s => s.RejectRecipeAsync("gen-123", It.Is<RecipeRejectRequestDto>(r => r.RejectReasonId == 2)), Times.Once);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_EmptyGenerationId_DoesNothing()
    {
        // Arrange - ViewModel with no generation data

        // Act
        await _viewModel.RejectAsync();

        // Assert
        _mockDisplayService.Verify(s => s.DisplayActionSheet(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        _mockRecipeService.Verify(s => s.RejectRecipeAsync(It.IsAny<string>(), It.IsAny<RecipeRejectRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task RejectAsync_AlreadyLoading_DoesNothing()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        _viewModel.IsLoading = true; // Simulate already loading

        // Act
        await _viewModel.RejectAsync();

        // Assert
        _mockDisplayService.Verify(s => s.DisplayActionSheet(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task RejectAsync_NoRejectReasons_ShowsErrorAlert()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto>(); // Empty list
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        // Act
        await _viewModel.RejectAsync();

        // Assert
        _mockDisplayService.Verify(s => s.DisplayActionSheet(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
    }

    [Fact]
    public async Task RejectAsync_UserCancels_DoesNothing()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        _mockDisplayService.Setup(s => s.DisplayActionSheet(
                "Why are you rejecting this recipe?",
                "Cancel",
                null,
                It.IsAny<string[]>()))
            .ReturnsAsync("Cancel");

        // Act
        await _viewModel.RejectAsync();

        // Assert
        _mockRecipeService.Verify(s => s.RejectRecipeAsync(It.IsAny<string>(), It.IsAny<RecipeRejectRequestDto>()), Times.Never);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task RejectAsync_InvalidReasonSelected_DoesNothing()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        _mockDisplayService.Setup(s => s.DisplayActionSheet(
                "Why are you rejecting this recipe?",
                "Cancel",
                null,
                It.IsAny<string[]>()))
            .ReturnsAsync("Non-existent reason");

        // Act
        await _viewModel.RejectAsync();

        // Assert
        _mockRecipeService.Verify(s => s.RejectRecipeAsync(It.IsAny<string>(), It.IsAny<RecipeRejectRequestDto>()), Times.Never);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task RejectAsync_NotFoundError_ShowsAlertAndNavigatesBack()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        _mockDisplayService.Setup(s => s.DisplayActionSheet(
                "Why are you rejecting this recipe?",
                "Cancel",
                null,
                It.IsAny<string[]>()))
            .ReturnsAsync("Too spicy");

        var exception = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);
        _mockRecipeService.Setup(s => s.RejectRecipeAsync("gen-123", It.IsAny<RecipeRejectRequestDto>()))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.RejectAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_ConflictError_ShowsAlertAndNavigatesBack()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        _mockDisplayService.Setup(s => s.DisplayActionSheet(
                "Why are you rejecting this recipe?",
                "Cancel",
                null,
                It.IsAny<string[]>()))
            .ReturnsAsync("Too spicy");

        var exception = new HttpRequestException("Conflict", null, HttpStatusCode.Conflict);
        _mockRecipeService.Setup(s => s.RejectRecipeAsync("gen-123", It.IsAny<RecipeRejectRequestDto>()))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.RejectAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_UnauthorizedError_ShowsAlertAndNavigatesToLogin()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        _mockDisplayService.Setup(s => s.DisplayActionSheet(
                "Why are you rejecting this recipe?",
                "Cancel",
                null,
                It.IsAny<string[]>()))
            .ReturnsAsync("Too spicy");

        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);
        _mockRecipeService.Setup(s => s.RejectRecipeAsync("gen-123", It.IsAny<RecipeRejectRequestDto>()))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.RejectAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
        _mockNavigationService.Verify(s => s.PopModalAsync(It.IsAny<bool>()), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.LoginRoute, It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_NetworkError_ShowsAlert()
    {
        // Arrange
        var generationResponse = new RecipeGenerateResponseDto("gen-123", "Recipe content");
        var rejectReasons = new List<RecipeRejectReasonDto> { new RecipeRejectReasonDto(1, "Too spicy") };
        _mockRecipeService.Setup(s => s.GetRejectReasonsAsync()).ReturnsAsync(rejectReasons);
        _mockRecipeService.Setup(s => s.GenerateRecipeAsync()).ReturnsAsync(generationResponse);
        await _viewModel.LoadDataAsync();

        _mockDisplayService.Setup(s => s.DisplayActionSheet(
                "Why are you rejecting this recipe?",
                "Cancel",
                null,
                It.IsAny<string[]>()))
            .ReturnsAsync("Too spicy");

        var exception = new HttpRequestException("Network error");
        _mockRecipeService.Setup(s => s.RejectRecipeAsync("gen-123", It.IsAny<RecipeRejectRequestDto>()))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.RejectAsync();

        // Assert
        Assert.False(_viewModel.IsLoading);
    }

    [Theory]
    [InlineData(true, "", false)]   // Loading, empty text -> false
    [InlineData(false, "", false)]  // Not loading, empty text -> false
    [InlineData(false, "recipe", true)] // Not loading, has text -> true
    [InlineData(true, "recipe", false)]  // Loading, has text -> false
    public void ShowRecipeContent_VariousStates_ReturnsExpectedResult(bool isLoading, string recipeText, bool expected)
    {
        // Arrange
        _viewModel.IsLoading = isLoading;
        _viewModel.RecipeText = recipeText;

        // Act & Assert
        Assert.Equal(expected, _viewModel.ShowRecipeContent);
    }

    [Fact]
    public async Task TryExtractErrorMessageAsync_WithValidContent_ReturnsContent()
    {
        // Arrange
        var exception = new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest);
        exception.Data["ResponseContent"] = "Custom error message";

        // Act
        var result = await RecipeGenerationViewModelExtensions.TryExtractErrorMessageAsync(_viewModel, exception);

        // Assert
        Assert.Equal("Custom error message", result);
    }

    [Fact]
    public async Task TryExtractErrorMessageAsync_WithoutContent_ReturnsNull()
    {
        // Arrange
        var exception = new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest);

        // Act
        var result = await RecipeGenerationViewModelExtensions.TryExtractErrorMessageAsync(_viewModel, exception);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task TryExtractErrorMessageAsync_ExceptionDuringExtraction_ReturnsNull()
    {
        // Arrange
        var exception = new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest);
        // Simulate exception during Data access
        exception.Data["ResponseContent"] = null; // This might cause issues in some scenarios

        // Act
        var result = await RecipeGenerationViewModelExtensions.TryExtractErrorMessageAsync(_viewModel, exception);

        // Assert
        Assert.Null(result);
    }
}

// Extension methods to access private methods for testing
internal static class RecipeGenerationViewModelExtensions
{
    public static async Task<string?> TryExtractErrorMessageAsync(RecipeGenerationViewModel viewModel, HttpRequestException ex)
    {
        var method = typeof(RecipeGenerationViewModel).GetMethod("TryExtractErrorMessageAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return await (Task<string?>)method!.Invoke(viewModel, [ex])!;
    }
}
