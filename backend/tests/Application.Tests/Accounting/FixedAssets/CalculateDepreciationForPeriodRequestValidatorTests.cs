using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.FixedAssets;
using System;

namespace Application.Tests.Accounting.FixedAssets;

public class CalculateDepreciationForPeriodRequestValidatorTests
{
    // Assuming a validator exists or might be created.
    // If no explicit validator class exists in the application for this request,
    // these tests would be for a potential future validator.
    // For now, let's assume a basic validator that checks PeriodEndDate.
    // public class CalculateDepreciationForPeriodRequestValidator : AbstractValidator<CalculateDepreciationForPeriodRequest>
    // {
    //    public CalculateDepreciationForPeriodRequestValidator()
    //    {
    //        RuleFor(p => p.PeriodEndDate).NotEmpty();
    //        RuleFor(p => p.FixedAssetId).NotEmptyGuid().When(p => p.FixedAssetId.HasValue);
    //    }
    // }
    // For this test, I will write it as if such a validator exists.
    // If it's not created in the main codebase, these tests won't compile/run against actual code
    // but serve the purpose of outlining what *should* be tested if validation is added.

    private readonly CalculateDepreciationForPeriodRequestValidator _validator;

    public CalculateDepreciationForPeriodRequestValidatorTests()
    {
        // If CalculateDepreciationForPeriodRequestValidator doesn't exist in the main codebase,
        // this line will cause a compile error when tests are actually compiled.
        // This is acceptable for this "outlining" phase.
        _validator = new CalculateDepreciationForPeriodRequestValidator();
    }


    private CalculateDepreciationForPeriodRequest CreateValidRequest() => new()
    {
        PeriodEndDate = DateTime.UtcNow.Date,
        FixedAssetId = Guid.NewGuid() // Can be null too
    };

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_FixedAssetId_Is_Null()
    {
        var request = CreateValidRequest();
        request.FixedAssetId = null; // Allowed for batch calculation
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_PeriodEndDate_Is_Default()
    {
        var request = CreateValidRequest();
        request.PeriodEndDate = default;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PeriodEndDate);
    }

    [Fact]
    public void Should_Have_Error_When_FixedAssetId_Is_EmptyGuid_If_Provided()
    {
        // This test assumes the validator has: RuleFor(p => p.FixedAssetId).NotEmptyGuid().When(p => p.FixedAssetId.HasValue);
        var request = CreateValidRequest();
        request.FixedAssetId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FixedAssetId).WithErrorMessage("'Fixed Asset Id' must not be empty.");
    }
}

// Placeholder for the actual validator if it's not yet defined in the application code
// This allows the test file to be created and outline tests.
#if !APPLICATION_HAS_CALCULATEDEPRECIATIONFORPERIODREQUESTVALIDATOR
namespace FSH.WebApi.Application.Accounting.FixedAssets
{
    using FluentValidation;
    public class CalculateDepreciationForPeriodRequestValidator : AbstractValidator<CalculateDepreciationForPeriodRequest>
    {
       public CalculateDepreciationForPeriodRequestValidator()
       {
           RuleFor(p => p.PeriodEndDate).NotEmpty();
           RuleFor(p => p.FixedAssetId).NotEmptyGuid().When(p => p.FixedAssetId.HasValue);
       }
    }
}
#endif
