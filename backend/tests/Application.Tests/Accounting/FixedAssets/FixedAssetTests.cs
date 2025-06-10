using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For FixedAsset, AssetDepreciationEntry, FixedAssetStatus
using System;
using System.Linq;

namespace Application.Tests.Accounting.FixedAssets;

public class FixedAssetTests
{
    private Guid _testAssetCategoryId;
    private Guid _straightLineDepreciationMethodId;
    private DepreciationMethod _straightLineDepreciationMethod; // For passing to FixedAsset constructor if it took the entity
                                                              // Currently, FixedAsset constructor takes DepreciationMethodId

    public FixedAssetTests()
    {
        _testAssetCategoryId = Guid.NewGuid();
        _straightLineDepreciationMethodId = Guid.NewGuid(); // Assume this ID represents "Straight-Line"
        _straightLineDepreciationMethod = new DepreciationMethod("Straight-Line", "Test SL Method") { Id = _straightLineDepreciationMethodId };
    }

    private FixedAsset CreateTestFixedAsset(
        string assetNumber = "FA-001",
        string assetName = "Test Asset",
        DateTime? purchaseDate = null,
        decimal purchaseCost = 12000m,
        decimal salvageValue = 2000m,
        int usefulLifeYears = 5,
        Guid? depreciationMethodId = null, // Will default to _straightLineDepreciationMethodId
        FixedAssetStatus status = FixedAssetStatus.Active)
    {
        return new FixedAsset(
            assetNumber,
            assetName,
            _testAssetCategoryId,
            purchaseDate ?? DateTime.UtcNow.Date.AddYears(-1), // Default purchased 1 year ago
            purchaseCost,
            salvageValue,
            usefulLifeYears,
            depreciationMethodId ?? _straightLineDepreciationMethodId,
            "Test Description",
            "Test Location",
            status
        );
    }


    [Fact]
    public void Constructor_Should_InitializeFixedAssetCorrectly()
    {
        // Arrange
        var purchaseDate = new DateTime(2023, 1, 15);
        var assetNumber = "FA-INIT-001";

        // Act
        var asset = new FixedAsset(
            assetNumber, "Laptop Pro", _testAssetCategoryId, purchaseDate, 1500m, 100m, 3, _straightLineDepreciationMethodId
        );

        // Assert
        asset.Id.Should().NotBe(Guid.Empty);
        asset.AssetNumber.Should().Be(assetNumber);
        asset.AssetName.Should().Be("Laptop Pro");
        asset.AssetCategoryId.Should().Be(_testAssetCategoryId);
        asset.PurchaseDate.Should().Be(purchaseDate);
        asset.PurchaseCost.Should().Be(1500m);
        asset.SalvageValue.Should().Be(100m);
        asset.UsefulLifeYears.Should().Be(3);
        asset.DepreciationMethodId.Should().Be(_straightLineDepreciationMethodId);
        asset.Status.Should().Be(FixedAssetStatus.Active); // Default from constructor
        asset.DepreciationEntries.Should().BeEmpty();
        asset.AccumulatedDepreciation.Should().Be(0);
        asset.BookValue.Should().Be(1500m); // PurchaseCost - 0
    }

