using FluentValidation;
using PantryPal.Api.Repositories;
using PantryPal.Data;

namespace PantryPal.Api.Validators;

/// <summary>
/// Validator for RecipeRejectRequestDto
/// Ensures reject reason ID is valid and exists in the database
/// </summary>
public class RecipeRejectRequestDtoValidator : AbstractValidator<RecipeRejectRequestDto>
{
    public RecipeRejectRequestDtoValidator(IRecipeRejectReasonsRepository rejectReasonsRepository)
    {
        RuleFor(dto => dto.RejectReasonId)
            .GreaterThan((short)0)
            .WithMessage("Reject reason ID must be greater than 0")
            .MustAsync(async (rejectReasonId, cancellation) =>
            {
                var rejectReason = await rejectReasonsRepository.GetByIdAsync(rejectReasonId);
                return rejectReason != null;
            })
            .WithMessage("Reject reason ID does not exist");
    }
}
