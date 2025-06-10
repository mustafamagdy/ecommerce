using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.FixedAssets;
using System;

namespace Application.Tests.Accounting.FixedAssets;

public class DisposeFixedAssetRequestValidatorTests
{
    private readonly DisposeFixedAssetRequestValidator _validator = new();

    private DisposeFixedAssetRequest CreateValidRequest() => new()
    {
        FixedAssetId = Guid.NewGuid(),
        DisposalDate = DateTime.UtcNow.Date,
        DisposalReason = "Sold at market value",
        DisposalAmount = 150.00m
    };

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Optional_DisposalReason_And_Amount_Are_Null()
    {
        var request = CreateValidRequest();
        request.DisposalReason = null;
        request.DisposalAmount = null;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_FixedAssetId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.FixedAssetId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FixedAssetId).WithErrorMessage("'Fixed Asset Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_DisposalDate_Is_Default()
    {
        var request = CreateValidRequest();
        request.DisposalDate = default;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DisposalDate);
    }

    [Fact]
    public void Should_Have_Error_When_DisposalDate_Is_In_Distant_Future()
    {
        // Validator allows AddDays(1). So AddDays(2) should be an error.
        var request = CreateValidRequest();
        request.DisposalDate = DateTime.UtcNow.AddDays(2);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DisposalDate)
            .WithErrorMessage("Disposal date cannot be too far in the future.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_DisposalDate_Is_Tomorrow()
    {
        var request = CreateValidRequest();
        request.DisposalDate = DateTime.UtcNow.AddDays(1).Date; // Allowed by validator
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.DisposalDate);
    }


    [Fact]
    public void Should_Have_Error_When_DisposalReason_Exceeds_MaxLength_If_Provided()
    {
        var request = CreateValidRequest();
        request.DisposalReason = new string('A', 501);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DisposalReason);
    }

    [Fact]
    public void Should_Have_Error_When_DisposalAmount_Is_Negative_If_Provided()
    {
        var request = CreateValidRequest();
        request.DisposalAmount = -10m;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DisposalAmount);
    }

    [Fact]
    public void Should_Not_Have_Error_When_DisposalAmount_Is_Zero_If_Provided()
    {
        var request = CreateValidRequest();
        request.DisposalAmount = 0m;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.DisposalAmount);
    }
}
