using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid
using FSH.WebApi.Domain.Accounting; // For AssetCategory, DepreciationMethod (to potentially check if active)
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository (if checking existence)

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class CreateFixedAssetRequestValidator : CustomValidator<CreateFixedAssetRequest>
{
    // Using IReadRepository<T> to check existence of related entities.
    // This is optional; some prefer these checks in the handler.
    // For this example, let's assume these repositories are NOT injected into the validator
    // and such checks (if AssetCategory/DepreciationMethod are active/exist) are done in the handler.
    // If they were injected, they'd be used in MustAsync rules.

    public CreateFixedAssetRequestValidator(/* IReadRepository<AssetCategory> categoryRepo, IReadRepository<DepreciationMethod> depMethodRepo */)
    {
        RuleFor(p => p.AssetNumber)
            .NotEmpty()
            .MaximumLength(50);
            // Add .MustAsync(async (number, ct) => !await assetRepo.AnyAsync(new FixedAssetByAssetNumberSpec(number), ct))
            // .WithMessage("Asset Number must be unique.") if assetRepo is injected. This check is better in handler to avoid race conditions.

        RuleFor(p => p.AssetName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.Description)
            .MaximumLength(500);

        RuleFor(p => p.AssetCategoryId)
            .NotEmptyGuid();
            // Add .MustAsync(async (id, ct) => await categoryRepo.GetByIdAsync(id, ct) is not null /* && (await categoryRepo.GetByIdAsync(id, ct)).IsActive */)
            // .WithMessage("Asset Category not found or is inactive.") if categoryRepo is injected.

        RuleFor(p => p.PurchaseDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow) // Purchase date cannot be in the future
            .WithMessage("Purchase date cannot be in the future.");

        RuleFor(p => p.PurchaseCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(p => p.SalvageValue)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(p => p.PurchaseCost)
            .WithMessage("Salvage Value must be between zero and Purchase Cost.");

        RuleFor(p => p.UsefulLifeYears)
            .GreaterThan(0)
            .WithMessage("Useful Life in years must be positive.");

        RuleFor(p => p.DepreciationMethodId)
            .NotEmptyGuid();
            // Add .MustAsync(async (id, ct) => await depMethodRepo.GetByIdAsync(id, ct) is not null /* && (await depMethodRepo.GetByIdAsync(id, ct)).IsActive */)
            // .WithMessage("Depreciation Method not found or is inactive.") if depMethodRepo is injected.

        RuleFor(p => p.Location)
            .MaximumLength(100);
    }
}
