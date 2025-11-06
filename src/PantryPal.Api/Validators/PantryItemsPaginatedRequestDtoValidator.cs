using FluentValidation;
using PantryPal.Data;

namespace PantryPal.Api.Validators;

/// <summary>
/// Validator for PantryItemsPaginatedRequestDto
/// Ensures pagination parameters are valid
/// </summary>
public class PantryItemsPaginatedRequestDtoValidator : AbstractValidator<PantryItemsPaginatedRequestDto>
{
    public PantryItemsPaginatedRequestDtoValidator()
    {
        RuleFor(dto => dto.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(dto => dto.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(dto => dto.Sort)
            .Must(sort => sort == "created_at" || sort == "name")
            .WithMessage("Sort must be either 'created_at' or 'name'.");
    }
}
