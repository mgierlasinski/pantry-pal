using FluentValidation.TestHelper;
using PantryPal.Api.Validators;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Validators;

/// <summary>
/// Unit tests for PantryItemCreateDtoValidator
/// </summary>
public class PantryItemCreateDtoValidatorTests
{
    private readonly PantryItemCreateDtoValidator _validator;

    public PantryItemCreateDtoValidatorTests()
    {
        _validator = new PantryItemCreateDtoValidator();
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Apple")]
    [InlineData("Fresh Organic Tomatoes")]
    [MemberData(nameof(GetValidNamesData))]
    public void Validate_Name_ValidValues_ShouldNotHaveValidationError(string name)
    {
        // Arrange
        var dto = new PantryItemCreateDto(Name: name);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(dto => dto.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Name_NullOrEmptyOrWhitespace_ShouldHaveValidationError(string name)
    {
        // Arrange
        var dto = new PantryItemCreateDto(Name: name);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.Name)
            .WithErrorMessage("Name is required");
    }

    [Theory]
    [MemberData(nameof(GetInvalidNamesData))]
    public void Validate_Name_TooLong_ShouldHaveValidationError(string name)
    {
        // Arrange
        var dto = new PantryItemCreateDto(Name: name);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.Name)
            .WithErrorMessage("Name must be 1–100 characters");
    }

    [Fact]
    public void Validate_ValidName_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemCreateDto(Name: "Organic Apples");

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameAtMinimumLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemCreateDto(Name: "A");

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameAtMaximumLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemCreateDto(Name: GetValidNamesData().First()[0] as string);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameWithSpecialCharacters_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemCreateDto(Name: "Apples & Bananas!");

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameWithNumbers_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemCreateDto(Name: "Milk 2L");

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    public static IEnumerable<object[]> GetValidNamesData()
    {
        yield return new object[] { new string('A', 100) };
    }

    public static IEnumerable<object[]> GetInvalidNamesData()
    {
        yield return new object[] { new string('A', 101) };
        yield return new object[] { new string('A', 200) };
    }
}
