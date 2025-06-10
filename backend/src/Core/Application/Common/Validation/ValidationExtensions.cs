using FluentValidation;
using System;

namespace FSH.WebApi.Application.Common.Validation;

public static class CustomValidatorExtensions
{
    public static IRuleBuilderOptions<T, Guid> NotEmptyGuid<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder.Must(x => x != Guid.Empty)
            .WithMessage("'{PropertyName}' must not be an empty Guid.");
            // Changed message slightly for clarity
    }

    public static IRuleBuilderOptions<T, Guid?> NotEmptyGuid<T>(this IRuleBuilder<T, Guid?> ruleBuilder)
    {
        return ruleBuilder.Must(x => x.HasValue && x.Value != Guid.Empty)
            .WithMessage("'{PropertyName}' must not be an empty Guid.")
            // Apply this rule only when x has a value.
            // If x is null, it's considered valid by this specific rule (other rules like NotNull can handle nullability).
            .When(x => x.HasValue);
    }
}
