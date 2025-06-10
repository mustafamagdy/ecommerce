using Xunit;
using FluentAssertions;
using FSH.WebApi.Application.Accounting.AssetCategories;
using FSH.WebApi.Application.Accounting.DepreciationMethods;
using FSH.WebApi.Application.Accounting.FixedAssets;
using FSH.WebApi.Domain.Accounting; // For enums like FixedAssetStatus
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSH.WebApi.Application.IntegrationTests.Infra; // Assuming TestBase or TestFixture is here

namespace FSH.WebApi.Application.IntegrationTests.TestCases.Accounting;

public class FixedAssetManagementFlowTests : TestBase
{
    private async Task<Guid> EnsureDepreciationMethodAsync(string name = "FAM Flow SL Test")
    {
        var searchRequest = new SearchDepreciationMethodsRequest { NameKeyword = name, PageSize = 1, PageNumber = 1 };
        var searchResult = await Sender.Send(searchRequest);
        if (searchResult.Data.Any(dm => dm.Name == name))
        {
            return searchResult.Data.First(dm => dm.Name == name).Id;
        }
        var createRequest = new CreateDepreciationMethodRequest { Name = name, Description = $"{name} Description" };
        return await Sender.Send(createRequest);
    }

    private async Task<Guid> EnsureAssetCategoryAsync(string name, Guid depreciationMethodId, int usefulLife = 5)
    {
        var searchRequest = new SearchAssetCategoriesRequest { NameKeyword = name, PageSize = 1, PageNumber = 1 };
        var searchResult = await Sender.Send(searchRequest);
        if (searchResult.Data.Any(ac => ac.Name == name))
        {
            return searchResult.Data.First(ac => ac.Name == name).Id;
        }
        var createRequest = new CreateAssetCategoryRequest
        {
            Name = name,
            Description = $"{name} Description",
            DefaultDepreciationMethodId = depreciationMethodId,
            DefaultUsefulLifeYears = usefulLife,
            IsActive = true
        };
        return await Sender.Send(createRequest);
    }

