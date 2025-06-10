using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class UpdateFixedAssetRequestValidator : CustomValidator<UpdateFixedAssetRequest>
{
    public UpdateFixedAssetRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmptyGuid();

        RuleFor(p => p.AssetName)
            .NotEmpty()
            .MaximumLength(100)
            .When(p => p.AssetName is not null);

        RuleFor(p => p.Description)
            .MaximumLength(500)
            .When(p => p.Description is not null);

        RuleFor(p => p.AssetCategoryId)
            .NotEmptyGuid()
            .When(p => p.AssetCategoryId.HasValue);

        RuleFor(p => p.PurchaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Purchase date cannot be in the future.")
            .When(p => p.PurchaseDate.HasValue);

        RuleFor(p => p.PurchaseCost)
            .GreaterThanOrEqualTo(0)
            .When(p => p.PurchaseCost.HasValue);

        RuleFor(p => p.SalvageValue)
            .GreaterThanOrEqualTo(0)
            // Cannot easily validate SalvageValue <= PurchaseCost here without knowing which one is being updated or fetching existing entity.
            // This cross-field validation is better handled in the domain entity's Update method or the handler.
            .When(p => p.SalvageValue.HasValue);

        // Cross-field validation: If both PurchaseCost and SalvageValue are provided for update, ensure new SalvageValue <= new PurchaseCost
        // RuleFor(p => p)
        //    .Must(p => (p.SalvageValue ?? 0) <= (p.PurchaseCost ?? decimal.MaxValue))
        //    .When(p => p.SalvageValue.HasValue && p.PurchaseCost.HasValue)
        //    .WithMessage("Salvage Value must be less than or equal to Purchase Cost.");
        // This still doesn't account for existing values if only one is updated. Best in handler/domain.

        RuleFor(p => p.UsefulLifeYears)
            .GreaterThan(0)
            .WithMessage("Useful Life in years must be positive.")
            .When(p => p.UsefulLifeYears.HasValue);

        RuleFor(p => p.DepreciationMethodId)
            .NotEmptyGuid()
            .When(p => p.DepreciationMethodId.HasValue);

        RuleFor(p => p.Location)
            .MaximumLength(100)
            .When(p => p.Location is not null);

        // Status can be any valid enum value.
    }
}
