using FluentValidation;
using PantryPal.Data;

namespace PantryPal.Api.Validators;

/// <summary>
/// Validator for RecipesPaginatedRequestDto
/// Ensures pagination parameters are valid
/// </summary>
public class RecipesPaginatedRequestDtoValidator : AbstractValidator<RecipesPaginatedRequestDto>
{
    public RecipesPaginatedRequestDtoValidator()
    {
        RuleFor(dto => dto.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(dto => dto.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
