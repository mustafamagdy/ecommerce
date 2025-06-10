using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.FixedAssets;
using FSH.WebApi.Domain.Accounting; // For FixedAssetStatus
using System;

namespace Application.Tests.Accounting.FixedAssets;

public class UpdateFixedAssetRequestValidatorTests
{
    private readonly UpdateFixedAssetRequestValidator _validator = new();

    private UpdateFixedAssetRequest CreateValidRequest(Guid? id = null)
    {
        return new UpdateFixedAssetRequest
        {
            Id = id ?? Guid.NewGuid(),
            AssetName = "Updated Asset Name",
            Description = "Updated description.",
            AssetCategoryId = Guid.NewGuid(),
            PurchaseDate = DateTime.UtcNow.Date.AddDays(-20),
            PurchaseCost = 1200m,
            SalvageValue = 120m,
            UsefulLifeYears = 6,
            DepreciationMethodId = Guid.NewGuid(),
            Location = "Building B",
            Status = FixedAssetStatus.Active
        };
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid_With_All_Fields()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Only_Id_And_One_Editable_Field_Is_Provided()
    {
        var request = new UpdateFixedAssetRequest { Id = Guid.NewGuid(), AssetName = "Only Name Changed" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();

        request = new UpdateFixedAssetRequest { Id = Guid.NewGuid(), Status = FixedAssetStatus.Inactive };
        result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var request = CreateValidRequest(id: Guid.Empty);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage("'Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_AssetName_Is_Empty_If_Provided()
    {
        var request = CreateValidRequest();
        request.AssetName = string.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AssetName);
    }

    [Fact]
    public void Should_Have_Error_When_PurchaseDate_Is_In_Future_If_Provided()
    {
        var request = CreateValidRequest();
        request.PurchaseDate = DateTime.UtcNow.AddDays(2);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseDate).WithErrorMessage("Purchase date cannot be in the future.");
    }

    [Fact]
    public void Should_Have_Error_When_PurchaseCost_Is_Negative_If_Provided()
    {
        var request = CreateValidRequest();
        request.PurchaseCost = -100m;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PurchaseCost);
    }

    // SalvageValue <= PurchaseCost is handled in domain/handler for Update as it requires context of existing values.
    // Validator only checks SalvageValue >= 0.
    [Fact]
    public void Should_Have_Error_When_SalvageValue_Is_Negative_If_Provided()
    {
        var request = CreateValidRequest();
        request.SalvageValue = -50m;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SalvageValue);
    }

    [Fact]
    public void Should_Have_Error_When_UsefulLifeYears_Is_Zero_If_Provided()
    {
        var request = CreateValidRequest();
        request.UsefulLifeYears = 0;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.UsefulLifeYears).WithErrorMessage("Useful Life in years must be positive.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Optional_Fields_Are_Null()
    {
        var request = new UpdateFixedAssetRequest { Id = Guid.NewGuid() }; // All other fields null
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_AssetCategoryId_Is_EmptyGuid_If_Provided()
    {
        var request = CreateValidRequest();
        request.AssetCategoryId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AssetCategoryId).WithErrorMessage("'Asset Category Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_DepreciationMethodId_Is_EmptyGuid_If_Provided()
    {
        var request = CreateValidRequest();
        request.DepreciationMethodId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DepreciationMethodId).WithErrorMessage("'Depreciation Method Id' must not be empty.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Status_Is_ValidEnumValue()
    {
        var request = CreateValidRequest();
        request.Status = FixedAssetStatus.Disposed; // A valid enum value
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }
}
