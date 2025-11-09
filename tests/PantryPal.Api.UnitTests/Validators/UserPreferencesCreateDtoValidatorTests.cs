using FluentValidation.TestHelper;
using Moq;
using PantryPal.Api.Repositories;
using PantryPal.Api.Validators;
using PantryPal.Data;

namespace PantryPal.Api.UnitTests.Validators;

/// <summary>
/// Unit tests for UserPreferencesCreateDtoValidator
/// </summary>
public class UserPreferencesCreateDtoValidatorTests
{
    private readonly Mock<IUserPreferencesRepository> _mockRepository;
    private readonly UserPreferencesCreateDtoValidator _validator;

    public UserPreferencesCreateDtoValidatorTests()
    {
        _mockRepository = new Mock<IUserPreferencesRepository>();
        _validator = new UserPreferencesCreateDtoValidator(_mockRepository.Object);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(short.MaxValue)]
    public void Validate_DietTypeId_ValidValues_ShouldNotHaveValidationError(short dietTypeId)
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(
            DietTypeId: dietTypeId,
            PreferredCuisineId: 1,
            DislikedIngredients: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(dto => dto.DietTypeId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(short.MinValue)]
    public void Validate_DietTypeId_InvalidValues_ShouldHaveValidationError(short dietTypeId)
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(
            DietTypeId: dietTypeId,
            PreferredCuisineId: 1,
            DislikedIngredients: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.DietTypeId)
            .WithErrorMessage("Diet type ID must be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(short.MaxValue)]
    public void Validate_PreferredCuisineId_ValidValues_ShouldNotHaveValidationError(short preferredCuisineId)
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(
            DietTypeId: 1,
            PreferredCuisineId: preferredCuisineId,
            DislikedIngredients: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(dto => dto.PreferredCuisineId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(short.MinValue)]
    public void Validate_PreferredCuisineId_InvalidValues_ShouldHaveValidationError(short preferredCuisineId)
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(
            DietTypeId: 1,
            PreferredCuisineId: preferredCuisineId,
            DislikedIngredients: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.PreferredCuisineId)
            .WithErrorMessage("Preferred cuisine ID must be greater than 0");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Tomatoes")]
    [InlineData("Tomatoes, Onions")]
    [MemberData(nameof(GetValidDislikedIngredientsData))]
    public void Validate_DislikedIngredients_ValidValues_ShouldNotHaveValidationError(string dislikedIngredients)
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(
            DietTypeId: 1,
            PreferredCuisineId: 1,
            DislikedIngredients: dislikedIngredients);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(dto => dto.DislikedIngredients);
    }

    [Theory]
    [MemberData(nameof(GetInvalidDislikedIngredientsData))]
    public void Validate_DislikedIngredients_TooLong_ShouldHaveValidationError(string dislikedIngredients)
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(
            DietTypeId: 1,
            PreferredCuisineId: 1,
            DislikedIngredients: dislikedIngredients);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.DislikedIngredients)
            .WithErrorMessage("Disliked ingredients must not exceed 1000 characters");
    }

    [Fact]
    public void Validate_AllValidFields_ShouldPassValidation()
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(
            DietTypeId: 2,
            PreferredCuisineId: 5,
            DislikedIngredients: "Tomatoes, Onions");

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_AllInvalidFields_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(
            DietTypeId: 0,
            PreferredCuisineId: -1,
            DislikedIngredients: "A".PadRight(1001, 'a'));

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(dto => dto.DietTypeId);
        result.ShouldHaveValidationErrorFor(dto => dto.PreferredCuisineId);
        result.ShouldHaveValidationErrorFor(dto => dto.DislikedIngredients);
        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void Validate_ValidDtoWithNullDislikedIngredients_ShouldPassValidation()
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(
            DietTypeId: 1,
            PreferredCuisineId: 1,
            DislikedIngredients: null);

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidDtoWithEmptyDislikedIngredients_ShouldPassValidation()
    {
        // Arrange
        var dto = new UserPreferencesCreateDto(
            DietTypeId: 1,
            PreferredCuisineId: 1,
            DislikedIngredients: "");

        // Act & Assert
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    public static IEnumerable<object[]> GetValidDislikedIngredientsData()
    {
        yield return new object[] { new string('A', 1000) };
    }

    public static IEnumerable<object[]> GetInvalidDislikedIngredientsData()
    {
        yield return new object[] { new string('A', 1001) };
        yield return new object[] { new string('A', 2000) };
    }
}