    [Fact]
    public async Task Should_Successfully_Complete_Full_Fixed_Asset_Lifecycle()
    {
        // Arrange
        var depMethodName = $"FAM SL Cycle-{Guid.NewGuid().ToString().Substring(0, 4)}";
        var depreciationMethodId = await EnsureDepreciationMethodAsync(depMethodName);

        var categoryName = $"FAM Cat Cycle-{Guid.NewGuid().ToString().Substring(0, 4)}";
        var assetCategoryId = await EnsureAssetCategoryAsync(categoryName, depreciationMethodId, 1); // 1 year useful life for faster test

        var purchaseDate = DateTime.UtcNow.Date.AddMonths(-12); // Purchased 12 months ago
        var purchaseCost = 1200m;
        var salvageValue = 0m; // Simplify to zero salvage for easy monthly calculation
        var usefulLifeYears = 1; // 12 months

        var createAssetRequest = new CreateFixedAssetRequest
        {
            AssetNumber = $"FA-CYCLE-{Guid.NewGuid().ToString().Substring(0, 6)}",
            AssetName = "Lifecycle Test Asset",
            AssetCategoryId = assetCategoryId,
            PurchaseDate = purchaseDate,
            PurchaseCost = purchaseCost,
            SalvageValue = salvageValue,
            UsefulLifeYears = usefulLifeYears,
            DepreciationMethodId = depreciationMethodId,
            Location = "Test Location"
        };

        // Act & Assert (sequentially):

        // 1. Create Fixed Asset
        var assetId = await Sender.Send(createAssetRequest);
        assetId.Should().NotBeEmpty();

        var getAssetRequest = new GetFixedAssetRequest(assetId);
        var assetDto = await Sender.Send(getAssetRequest);
        assetDto.Should().NotBeNull();
        assetDto.PurchaseCost.Should().Be(purchaseCost);
        assetDto.Status.Should().Be(FixedAssetStatus.Active.ToString());
        assetDto.BookValue.Should().Be(purchaseCost); // Initially book value is purchase cost
        assetDto.AccumulatedDepreciation.Should().Be(0);

        // 2. Calculate Depreciation for Several Periods (e.g., 12 months for 1 year life)
        decimal expectedMonthlyDepreciation = (purchaseCost - salvageValue) / usefulLifeYears / 12;
        DateTime currentPeriodEndDate = purchaseDate;

        for (int i = 1; i <= 12; i++)
        {
            currentPeriodEndDate = purchaseDate.AddMonths(i);
            // Ensure period end date is not in the future relative to "now" if domain logic prevents it for actual run date
            // For testing, we might need to control "now" or ensure periodEndDate is in the past/present.
            // The domain entity's CalculateDepreciationForPeriod doesn't explicitly check against UtcNow.
            // It checks periodEndDate < PurchaseDate and if period already depreciated.

            var calcRequest = new CalculateDepreciationForPeriodRequest
            {
                FixedAssetId = assetId,
                PeriodEndDate = currentPeriodEndDate
            };
            var calcResult = await Sender.Send(calcRequest);

            calcResult.Should().NotBeNull();
            if (i <= usefulLifeYears * 12 && (purchaseCost - salvageValue > 0)) // If still within useful life and depreciable
            {
                calcResult.DepreciationEntriesCreated.Should().Be(1);
                calcResult.TotalDepreciationAmount.Should().BeApproximately(expectedMonthlyDepreciation, 0.01m);
            }
            else // Should be fully depreciated or not applicable
            {
                calcResult.DepreciationEntriesCreated.Should().Be(0);
                calcResult.TotalDepreciationAmount.Should().Be(0);
            }

            assetDto = await Sender.Send(getAssetRequest); // Refresh asset DTO
            assetDto.AccumulatedDepreciation.Should().BeApproximately(expectedMonthlyDepreciation * Math.Min(i, usefulLifeYears * 12), 0.01m * i);
            assetDto.BookValue.Should().BeApproximately(purchaseCost - assetDto.AccumulatedDepreciation, 0.01m);
        }

        // After 12 periods for a 1-year asset, it should be fully depreciated (or close to salvage value)
        assetDto.AccumulatedDepreciation.Should().BeApproximately(purchaseCost - salvageValue, 0.01m);
        assetDto.BookValue.Should().BeApproximately(salvageValue, 0.01m);

        // 3. Dispose Fixed Asset
        var disposalDate = currentPeriodEndDate.AddDays(1);
        var disposeAssetRequest = new DisposeFixedAssetRequest
        {
            FixedAssetId = assetId,
            DisposalDate = disposalDate,
            DisposalReason = "End of life, sold for scrap",
            DisposalAmount = salvageValue > 0 ? salvageValue - 10 : 0 // Simulate selling for slightly less than salvage or 0
        };
        await Sender.Send(disposeAssetRequest);

        var disposedAssetDto = await Sender.Send(getAssetRequest);
        disposedAssetDto.Should().NotBeNull();
        disposedAssetDto.Status.Should().Be(FixedAssetStatus.Disposed.ToString());
        // Check if DisposalDate, DisposalReason, DisposalAmount are populated in DTO and match request.
        // FixedAssetDto does not currently have these. If it did:
        // disposedAssetDto.DisposalDate.Should().Be(disposalDate);
        // disposedAssetDto.DisposalReason.Should().Be(disposeAssetRequest.DisposalReason);
        // disposedAssetDto.DisposalAmount.Should().Be(disposeAssetRequest.DisposalAmount);


        // 4. (Optional) Attempt Depreciation After Disposal
        var postDisposalPeriodEndDate = disposalDate.AddMonths(1);
        var calcAfterDisposalRequest = new CalculateDepreciationForPeriodRequest
        {
            FixedAssetId = assetId,
            PeriodEndDate = postDisposalPeriodEndDate
        };
        var calcAfterDisposalResult = await Sender.Send(calcAfterDisposalRequest);

        calcAfterDisposalResult.Should().NotBeNull();
        calcAfterDisposalResult.DepreciationEntriesCreated.Should().Be(0);
        calcAfterDisposalResult.AssetsProcessed.Should().Be(1); // Processed, but no depreciation generated
    }

    [Fact]
    public async Task Should_Fail_To_Update_Disposed_Asset()
    {
        // Arrange
        var depMethodId = await EnsureDepreciationMethodAsync($"FAM UpdateDisp SL-{Guid.NewGuid().ToString().Substring(0,4)}");
        var catId = await EnsureAssetCategoryAsync($"FAM UpdateDisp Cat-{Guid.NewGuid().ToString().Substring(0,4)}", depMethodId);
        var assetId = await Sender.Send(new CreateFixedAssetRequest { AssetNumber = $"FA-DISPUPD-{Guid.NewGuid().ToString().Substring(0,6)}", AssetName="To Dispose then Update", AssetCategoryId=catId, PurchaseDate=DateTime.UtcNow.AddYears(-1), PurchaseCost=100, SalvageValue=0, UsefulLifeYears=1, DepreciationMethodId=depMethodId });
        await Sender.Send(new DisposeFixedAssetRequest { FixedAssetId = assetId, DisposalDate = DateTime.UtcNow, DisposalReason = "Test" });

        var updateRequest = new UpdateFixedAssetRequest
        {
            Id = assetId,
            AssetName = "Trying to update a disposed asset"
        };

        // Act
        Func<Task> act = async () => await Sender.Send(updateRequest);

        // Assert
        await act.Should().ThrowAsync<FSH.WebApi.Application.Common.Exceptions.ConflictException>(); // Assuming UpdateFixedAssetHandler throws ConflictException
    }
}
