using FluentValidation;
using FSH.WebApi.Application.Common.Validation; // For NotEmptyGuid

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class CreateAssetCategoryRequestValidator : CustomValidator<CreateAssetCategoryRequest>
{
    public CreateAssetCategoryRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(p => p.Description)
            .MaximumLength(256);

        RuleFor(p => p.DefaultDepreciationMethodId)
            .NotEmptyGuid()
            .When(p => p.DefaultDepreciationMethodId.HasValue);

        RuleFor(p => p.DefaultUsefulLifeYears)
            .InclusiveBetween(1, 100)
            .When(p => p.DefaultUsefulLifeYears.HasValue)
            .WithMessage("Default useful life in years must be between 1 and 100 if specified.");
    }
}
