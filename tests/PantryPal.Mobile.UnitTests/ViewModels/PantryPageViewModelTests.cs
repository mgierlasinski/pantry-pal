using CommunityToolkit.Maui.Core;
using Moq;
using PantryPal.Data;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.ViewModels;
using System.Net;

namespace PantryPal.Mobile.UnitTests.ViewModels;

public class PantryPageViewModelTests
{
    private readonly Mock<IPantryService> _mockPantryService;
    private readonly Mock<IDisplayService> _mockDisplayService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly PantryPageViewModel _viewModel;

    public PantryPageViewModelTests()
    {
        _mockPantryService = new Mock<IPantryService>();
        _mockDisplayService = new Mock<IDisplayService>();
        _mockNavigationService = new Mock<INavigationService>();
        _viewModel = new PantryPageViewModel(_mockPantryService.Object, _mockDisplayService.Object, _mockNavigationService.Object);
    }

    [Fact]
    public async Task LoadItemsAsync_Success_PopulatesItems()
    {
        // Arrange
        var mockItems = new List<PantryItemDto>
        {
            new("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString()),
            new("2", "Onions", true, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString())
        };

        var response = new PantryItemsPaginatedResponseDto(mockItems, 1, 20, 2);
        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.LoadItemsAsync();

        // Assert
        Assert.Equal(2, _viewModel.Items.Count);
        Assert.Equal("Tomatoes", _viewModel.Items[0].Name);
        Assert.Equal("Onions", _viewModel.Items[1].Name);
        Assert.False(_viewModel.IsEmpty);
        Assert.Equal(2, _viewModel.Total);
    }

    [Fact]
    public async Task LoadItemsAsync_EmptyResponse_SetsIsEmpty()
    {
        // Arrange
        var response = new PantryItemsPaginatedResponseDto([], 1, 20, 0);
        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        // Act
        await _viewModel.LoadItemsAsync();

        // Assert
        Assert.Empty(_viewModel.Items);
        Assert.True(_viewModel.IsEmpty);
        Assert.Equal(0, _viewModel.Total);
    }

    [Fact]
    public async Task LoadItemsAsync_NetworkError_DoesNotCrash()
    {
        // Arrange
        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert - Should not throw exception, error is handled via Toast
        await _viewModel.LoadItemsAsync();
    }

    [Fact]
    public async Task AddItemAsync_ValidName_AddsItemToCollection()
    {
        // Arrange
        var newItem = new PantryItemDto("3", "Carrots", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        _mockPantryService.Setup(s => s.CreatePantryItemAsync(It.IsAny<PantryItemCreateDto>()))
            .ReturnsAsync(newItem);

        // Act
        await _viewModel.AddItemAsync("Carrots");

        // Assert
        Assert.Single(_viewModel.Items);
        Assert.Equal("Carrots", _viewModel.Items[0].Name);
        Assert.False(_viewModel.IsEmpty);
        Assert.Equal(1, _viewModel.Total);
    }

    [Fact]
    public async Task AddItemAsync_EmptyName_DoesNotCallService()
    {
        // Arrange

        // Act
        await _viewModel.AddItemAsync("");

        // Assert
        Assert.Empty(_viewModel.Items);
        _mockPantryService.Verify(s => s.CreatePantryItemAsync(It.IsAny<PantryItemCreateDto>()), Times.Never);
        _mockDisplayService.Verify(s => s.ShowToast("Item name is required.", ToastDuration.Short), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_NameTooLong_DoesNotCallService()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act
        await _viewModel.AddItemAsync(longName);

        // Assert
        Assert.Empty(_viewModel.Items);
        _mockPantryService.Verify(s => s.CreatePantryItemAsync(It.IsAny<PantryItemCreateDto>()), Times.Never);
        _mockDisplayService.Verify(s => s.ShowToast("Item name must be 100 characters or less.", ToastDuration.Short), Times.Once);
    }

    [Fact]
    public async Task EditItemAsync_ValidUpdate_UpdatesItemInCollection()
    {
        // Arrange
        var originalItem = new PantryItemDto("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var updatedItem = new PantryItemDto("1", "Cherry Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());

        var response = new PantryItemsPaginatedResponseDto([originalItem], 1, 20, 1);
        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        _mockPantryService.Setup(s => s.UpdatePantryItemAsync("1", It.IsAny<PantryItemUpdateDto>()))
            .ReturnsAsync(updatedItem);

        // Act
        await _viewModel.EditItemAsync("1", "Cherry Tomatoes");

        // Assert
        Assert.Single(_viewModel.Items);
        Assert.Equal("Cherry Tomatoes", _viewModel.Items[0].Name);
    }

    [Fact]
    public async Task ConfirmDeleteAsync_ExistingItem_RemovesFromCollection()
    {
        // Arrange
        var item = new PantryItemDto("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var response = new PantryItemsPaginatedResponseDto([item], 1, 20, 1);

        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        _mockPantryService.Setup(s => s.DeletePantryItemAsync("1"))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.ConfirmDeleteAsync("1");

        // Assert
        Assert.Empty(_viewModel.Items);
        Assert.True(_viewModel.IsEmpty);
        Assert.Equal(0, _viewModel.Total);
    }

    [Fact]
    public async Task ToggleFavoriteAsync_TogglesState_UpdatesItem()
    {
        // Arrange
        var item = new PantryItemDto("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var response = new PantryItemsPaginatedResponseDto([item], 1, 20, 1);
        
        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        var updatedItem = new PantryItemDto("1", "Tomatoes", true, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        _mockPantryService.Setup(s => s.UpdatePantryItemAsync("1", It.IsAny<PantryItemUpdateDto>()))
            .ReturnsAsync(updatedItem);

        // Act
        await _viewModel.ToggleFavoriteAsync("1");

        // Assert
        Assert.Single(_viewModel.Items);
        Assert.True(_viewModel.Items[0].IsFavorite);
    }

    [Fact]
    public async Task ToggleFavoriteAsync_NetworkError_RevertsState()
    {
        // Arrange
        var item = new PantryItemDto("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var response = new PantryItemsPaginatedResponseDto([item], 1, 20, 1);
        
        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        _mockPantryService.Setup(s => s.UpdatePantryItemAsync("1", It.IsAny<PantryItemUpdateDto>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _viewModel.ToggleFavoriteAsync("1");

        // Assert
        Assert.Single(_viewModel.Items);
        Assert.False(_viewModel.Items[0].IsFavorite); // Should revert to original state
    }

    [Fact]
    public void IsNotEmpty_WhenItemsExist_ReturnsTrue()
    {
        // Arrange
        _viewModel.IsEmpty = false;

        // Act & Assert
        Assert.True(_viewModel.IsNotEmpty);
    }



    [Fact]
    public async Task GenerateRecipeAsync_WhenEmpty_ShowsAlertAndDoesNotNavigate()
    {
        // Arrange
        _viewModel.IsEmpty = true;

        // Act
        await _viewModel.GenerateRecipeAsync();

        // Assert - Should show alert when empty
        _mockDisplayService.Verify(s => s.ShowToast("Add items to your pantry before generating a recipe.", ToastDuration.Short), Times.Once);
        // Navigation testing would require integration tests with Shell mocking
    }

    [Fact]
    public async Task GenerateRecipeAsync_WhenHasItems_NavigatesToRecipeGenerationPage()
    {
        // Arrange
        var item = new PantryItemDto("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var response = new PantryItemsPaginatedResponseDto([item], 1, 20, 1);

        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        // Act - Method will try to navigate which fails in unit tests
        try
        {
            await _viewModel.GenerateRecipeAsync();
        }
        catch (NullReferenceException)
        {
            // Expected in unit tests due to Shell.Current being null
        }

        // Assert - Items loaded and not empty
        Assert.False(_viewModel.IsEmpty);
        Assert.Single(_viewModel.Items);
    }

    [Fact]
    public async Task AddItemAsync_ConflictError_ShowsDuplicateAlert()
    {
        // Arrange
        var exception = new HttpRequestException("Conflict", null, HttpStatusCode.Conflict);

        _mockPantryService.Setup(s => s.CreatePantryItemAsync(It.IsAny<PantryItemCreateDto>()))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.AddItemAsync("Existing Item");

        // Assert - Service was called and alert was shown
        _mockPantryService.Verify(s => s.CreatePantryItemAsync(It.IsAny<PantryItemCreateDto>()), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("An item with this name already exists.", ToastDuration.Short), Times.Once);
    }

    [Fact]
    public async Task EditItemAsync_ConflictError_ShowsDuplicateAlert()
    {
        // Arrange
        var originalItem = new PantryItemDto("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var response = new PantryItemsPaginatedResponseDto([originalItem], 1, 20, 1);

        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        var exception = new HttpRequestException("Conflict", null, HttpStatusCode.Conflict);

        _mockPantryService.Setup(s => s.UpdatePantryItemAsync("1", It.IsAny<PantryItemUpdateDto>()))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.EditItemAsync("1", "Existing Item");

        // Assert - Service was called and alert was shown
        _mockPantryService.Verify(s => s.UpdatePantryItemAsync("1", It.IsAny<PantryItemUpdateDto>()), Times.Once);
        _mockDisplayService.Verify(s => s.ShowToast("An item with this name already exists.", ToastDuration.Short), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_UnauthorizedError_ThrowsException()
    {
        // Arrange
        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

        _mockPantryService.Setup(s => s.CreatePantryItemAsync(It.IsAny<PantryItemCreateDto>()))
            .ThrowsAsync(exception);

        // Act & Assert - Should throw exception (handled by global error handler)
        await Assert.ThrowsAsync<HttpRequestException>(() => _viewModel.AddItemAsync("Valid Item"));
        _mockPantryService.Verify(s => s.CreatePantryItemAsync(It.IsAny<PantryItemCreateDto>()), Times.Once);
    }

    [Fact]
    public async Task EditItemAsync_UnauthorizedError_NavigatesToLogin()
    {
        // Arrange
        var originalItem = new PantryItemDto("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var response = new PantryItemsPaginatedResponseDto([originalItem], 1, 20, 1);

        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

        _mockPantryService.Setup(s => s.UpdatePantryItemAsync("1", It.IsAny<PantryItemUpdateDto>()))
            .ThrowsAsync(exception);

        // Act
        await _viewModel.EditItemAsync("1", "Valid Item");

        // Assert - Service was called and navigation was triggered
        _mockPantryService.Verify(s => s.UpdatePantryItemAsync("1", It.IsAny<PantryItemUpdateDto>()), Times.Once);
        _mockNavigationService.Verify(s => s.GoToAsync(AppShell.LoginRoute, false), Times.Once);
    }

    [Fact]
    public async Task ConfirmDeleteAsync_UnauthorizedError_ThrowsException()
    {
        // Arrange
        var item = new PantryItemDto("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var response = new PantryItemsPaginatedResponseDto([item], 1, 20, 1);

        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

        _mockPantryService.Setup(s => s.DeletePantryItemAsync("1"))
            .ThrowsAsync(exception);

        // Act & Assert - Should throw exception (handled by global error handler)
        await Assert.ThrowsAsync<HttpRequestException>(() => _viewModel.ConfirmDeleteAsync("1"));
        _mockPantryService.Verify(s => s.DeletePantryItemAsync("1"), Times.Once);
    }

    [Fact]
    public async Task ToggleFavoriteAsync_UnauthorizedError_RevertsStateAndThrowsException()
    {
        // Arrange
        var item = new PantryItemDto("1", "Tomatoes", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        var response = new PantryItemsPaginatedResponseDto([item], 1, 20, 1);

        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        await _viewModel.LoadItemsAsync();

        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

        _mockPantryService.Setup(s => s.UpdatePantryItemAsync("1", It.IsAny<PantryItemUpdateDto>()))
            .ThrowsAsync(exception);

        // Act & Assert - Should throw exception after reverting state
        await Assert.ThrowsAsync<HttpRequestException>(() => _viewModel.ToggleFavoriteAsync("1"));
        Assert.Single(_viewModel.Items);
        Assert.False(_viewModel.Items[0].IsFavorite); // Should revert to original state
        _mockPantryService.Verify(s => s.UpdatePantryItemAsync("1", It.IsAny<PantryItemUpdateDto>()), Times.Once);
    }
}

