using FluentValidation;
using PantryPal.Api.Repositories;
using PantryPal.Data;

namespace PantryPal.Api.Validators;

/// <summary>
/// Validator for UserPreferencesCreateDto
/// Ensures diet type and cuisine IDs exist in the database, and disliked ingredients meet length requirements
/// </summary>
public class UserPreferencesCreateDtoValidator : AbstractValidator<UserPreferencesCreateDto>
{
    public UserPreferencesCreateDtoValidator(IUserPreferencesRepository userPreferencesRepository)
    {
        RuleFor(dto => dto.DietTypeId)
            .GreaterThan((short)0)
            .WithMessage("Diet type ID must be greater than 0")
            .MustAsync(async (dietTypeId, cancellation) =>
            {
                return await userPreferencesRepository.DietTypeExistsAsync(dietTypeId);
            })
            .WithMessage("Diet type ID does not exist");

        RuleFor(dto => dto.PreferredCuisineId)
            .GreaterThan((short)0)
            .WithMessage("Preferred cuisine ID must be greater than 0")
            .MustAsync(async (preferredCuisineId, cancellation) =>
            {
                return await userPreferencesRepository.PreferredCuisineExistsAsync(preferredCuisineId);
            })
            .WithMessage("Preferred cuisine ID does not exist");

        RuleFor(dto => dto.DislikedIngredients)
            .MaximumLength(1000)
            .WithMessage("Disliked ingredients must not exceed 1000 characters")
            .When(dto => dto.DislikedIngredients != null);
    }
}
