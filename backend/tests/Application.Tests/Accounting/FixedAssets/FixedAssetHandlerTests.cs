using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.FixedAssets; // Requests, Handlers, DTOs, Specs
using FSH.WebApi.Domain.Accounting; // FixedAsset, AssetCategory, DepreciationMethod entities & enums
using FSH.WebApi.Application.Common.Persistence;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Models; // PaginationResponse
using Ardalis.Specification;

namespace Application.Tests.Accounting.FixedAssets;

public class FixedAssetHandlerTests
{
    private readonly Mock<IRepository<FixedAsset>> _mockAssetRepo;
    private readonly Mock<IReadRepository<FixedAsset>> _mockAssetReadRepo;
    private readonly Mock<IReadRepository<AssetCategory>> _mockCategoryReadRepo;
    private readonly Mock<IReadRepository<DepreciationMethod>> _mockMethodReadRepo;

    // Mocks for Localizer and Logger for each handler
    private readonly Mock<IStringLocalizer<CreateFixedAssetHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateFixedAssetHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdateFixedAssetHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdateFixedAssetHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<DisposeFixedAssetHandler>> _mockDisposeLocalizer;
    private readonly Mock<ILogger<DisposeFixedAssetHandler>> _mockDisposeLogger;
    private readonly Mock<IStringLocalizer<CalculateDepreciationForPeriodHandler>> _mockCalcLocalizer;
    private readonly Mock<ILogger<CalculateDepreciationForPeriodHandler>> _mockCalcLogger;
    private readonly Mock<IStringLocalizer<GetFixedAssetHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<SearchFixedAssetsHandler>> _mockSearchLocalizer;
    private readonly Mock<IStringLocalizer<GetAssetDepreciationHistoryHandler>> _mockGetHistoryLocalizer;


