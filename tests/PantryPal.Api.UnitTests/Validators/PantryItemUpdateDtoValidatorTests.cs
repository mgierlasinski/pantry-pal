using FluentValidation.TestHelper;
using PantryPal.Api.Validators;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Validators;

/// <summary>
/// Unit tests for PantryItemUpdateDtoValidator
/// </summary>
public class PantryItemUpdateDtoValidatorTests
{
    private readonly PantryItemUpdateDtoValidator _validator;

    public PantryItemUpdateDtoValidatorTests()
    {
        _validator = new PantryItemUpdateDtoValidator();
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Apple")]
    [InlineData("Fresh Organic Tomatoes")]
    [MemberData(nameof(GetValidNamesData))]
    public void Validate_Name_ValidValuesWithNameProvided_ShouldNotHaveValidationError(string name)
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: name, IsFavorite: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(dto => dto.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Name_EmptyOrWhitespaceWithNameProvided_ShouldHaveValidationError(string name)
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: name, IsFavorite: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.Name)
            .WithErrorMessage("Name cannot be empty");
    }

    [Theory]
    [MemberData(nameof(GetInvalidNamesData))]
    public void Validate_Name_TooLongWithNameProvided_ShouldHaveValidationError(string name)
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: name, IsFavorite: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.Name)
            .WithErrorMessage("Name must be between 1 and 100 characters");
    }

    [Fact]
    public void Validate_NameProvidedButNull_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: null, IsFavorite: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto)
            .WithErrorMessage("At least one field (name or is_favorite) must be provided for update");
    }

    [Fact]
    public void Validate_NoFieldsProvided_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: null, IsFavorite: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto)
            .WithErrorMessage("At least one field (name or is_favorite) must be provided for update");
    }

    [Fact]
    public void Validate_OnlyNameProvidedValid_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: "Updated Apple", IsFavorite: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_OnlyIsFavoriteProvided_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: null, IsFavorite: true);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_BothFieldsProvidedValidName_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: "Updated Apple", IsFavorite: false);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_BothFieldsProvidedInvalidName_ShouldHaveValidationError()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: "", IsFavorite: true);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.Name)
            .WithErrorMessage("Name cannot be empty");
    }

    [Fact]
    public void Validate_NameAtMinimumLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: "A", IsFavorite: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameAtMaximumLength_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: GetValidNamesData().First()[0] as string, IsFavorite: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameWithSpecialCharacters_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: "Apples & Bananas!", IsFavorite: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_IsFavoriteFalse_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: null, IsFavorite: false);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_IsFavoriteTrue_ShouldPassValidation()
    {
        // Arrange
        var dto = new PantryItemUpdateDto(Name: null, IsFavorite: true);

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