    [Fact]
    public void Constructor_PurchaseCostLessThanSalvageValue_Should_ThrowArgumentException()
    {
        Action act = () => CreateTestFixedAsset(purchaseCost: 1000m, salvageValue: 1200m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Purchase cost cannot be less than salvage value.");
    }

    [Fact]
    public void Constructor_NegativeSalvageValue_Should_ThrowArgumentOutOfRangeException()
    {
        Action act = () => CreateTestFixedAsset(salvageValue: -100m);
        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("salvageValue");
    }

    [Fact]
    public void Constructor_ZeroUsefulLife_Should_ThrowArgumentOutOfRangeException()
    {
        Action act = () => CreateTestFixedAsset(usefulLifeYears: 0);
        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("usefulLifeYears");
    }


    [Fact]
    public void Dispose_Should_UpdateStatusAndDisposalFields()
    {
        // Arrange
        var asset = CreateTestFixedAsset();
        var disposalDate = DateTime.UtcNow.Date;
        var reason = "Sold asset";
        var amount = 500m;

        // Act
        asset.Dispose(disposalDate, reason, amount);

        // Assert
        asset.Status.Should().Be(FixedAssetStatus.Disposed);
        asset.DisposalDate.Should().Be(disposalDate);
        asset.DisposalReason.Should().Be(reason);
        asset.DisposalAmount.Should().Be(amount);
    }

    // --- CalculateDepreciationForPeriod Tests (Straight-Line) ---
    // Assuming Monthly Depreciation: (Cost - Salvage) / (Years * 12)

    [Fact]
    public void CalculateDepreciation_NewAsset_FirstMonth_Should_CalculateCorrectDepreciation()
    {
        // Arrange
        // Cost 12000, Salvage 2000, Life 5 years (60 months)
        // Depreciable Base = 10000
        // Yearly Dep = 10000 / 5 = 2000
        // Monthly Dep = 2000 / 12 = 166.666...
        var purchaseDate = new DateTime(2023, 1, 1);
        var asset = CreateTestFixedAsset(purchaseDate: purchaseDate, purchaseCost: 12000, salvageValue: 2000, usefulLifeYears: 5);
        // Simulate that the DepreciationMethod object is loaded for the check `DepreciationMethod?.Name`
        // This is a bit of a hack for unit testing; in reality, EF might load this.
        // For now, the domain code has `DepreciationMethod?.Name != "Straight-Line"`
        // This means if DepreciationMethod is null, it passes. This needs careful handling or a direct ID check.
        // The domain code was simplified to `Console.WriteLine` for non-SL, so it will proceed.

        var periodEndDate = new DateTime(2023, 1, 31);

        // Act
        var entry = asset.CalculateDepreciationForPeriod(periodEndDate);

        // Assert
        entry.Should().NotBeNull();
        entry!.Amount.Should().BeApproximately(10000m / (5 * 12), 0.01m); // 166.67
        entry.DepreciationDate.Should().Be(periodEndDate);
        asset.DepreciationEntries.Should().ContainSingle().Which.Should().Be(entry);
        asset.AccumulatedDepreciation.Should().BeApproximately(166.67m, 0.01m);
        asset.BookValue.Should().BeApproximately(12000m - 166.67m, 0.01m);
    }

    [Fact]
    public void CalculateDepreciation_MidLife_Should_CalculateCorrectDepreciation()
    {
        var purchaseDate = new DateTime(2023, 1, 1);
        var asset = CreateTestFixedAsset(purchaseDate: purchaseDate, purchaseCost: 12000, salvageValue: 2000, usefulLifeYears: 5);
        // Simulate 5 previous depreciations
        for (int i = 1; i <= 5; i++)
        {
            asset.CalculateDepreciationForPeriod(new DateTime(2023, i, DateTime.DaysInMonth(2023,i)));
        }
        var expectedAccumulated = (10000m / 60) * 5;
        asset.AccumulatedDepreciation.Should().BeApproximately(expectedAccumulated, 0.01m);

        var periodEndDate = new DateTime(2023, 6, 30);

        // Act
        var entry = asset.CalculateDepreciationForPeriod(periodEndDate);

        // Assert
        entry.Should().NotBeNull();
        entry!.Amount.Should().BeApproximately(10000m / 60, 0.01m);
        asset.AccumulatedDepreciation.Should().BeApproximately(expectedAccumulated + (10000m / 60), 0.01m);
    }

    [Fact]
    public void CalculateDepreciation_FullyDepreciated_Should_ReturnNullAndNotAddEntry()
    {
        var asset = CreateTestFixedAsset(purchaseCost: 1200, salvageValue: 0, usefulLifeYears: 1); // Monthly = 100
        // Depreciate for 12 months
        for (int i = 1; i <= 12; i++)
        {
            asset.CalculateDepreciationForPeriod(asset.PurchaseDate.AddMonths(i).AddDays(-1)); // End of each month
        }
        asset.AccumulatedDepreciation.Should().Be(1200m); // Fully depreciated

        // Act
        var entry = asset.CalculateDepreciationForPeriod(asset.PurchaseDate.AddMonths(13).AddDays(-1));

        // Assert
        entry.Should().BeNull();
        asset.DepreciationEntries.Should().HaveCount(12); // No new entry
    }


    [Fact]
    public void CalculateDepreciation_FinalPeriod_Should_AdjustToSalvageValue()
    {
        // Cost 1200, Salvage 50, Life 1 year. Depreciable base = 1150. Monthly = 1150/12 = 95.8333
        var asset = CreateTestFixedAsset(purchaseCost: 1200, salvageValue: 50, usefulLifeYears: 1);
        // Depreciate for 11 months
        for (int i = 1; i <= 11; i++)
        {
            asset.CalculateDepreciationForPeriod(asset.PurchaseDate.AddMonths(i).AddDays(-1));
        }
        // Accumulated after 11 months: 11 * (1150/12) = 1054.166...
        // Remaining to depreciate: 1150 - 1054.166... = 95.833...
        var expectedLastDepreciation = asset.PurchaseCost - asset.SalvageValue - asset.AccumulatedDepreciation;
        expectedLastDepreciation.Should().BeApproximately(1150m/12, 0.01m);


        // Act: 12th month
        var entry = asset.CalculateDepreciationForPeriod(asset.PurchaseDate.AddMonths(12).AddDays(-1));

        // Assert
        entry.Should().NotBeNull();
        entry!.Amount.Should().BeApproximately(expectedLastDepreciation, 0.01m);
        asset.AccumulatedDepreciation.Should().Be(1150m); // Exactly depreciable base
        asset.BookValue.Should().Be(asset.SalvageValue);
    }


    [Fact]
    public void CalculateDepreciation_BeforePurchaseDate_Should_ReturnNull()
    {
        var asset = CreateTestFixedAsset(purchaseDate: new DateTime(2023, 5, 1));
        var entry = asset.CalculateDepreciationForPeriod(new DateTime(2023, 4, 30));
        entry.Should().BeNull();
    }

    [Fact]
    public void CalculateDepreciation_ForDisposedAsset_Should_ReturnNull()
    {
        var asset = CreateTestFixedAsset(status: FixedAssetStatus.Disposed);
        var entry = asset.CalculateDepreciationForPeriod(DateTime.UtcNow);
        entry.Should().BeNull();
    }

    [Fact]
    public void CalculateDepreciation_ZeroDepreciableBase_Should_ReturnNull()
    {
        var asset = CreateTestFixedAsset(purchaseCost: 1000, salvageValue: 1000); // Depreciable base is 0
        var entry = asset.CalculateDepreciationForPeriod(DateTime.UtcNow);
        entry.Should().BeNull();
    }

    [Fact]
    public void CalculateDepreciation_PeriodAlreadyDepreciated_Should_ReturnNull()
    {
        var asset = CreateTestFixedAsset();
        var periodEndDate = asset.PurchaseDate.AddMonths(1).AddDays(-1);
        asset.CalculateDepreciationForPeriod(periodEndDate); // First depreciation

        // Act: Try to depreciate for the same period again
        var entry = asset.CalculateDepreciationForPeriod(periodEndDate);

        // Assert
        entry.Should().BeNull();
        asset.DepreciationEntries.Should().HaveCount(1); // Should not add duplicate
    }


    [Fact]
    public void Update_Should_ModifyEditableProperties()
    {
        // Arrange
        var asset = CreateTestFixedAsset();
        var originalAssetNumber = asset.AssetNumber;

        var newAssetName = "Updated Asset Name";
        var newDesc = "Updated Description";
        var newCategoryId = Guid.NewGuid();
        // ... other properties

        // Act
        asset.Update(
            assetName: newAssetName,
            description: newDesc,
            assetCategoryId: newCategoryId,
            purchaseDate: null, // Not changing these for this test
            purchaseCost: null,
            salvageValue: null,
            usefulLifeYears: null,
            depreciationMethodId: null,
            location: "New Location",
            status: FixedAssetStatus.Inactive
        );

        // Assert
        asset.AssetName.Should().Be(newAssetName);
        asset.Description.Should().Be(newDesc);
        asset.AssetCategoryId.Should().Be(newCategoryId);
        asset.Location.Should().Be("New Location");
        asset.Status.Should().Be(FixedAssetStatus.Inactive);
        asset.AssetNumber.Should().Be(originalAssetNumber); // AssetNumber not updatable via Update
    }
}
