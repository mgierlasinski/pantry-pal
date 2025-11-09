using FluentValidation.TestHelper;
using Moq;
using PantryPal.Api.Db;
using PantryPal.Api.Repositories;
using PantryPal.Api.Validators;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Validators;

/// <summary>
/// Unit tests for RecipeRejectRequestDtoValidator
/// </summary>
public class RecipeRejectRequestDtoValidatorTests
{
    private readonly Mock<IRecipeRejectReasonsRepository> _mockRepository;
    private readonly RecipeRejectRequestDtoValidator _validator;

    public RecipeRejectRequestDtoValidatorTests()
    {
        _mockRepository = new Mock<IRecipeRejectReasonsRepository>();
        _validator = new RecipeRejectRequestDtoValidator(_mockRepository.Object);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(short.MaxValue)]
    public async Task Validate_RejectReasonId_ValidValuesExistInDatabase_ShouldNotHaveValidationError(short rejectReasonId)
    {
        // Arrange
        var dto = new RecipeRejectRequestDto(RejectReasonId: rejectReasonId);
        _mockRepository
            .Setup(r => r.GetByIdAsync(rejectReasonId))
            .ReturnsAsync(new RecipeRejectReasonsSelect()); // Mock existing entity

        // Act & Assert
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveValidationErrorFor(dto => dto.RejectReasonId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(short.MinValue)]
    public async Task Validate_RejectReasonId_InvalidValues_ShouldHaveValidationError(short rejectReasonId)
    {
        // Arrange
        var dto = new RecipeRejectRequestDto(RejectReasonId: rejectReasonId);

        // Act & Assert
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.RejectReasonId)
            .WithErrorMessage("Reject reason ID must be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task Validate_RejectReasonId_ValidValuesNotExistInDatabase_ShouldHaveValidationError(short rejectReasonId)
    {
        // Arrange
        var dto = new RecipeRejectRequestDto(RejectReasonId: rejectReasonId);
        _mockRepository
            .Setup(r => r.GetByIdAsync(rejectReasonId))
            .ReturnsAsync((RecipeRejectReasonsSelect?)null); // Mock non-existing entity

        // Act & Assert
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.RejectReasonId)
            .WithErrorMessage("Reject reason ID does not exist");
    }

    [Fact]
    public async Task Validate_ValidRejectReasonId_ShouldPassValidation()
    {
        // Arrange
        var dto = new RecipeRejectRequestDto(RejectReasonId: 2);
        _mockRepository
            .Setup(r => r.GetByIdAsync((short)2))
            .ReturnsAsync(new RecipeRejectReasonsSelect()); // Mock existing entity

        // Act & Assert
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_InvalidRejectReasonIdZero_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new RecipeRejectRequestDto(RejectReasonId: 0);

        // Act & Assert
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.RejectReasonId)
            .WithErrorMessage("Reject reason ID must be greater than 0");
    }

    [Fact]
    public async Task Validate_RejectReasonIdNotFoundInDatabase_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new RecipeRejectRequestDto(RejectReasonId: 999);
        _mockRepository
            .Setup(r => r.GetByIdAsync((short)999))
            .ReturnsAsync((RecipeRejectReasonsSelect?)null); // Mock non-existing entity

        // Act & Assert
        var result = await _validator.TestValidateAsync(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.RejectReasonId)
            .WithErrorMessage("Reject reason ID does not exist");
    }

    [Fact]
    public async Task Validate_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var dto = new RecipeRejectRequestDto(RejectReasonId: 1);
        _mockRepository
            .Setup(r => r.GetByIdAsync((short)1))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _validator.TestValidateAsync(dto));
    }
}
