using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class DisposeFixedAssetRequestValidator : CustomValidator<DisposeFixedAssetRequest>
{
    public DisposeFixedAssetRequestValidator()
    {
        RuleFor(p => p.FixedAssetId)
            .NotEmptyGuid();

        RuleFor(p => p.DisposalDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)) // Allow slight future date for planned disposal entry
            .WithMessage("Disposal date cannot be too far in the future.");

        RuleFor(p => p.DisposalReason)
            .MaximumLength(500)
            .When(p => !string.IsNullOrEmpty(p.DisposalReason)); // Validate only if provided

        RuleFor(p => p.DisposalAmount)
            .GreaterThanOrEqualTo(0)
            .When(p => p.DisposalAmount.HasValue);
    }
}
