using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.FixedAssets;
using FSH.WebApi.Domain.Accounting; // For FixedAssetStatus if used in request/validator
using System;

namespace Application.Tests.Accounting.FixedAssets;

public class CreateFixedAssetRequestValidatorTests
{
    private readonly CreateFixedAssetRequestValidator _validator = new();

    private CreateFixedAssetRequest CreateValidRequest()
    {
        return new CreateFixedAssetRequest
        {
            AssetNumber = "FA001-VALID",
            AssetName = "Valid Asset Name",
            AssetCategoryId = Guid.NewGuid(),
            PurchaseDate = DateTime.UtcNow.Date.AddDays(-10), // In the past
            PurchaseCost = 1000m,
            SalvageValue = 100m,
            UsefulLifeYears = 5,
            DepreciationMethodId = Guid.NewGuid(),
            Location = "Office A",
            Description = "Valid description."
        };
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Have_Error_When_AssetNumber_Is_NullOrEmpty(string assetNumber)
    {
        var request = CreateValidRequest();
        request.AssetNumber = assetNumber;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AssetNumber);
    }

    [Fact]
    public void Should_Have_Error_When_AssetNumber_Exceeds_MaxLength()
    {
        var request = CreateValidRequest();
        request.AssetNumber = new string('A', 51);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AssetNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Have_Error_When_AssetName_Is_NullOrEmpty(string assetName)
    {
        var request = CreateValidRequest();
        request.AssetName = assetName;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AssetName);
    }

    [Fact]
    public void Should_Have_Error_When_AssetName_Exceeds_MaxLength()
    {
        var request = CreateValidRequest();
        request.AssetName = new string('A', 101);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AssetName);
    }


    [Fact]
    public void Should_Have_Error_When_AssetCategoryId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.AssetCategoryId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AssetCategoryId).WithErrorMessage("'Asset Category Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_PurchaseDate_Is_Default()
    {
        var request = CreateValidRequest();
        request.PurchaseDate = default;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDate);
    }

    [Fact]
    public void Should_Have_Error_When_PurchaseDate_Is_In_Future()
    {
        var request = CreateValidRequest();
        request.PurchaseDate = DateTime.UtcNow.AddDays(1);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDate).WithErrorMessage("Purchase date cannot be in the future.");
    }


    [Fact]
    public void Should_Have_Error_When_PurchaseCost_Is_Negative()
    {
        var request = CreateValidRequest();
        request.PurchaseCost = -1;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseCost);
    }

    [Fact]
    public void Should_Have_Error_When_SalvageValue_Is_Negative()
    {
        var request = CreateValidRequest();
        request.SalvageValue = -1;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SalvageValue);
    }

    [Fact]
    public void Should_Have_Error_When_SalvageValue_Is_GreaterThan_PurchaseCost()
    {
        var request = CreateValidRequest();
        request.PurchaseCost = 100m;
        request.SalvageValue = 101m;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SalvageValue)
            .WithErrorMessage("Salvage Value must be between zero and Purchase Cost.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_SalvageValue_Is_EqualTo_PurchaseCost()
    {
        var request = CreateValidRequest();
        request.PurchaseCost = 100m;
        request.SalvageValue = 100m;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.SalvageValue);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_UsefulLifeYears_Is_Not_Positive(int years)
    {
        var request = CreateValidRequest();
        request.UsefulLifeYears = years;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.UsefulLifeYears)
            .WithErrorMessage("Useful Life in years must be positive.");
    }

    [Fact]
    public void Should_Have_Error_When_DepreciationMethodId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.DepreciationMethodId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DepreciationMethodId).WithErrorMessage("'Depreciation Method Id' must not be empty.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Optional_Description_And_Location_Are_Null()
    {
        var request = CreateValidRequest();
        request.Description = null;
        request.Location = null;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
