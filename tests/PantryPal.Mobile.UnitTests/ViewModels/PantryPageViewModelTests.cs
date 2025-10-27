using Moq;
using PantryPal.Data;
using PantryPal.Mobile.Services;
using PantryPal.Mobile.ViewModels;
using System.Net;

namespace PantryPal.Mobile.UnitTests.ViewModels;

public class PantryPageViewModelTests
{
    private readonly Mock<IPantryService> _mockPantryService;
    private readonly PantryPageViewModel _viewModel;

    public PantryPageViewModelTests()
    {
        _mockPantryService = new Mock<IPantryService>();
        _viewModel = new PantryPageViewModel(_mockPantryService.Object);
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
    public async Task LoadItemsAsync_NetworkError_SetsErrorMessage()
    {
        // Arrange
        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        await _viewModel.LoadItemsAsync();

        // Assert
        Assert.Contains("Network error", _viewModel.ErrorMessage);
        Assert.True(_viewModel.ShowErrorSnackbar);
    }

    [Fact]
    public async Task AddItemAsync_ValidName_AddsItemToCollection()
    {
        // Arrange
        var newItem = new PantryItemDto("3", "Carrots", false, DateTime.UtcNow.ToString(), DateTime.UtcNow.ToString());
        _mockPantryService.Setup(s => s.CreatePantryItemAsync(It.IsAny<PantryItemCreateDto>()))
            .ReturnsAsync(newItem);

        _viewModel.DialogItemName = "Carrots";

        // Act
        await _viewModel.AddItemAsync();

        // Assert
        Assert.Single(_viewModel.Items);
        Assert.Equal("Carrots", _viewModel.Items[0].Name);
        Assert.False(_viewModel.IsEmpty);
        Assert.Equal(1, _viewModel.Total);
        Assert.Empty(_viewModel.DialogItemName);
    }

    [Fact]
    public async Task AddItemAsync_EmptyName_DoesNotCallService()
    {
        // Arrange
        _viewModel.DialogItemName = "";

        // Act
        // The method will try to display an alert which fails in unit tests
        // We're verifying that the service is never called
        try
        {
            await _viewModel.AddItemAsync();
        }
        catch (NullReferenceException)
        {
            // Expected in unit tests due to Shell.Current being null
        }

        // Assert
        Assert.Empty(_viewModel.Items);
        _mockPantryService.Verify(s => s.CreatePantryItemAsync(It.IsAny<PantryItemCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_NameTooLong_DoesNotCallService()
    {
        // Arrange
        _viewModel.DialogItemName = new string('a', 101);

        // Act
        // The method will try to display an alert which fails in unit tests
        // We're verifying that the service is never called
        try
        {
            await _viewModel.AddItemAsync();
        }
        catch (NullReferenceException)
        {
            // Expected in unit tests due to Shell.Current being null
        }

        // Assert
        Assert.Empty(_viewModel.Items);
        _mockPantryService.Verify(s => s.CreatePantryItemAsync(It.IsAny<PantryItemCreateDto>()), Times.Never);
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

        _viewModel.DialogItemName = "Cherry Tomatoes";
        _viewModel.SelectedItemId = "1";

        // Act
        await _viewModel.EditItemAsync();

        // Assert
        Assert.Single(_viewModel.Items);
        Assert.Equal("Cherry Tomatoes", _viewModel.Items[0].Name);
        Assert.Empty(_viewModel.DialogItemName);
        Assert.Empty(_viewModel.SelectedItemId);
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

        _viewModel.SelectedItemId = "1";

        // Act
        await _viewModel.ConfirmDeleteAsync();

        // Assert
        Assert.Empty(_viewModel.Items);
        Assert.True(_viewModel.IsEmpty);
        Assert.Equal(0, _viewModel.Total);
        Assert.Empty(_viewModel.SelectedItemId);
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
        Assert.Contains("Network error", _viewModel.ErrorMessage);
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
    public void HasError_WhenErrorMessageSet_ReturnsTrue()
    {
        // Arrange
        _viewModel.ErrorMessage = "Some error";

        // Act & Assert
        Assert.True(_viewModel.HasError);
    }

    [Fact]
    public void DismissError_ClearsErrorState()
    {
        // Arrange
        _viewModel.ErrorMessage = "Some error";
        _viewModel.ShowErrorSnackbar = true;

        // Act
        _viewModel.DismissError();

        // Assert
        Assert.Empty(_viewModel.ErrorMessage);
        Assert.False(_viewModel.ShowErrorSnackbar);
    }

    [Fact]
    public async Task RetryAsync_CallsLoadItemsAsync()
    {
        // Arrange
        var response = new PantryItemsPaginatedResponseDto([], 1, 20, 0);
        _mockPantryService.Setup(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(response);

        _viewModel.ErrorMessage = "Some error";
        _viewModel.ShowErrorSnackbar = true;

        // Act
        await _viewModel.RetryAsync();

        // Assert
        Assert.Empty(_viewModel.ErrorMessage);
        Assert.False(_viewModel.ShowErrorSnackbar);
        _mockPantryService.Verify(s => s.GetPantryItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }
}

