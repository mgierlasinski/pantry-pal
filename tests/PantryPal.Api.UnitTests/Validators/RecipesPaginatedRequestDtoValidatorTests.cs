using FluentValidation.TestHelper;
using PantryPal.Api.Validators;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Validators;

/// <summary>
/// Unit tests for RecipesPaginatedRequestDtoValidator
/// </summary>
public class RecipesPaginatedRequestDtoValidatorTests
{
    private readonly RecipesPaginatedRequestDtoValidator _validator;

    public RecipesPaginatedRequestDtoValidatorTests()
    {
        _validator = new RecipesPaginatedRequestDtoValidator();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Validate_Page_ValidValues_ShouldNotHaveValidationError(int page)
    {
        // Arrange
        var dto = new RecipesPaginatedRequestDto(Page: page, PageSize: 20);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(dto => dto.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_Page_InvalidValues_ShouldHaveValidationError(int page)
    {
        // Arrange
        var dto = new RecipesPaginatedRequestDto(Page: page, PageSize: 20);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.Page)
            .WithErrorMessage("Page must be greater than or equal to 1.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_PageSize_ValidValues_ShouldNotHaveValidationError(int pageSize)
    {
        // Arrange
        var dto = new RecipesPaginatedRequestDto(Page: 1, PageSize: pageSize);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(dto => dto.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(200)]
    public void Validate_PageSize_InvalidValues_ShouldHaveValidationError(int pageSize)
    {
        // Arrange
        var dto = new RecipesPaginatedRequestDto(Page: 1, PageSize: pageSize);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.PageSize)
            .WithErrorMessage("PageSize must be between 1 and 100.");
    }

    [Fact]
    public void Validate_AllValidFields_ShouldPassValidation()
    {
        // Arrange
        var dto = new RecipesPaginatedRequestDto(Page: 2, PageSize: 50);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_AllInvalidFields_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var dto = new RecipesPaginatedRequestDto(Page: 0, PageSize: 150);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.Page);
        result.ShouldHaveValidationErrorFor(dto => dto.PageSize);
        Assert.Equal(2, result.Errors.Count);
    }
}
