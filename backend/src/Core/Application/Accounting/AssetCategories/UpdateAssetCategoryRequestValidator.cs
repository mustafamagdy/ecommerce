using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class UpdateAssetCategoryRequestValidator : CustomValidator<UpdateAssetCategoryRequest>
{
    public UpdateAssetCategoryRequestValidator()
    {
        RuleFor(p => p.Id)
            .NotEmptyGuid();

        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(100)
            .When(p => p.Name is not null);

        RuleFor(p => p.Description)
            .MaximumLength(256)
            .When(p => p.Description is not null);

        RuleFor(p => p.DefaultDepreciationMethodId)
            .NotEmptyGuid()
            .When(p => p.DefaultDepreciationMethodId.HasValue); // Validate only if a value is provided

        RuleFor(p => p.DefaultUsefulLifeYears)
            .InclusiveBetween(1, 100)
            .When(p => p.DefaultUsefulLifeYears.HasValue)
            .WithMessage("Default useful life in years must be between 1 and 100 if specified.");
    }
}
