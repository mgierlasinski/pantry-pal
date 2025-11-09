using FluentValidation.TestHelper;
using PantryPal.Api.Validators;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Validators;

/// <summary>
/// Unit tests for PantryItemsPaginatedRequestDtoValidator
/// </summary>
public class PantryItemsPaginatedRequestDtoValidatorTests
{
    private readonly PantryItemsPaginatedRequestDtoValidator _validator;

    public PantryItemsPaginatedRequestDtoValidatorTests()
    {
        _validator = new PantryItemsPaginatedRequestDtoValidator();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Validate_Page_ValidValues_ShouldNotHaveValidationError(int page)
    {
        // Arrange
        var dto = new PantryItemsPaginatedRequestDto(Page: page, PageSize: 20, Sort: "created_at");

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
        var dto = new PantryItemsPaginatedRequestDto(Page: page, PageSize: 20, Sort: "created_at");

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
        var dto = new PantryItemsPaginatedRequestDto(Page: 1, PageSize: pageSize, Sort: "created_at");

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
        var dto = new PantryItemsPaginatedRequestDto(Page: 1, PageSize: pageSize, Sort: "created_at");

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.PageSize)
            .WithErrorMessage("PageSize must be between 1 and 100.");
    }

    [Theory]
    [InlineData("created_at")]
    [InlineData("name")]
    public void Validate_Sort_ValidValues_ShouldNotHaveValidationError(string sort)
    {
        // Arrange
        var dto = new PantryItemsPaginatedRequestDto(Page: 1, PageSize: 20, Sort: sort);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(dto => dto.Sort);
    }

    [Theory]
    [InlineData("invalid_sort")]
    [InlineData("updated_at")]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_Sort_InvalidValues_ShouldHaveValidationError(string sort)
    {
        // Arrange
        var dto = new PantryItemsPaginatedRequestDto(Page: 1, PageSize: 20, Sort: sort);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.Sort)
            .WithErrorMessage("Sort must be either 'created_at' or 'name'.");
    }

    [Fact]
    public void Validate_AllValidFields_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemsPaginatedRequestDto(Page: 2, PageSize: 50, Sort: "name");

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_AllInvalidFields_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var dto = new PantryItemsPaginatedRequestDto(Page: 0, PageSize: 150, Sort: "invalid");

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.Page);
        result.ShouldHaveValidationErrorFor(dto => dto.PageSize);
        result.ShouldHaveValidationErrorFor(dto => dto.Sort);
        Assert.Equal(3, result.Errors.Count);
    }
}
