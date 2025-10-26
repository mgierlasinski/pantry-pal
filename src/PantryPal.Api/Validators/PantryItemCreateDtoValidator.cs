using FluentValidation;
using PantryPal.Data;

namespace PantryPal.Api.Validators;

/// <summary>
/// Validator for PantryItemCreateDto
/// Ensures name is provided and meets length requirements
/// </summary>
public class PantryItemCreateDtoValidator : AbstractValidator<PantryItemCreateDto>
{
    public PantryItemCreateDtoValidator()
    {
        RuleFor(dto => dto.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .NotNull()
            .WithMessage("Name is required")
            .Length(1, 100)
            .WithMessage("Name must be 1–100 characters");
    }
}