    public FixedAssetHandlerTests()
    {
        _mockAssetRepo = new Mock<IRepository<FixedAsset>>();
        _mockAssetReadRepo = new Mock<IReadRepository<FixedAsset>>();
        _mockCategoryReadRepo = new Mock<IReadRepository<AssetCategory>>();
        _mockMethodReadRepo = new Mock<IReadRepository<DepreciationMethod>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateFixedAssetHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateFixedAssetHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdateFixedAssetHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateFixedAssetHandler>>();
        _mockDisposeLocalizer = new Mock<IStringLocalizer<DisposeFixedAssetHandler>>();
        _mockDisposeLogger = new Mock<ILogger<DisposeFixedAssetHandler>>();
        _mockCalcLocalizer = new Mock<IStringLocalizer<CalculateDepreciationForPeriodHandler>>();
        _mockCalcLogger = new Mock<ILogger<CalculateDepreciationForPeriodHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetFixedAssetHandler>>();
        _mockSearchLocalizer = new Mock<IStringLocalizer<SearchFixedAssetsHandler>>(); // Not used by handler
        _mockGetHistoryLocalizer = new Mock<IStringLocalizer<GetAssetDepreciationHistoryHandler>>();


        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockUpdateLocalizer);
        SetupDefaultLocalizationMock(_mockDisposeLocalizer);
        SetupDefaultLocalizationMock(_mockCalcLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        SetupDefaultLocalizationMock(_mockGetHistoryLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class =>
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] args) => new LocalizedString(name, name));

    // Helper Methods
    private AssetCategory CreateSampleAssetCategory(Guid id, string name = "Test Category") =>
        new AssetCategory(name, "Desc", Guid.NewGuid(), 5, true) { Id = id };
    private DepreciationMethod CreateSampleDepreciationMethod(Guid id, string name = "Straight-Line") =>
        new DepreciationMethod(name, "Desc") { Id = id };
    private FixedAsset CreateSampleFixedAsset(Guid id, string assetNumber = "FA001", FixedAssetStatus status = FixedAssetStatus.Active, Guid? categoryId = null, Guid? methodId = null)
    {
        var asset = new FixedAsset(
            assetNumber, "Sample Asset " + assetNumber, categoryId ?? Guid.NewGuid(), DateTime.UtcNow.AddYears(-2),
            10000m, 1000m, 5, methodId ?? Guid.NewGuid(), "Description", "Location", status);
        asset.Id = id;
        return asset;
    }

    // === CreateFixedAssetHandler Tests ===
    [Fact]
    public async Task CreateFixedAssetHandler_Should_CreateAsset_WhenValid()
    {
        var categoryId = Guid.NewGuid();
        var methodId = Guid.NewGuid();
        var request = new CreateFixedAssetRequest
        {
            AssetNumber = "FA-NEW-001", AssetName = "New Asset", AssetCategoryId = categoryId, PurchaseDate = DateTime.UtcNow.AddDays(-10),
            PurchaseCost = 5000, SalvageValue = 500, UsefulLifeYears = 5, DepreciationMethodId = methodId
        };

        _mockCategoryReadRepo.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleAssetCategory(categoryId));
        _mockMethodReadRepo.Setup(r => r.GetByIdAsync(methodId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleDepreciationMethod(methodId));
        _mockAssetRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<FixedAssetByAssetNumberSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync((FixedAsset)null!);
        _mockAssetRepo.Setup(r => r.AddAsync(It.IsAny<FixedAsset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FixedAsset fa, CancellationToken ct) => { fa.Id = Guid.NewGuid(); return fa; });

        var handler = new CreateFixedAssetHandler(_mockAssetRepo.Object, _mockCategoryReadRepo.Object, _mockMethodReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeEmpty();
        _mockAssetRepo.Verify(r => r.AddAsync(It.Is<FixedAsset>(fa => fa.AssetNumber == request.AssetNumber), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateFixedAssetHandler_Should_ThrowConflict_WhenAssetNumberExists()
    {
        var request = new CreateFixedAssetRequest { AssetNumber = "FA-EXISTING", AssetCategoryId = Guid.NewGuid(), DepreciationMethodId = Guid.NewGuid() };
        _mockCategoryReadRepo.Setup(r => r.GetByIdAsync(request.AssetCategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleAssetCategory(request.AssetCategoryId));
        _mockMethodReadRepo.Setup(r => r.GetByIdAsync(request.DepreciationMethodId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleDepreciationMethod(request.DepreciationMethodId));
        _mockAssetRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<FixedAssetByAssetNumberSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSampleFixedAsset(Guid.NewGuid(), request.AssetNumber));
        var handler = new CreateFixedAssetHandler(_mockAssetRepo.Object, _mockCategoryReadRepo.Object, _mockMethodReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === UpdateFixedAssetHandler Tests ===
    [Fact]
    public async Task UpdateFixedAssetHandler_Should_UpdateAsset_WhenActiveAndValid()
    {
        var assetId = Guid.NewGuid();
        var request = new UpdateFixedAssetRequest { Id = assetId, AssetName = "Updated FA Name", Location = "New Location" };
        var existingAsset = CreateSampleFixedAsset(assetId, status: FixedAssetStatus.Active);
        _mockAssetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>())).ReturnsAsync(existingAsset);
        // Assuming category and method IDs are not changed, so no mocks for those repos needed here for successful path.

        var handler = new UpdateFixedAssetHandler(_mockAssetRepo.Object, _mockCategoryReadRepo.Object, _mockMethodReadRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(assetId);
        _mockAssetRepo.Verify(r => r.UpdateAsync(It.Is<FixedAsset>(fa => fa.Id == assetId && fa.AssetName == request.AssetName), It.IsAny<CancellationToken>()), Times.Once);
        existingAsset.AssetName.Should().Be(request.AssetName);
        existingAsset.Location.Should().Be(request.Location);
    }

    [Fact]
    public async Task UpdateFixedAssetHandler_Should_ThrowConflict_WhenAssetIsDisposed()
    {
        var assetId = Guid.NewGuid();
        var request = new UpdateFixedAssetRequest { Id = assetId, AssetName = "Try Update Disposed" };
        var existingAsset = CreateSampleFixedAsset(assetId, status: FixedAssetStatus.Disposed);
        _mockAssetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>())).ReturnsAsync(existingAsset);
        var handler = new UpdateFixedAssetHandler(_mockAssetRepo.Object, _mockCategoryReadRepo.Object, _mockMethodReadRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === DisposeFixedAssetHandler Tests ===
    [Fact]
    public async Task DisposeFixedAssetHandler_Should_DisposeAsset_WhenActive()
    {
        var assetId = Guid.NewGuid();
        var request = new DisposeFixedAssetRequest { FixedAssetId = assetId, DisposalDate = DateTime.UtcNow, DisposalReason = "Sold" };
        var existingAsset = CreateSampleFixedAsset(assetId, status: FixedAssetStatus.Active);
        _mockAssetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>())).ReturnsAsync(existingAsset);
        var handler = new DisposeFixedAssetHandler(_mockAssetRepo.Object, _mockDisposeLocalizer.Object, _mockDisposeLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(assetId);
        existingAsset.Status.Should().Be(FixedAssetStatus.Disposed);
        existingAsset.DisposalDate.Should().Be(request.DisposalDate);
        existingAsset.DisposalReason.Should().Be(request.DisposalReason);
        _mockAssetRepo.Verify(r => r.UpdateAsync(existingAsset, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DisposeFixedAssetHandler_Should_ThrowConflict_WhenAlreadyDisposed()
    {
        var assetId = Guid.NewGuid();
        var request = new DisposeFixedAssetRequest { FixedAssetId = assetId, DisposalDate = DateTime.UtcNow };
        var existingAsset = CreateSampleFixedAsset(assetId, status: FixedAssetStatus.Disposed); // Already disposed
        _mockAssetRepo.Setup(r => r.GetByIdAsync(assetId, It.IsAny<CancellationToken>())).ReturnsAsync(existingAsset);
        var handler = new DisposeFixedAssetHandler(_mockAssetRepo.Object, _mockDisposeLocalizer.Object, _mockDisposeLogger.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === CalculateDepreciationForPeriodHandler Tests ===
    [Fact]
    public async Task CalculateDepreciation_SingleAsset_Should_CallDomainAndSave()
    {
        var assetId = Guid.NewGuid();
        var request = new CalculateDepreciationForPeriodRequest { FixedAssetId = assetId, PeriodEndDate = DateTime.UtcNow.Date };
        var asset = CreateSampleFixedAsset(assetId, "FA-CALC", FixedAssetStatus.Active); // Ensure it's active for calc

        // Setup spec for single asset fetch within handler
        _mockAssetRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<FixedAsset>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(asset);
        // The domain method CalculateDepreciationForPeriod will be tested separately.
        // Here, we just ensure it's called and UpdateAsync is triggered if an entry is made.

        var handler = new CalculateDepreciationForPeriodHandler(_mockAssetRepo.Object, _mockCalcLocalizer.Object, _mockCalcLogger.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert based on whether a new entry would be generated (depends on asset's purchase date vs. period end date)
        // If an entry was generated (asset.CalculateDepreciationForPeriod returns non-null)
        if (asset.PurchaseDate <= request.PeriodEndDate && asset.BookValue > asset.SalvageValue)
        {
            _mockAssetRepo.Verify(r => r.UpdateAsync(asset, It.IsAny<CancellationToken>()), Times.Once);
            result.DepreciationEntriesCreated.Should().Be(1);
        }
        else
        {
            _mockAssetRepo.Verify(r => r.UpdateAsync(It.IsAny<FixedAsset>(), It.IsAny<CancellationToken>()), Times.Never);
            result.DepreciationEntriesCreated.Should().Be(0);
        }
        result.AssetsProcessed.Should().Be(1);
    }


    // === GetFixedAssetHandler Tests ===
    [Fact]
    public async Task GetFixedAssetHandler_Should_ReturnDtoWithNames_WhenFound()
    {
        var assetId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var methodId = Guid.NewGuid();
        var request = new GetFixedAssetRequest(assetId);
        var assetEntity = CreateSampleFixedAsset(assetId, categoryId: categoryId, methodId: methodId);
        var categoryEntity = CreateSampleAssetCategory(categoryId, "Machinery");
        var methodEntity = CreateSampleDepreciationMethod(methodId, "SL Test");

        _mockAssetReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<FixedAssetByIdWithDetailsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assetEntity);
        _mockCategoryReadRepo.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(categoryEntity);
        _mockMethodReadRepo.Setup(r => r.GetByIdAsync(methodId, It.IsAny<CancellationToken>())).ReturnsAsync(methodEntity);

        var handler = new GetFixedAssetHandler(_mockAssetReadRepo.Object, _mockCategoryReadRepo.Object, _mockMethodReadRepo.Object, _mockGetLocalizer.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(assetId);
        result.AssetCategoryName.Should().Be(categoryEntity.Name);
        result.DepreciationMethodName.Should().Be(methodEntity.Name);
    }


    // === SearchFixedAssetsHandler Tests ===
    [Fact]
    public async Task SearchFixedAssetsHandler_Should_ReturnPaginatedDtosWithNames()
    {
        var request = new SearchFixedAssetsRequest { PageNumber = 1, PageSize = 10 };
        var categoryId = Guid.NewGuid();
        var methodId = Guid.NewGuid();
        var assetList = new List<FixedAsset> { CreateSampleFixedAsset(Guid.NewGuid(), categoryId: categoryId, methodId: methodId) };
        var category = CreateSampleAssetCategory(categoryId);
        var method = CreateSampleDepreciationMethod(methodId);

        _mockAssetReadRepo.Setup(r => r.ListAsync(It.IsAny<FixedAssetsBySearchFilterSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(assetList);
        _mockAssetReadRepo.Setup(r => r.CountAsync(It.IsAny<FixedAssetsBySearchFilterSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(assetList.Count);
        _mockCategoryReadRepo.Setup(r => r.ListAsync(It.Is<AssetCategoriesByIdsSpec>(s => s.Ids.Contains(categoryId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AssetCategory> { category });
        _mockMethodReadRepo.Setup(r => r.ListAsync(It.Is<DepreciationMethodsByIdsSpec>(s => s.Ids.Contains(methodId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DepreciationMethod> { method });

        var handler = new SearchFixedAssetsHandler(_mockAssetReadRepo.Object, _mockCategoryReadRepo.Object, _mockMethodReadRepo.Object, _mockSearchLocalizer.Object); // Search handler has no IStringLocalizer
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(assetList.Count);
        result.Data.First().AssetCategoryName.Should().Be(category.Name);
        result.Data.First().DepreciationMethodName.Should().Be(method.Name);
    }

    [Fact]
    public async Task CalculateDepreciation_Batch_Should_OnlyProcessEligibleAssets()
    {
        // Arrange
        var periodEndDate = DateTime.UtcNow.Date;
        var request = new CalculateDepreciationForPeriodRequest { FixedAssetId = null, PeriodEndDate = periodEndDate };

        var eligibleAssetId = Guid.NewGuid();
        var eligibleAsset = CreateSampleFixedAsset(eligibleAssetId, "FA-ELIGIBLE", FixedAssetStatus.Active, purchaseDate: periodEndDate.AddMonths(-6));

        var fullyDepreciatedAsset = CreateSampleFixedAsset(Guid.NewGuid(), "FA-FULLDEPR", FixedAssetStatus.Active, purchaseDate: periodEndDate.AddYears(-2), usefulLifeYears: 1, purchaseCost: 1200, salvageValue: 0);
        // Manually make it fully depreciated for the test by adding entries
        for(int i = 1; i <= 12; i++) { fullyDepreciatedAsset.CalculateDepreciationForPeriod(periodEndDate.AddYears(-2).AddMonths(i)); }
        fullyDepreciatedAsset.BookValue.Should().Be(fullyDepreciatedAsset.SalvageValue);


        var notYetPurchasedAsset = CreateSampleFixedAsset(Guid.NewGuid(), "FA-FUTURE", FixedAssetStatus.Active, purchaseDate: periodEndDate.AddMonths(1));
        var disposedAsset = CreateSampleFixedAsset(Guid.NewGuid(), "FA-DISPOSED", FixedAssetStatus.Disposed);

        var allAssetsInDbMock = new List<FixedAsset> { eligibleAsset, fullyDepreciatedAsset, notYetPurchasedAsset, disposedAsset };

        // The EligibleAssetsForDepreciationSpec should ideally filter this list down to only eligibleAsset.
        // Let's mock ListAsync to return what the spec *should* return.
        _mockAssetRepo.Setup(r => r.ListAsync(It.IsAny<EligibleAssetsForDepreciationSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FixedAsset> { eligibleAsset }); // Spec filters to only eligible ones

        var handler = new CalculateDepreciationForPeriodHandler(_mockAssetRepo.Object, _mockCalcLocalizer.Object, _mockCalcLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.AssetsProcessed.Should().Be(1); // Only one asset was processed from the list returned by spec
        // Verify CalculateDepreciationForPeriod was called on the eligible asset
        // This is harder to verify directly on the object if it's not a mock itself.
        // We trust the domain method test. Here we verify that UpdateAsync was called, implying an entry was made.
        if (eligibleAsset.BookValue > eligibleAsset.SalvageValue) // if it wasn't already fully depreciated by chance
        {
            _mockAssetRepo.Verify(r => r.UpdateAsync(It.Is<FixedAsset>(fa => fa.Id == eligibleAssetId), It.IsAny<CancellationToken>()), Times.Once);
            result.DepreciationEntriesCreated.Should().Be(1);
        }
        else
        {
            _mockAssetRepo.Verify(r => r.UpdateAsync(It.Is<FixedAsset>(fa => fa.Id == eligibleAssetId), It.IsAny<CancellationToken>()), Times.Never);
            result.DepreciationEntriesCreated.Should().Be(0);
        }
    }
}
