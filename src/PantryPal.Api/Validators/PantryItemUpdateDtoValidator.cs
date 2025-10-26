using FluentValidation;
using PantryPal.Data;

namespace PantryPal.Api.Validators;

/// <summary>
/// Validator for PantryItemUpdateDto
/// Ensures at least one field is provided and validates field constraints
/// </summary>
public class PantryItemUpdateDtoValidator : AbstractValidator<PantryItemUpdateDto>
{
    public PantryItemUpdateDtoValidator()
    {
        // At least one field must be provided
        RuleFor(dto => dto)
            .Must(dto => dto.Name != null || dto.IsFavorite != null)
            .WithMessage("At least one field (name or is_favorite) must be provided for update");

        // If name is provided, validate its length
        When(dto => dto.Name != null, () =>
        {
            RuleFor(dto => dto.Name)
                .NotEmpty()
                .WithMessage("Name cannot be empty")
                .Length(1, 100)
                .WithMessage("Name must be between 1 and 100 characters");
        });
    }
}
