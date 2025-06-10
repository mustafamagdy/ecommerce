using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.AssetCategories;
using FSH.WebApi.Application.Accounting.DepreciationMethods; // For DepreciationMethodDto for context
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Application.Common.Persistence;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Models;
using Ardalis.Specification;

namespace Application.Tests.Accounting.AssetCategories;

public class AssetCategoryHandlerTests
{
    private readonly Mock<IRepository<AssetCategory>> _mockAssetCategoryRepo;
    private readonly Mock<IReadRepository<AssetCategory>> _mockAssetCategoryReadRepo;
    private readonly Mock<IReadRepository<DepreciationMethod>> _mockDepreciationMethodReadRepo;
    private readonly Mock<IReadRepository<FixedAsset>> _mockFixedAssetReadRepo; // For dependency check in Delete

    private readonly Mock<IStringLocalizer<CreateAssetCategoryHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateAssetCategoryHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdateAssetCategoryHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdateAssetCategoryHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<GetAssetCategoryHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<DeleteAssetCategoryHandler>> _mockDeleteLocalizer;
    private readonly Mock<ILogger<DeleteAssetCategoryHandler>> _mockDeleteLogger;
    // SearchAssetCategoriesHandler takes IReadRepository<DepreciationMethod> but not IStringLocalizer or ILogger

    public AssetCategoryHandlerTests()
    {
        _mockAssetCategoryRepo = new Mock<IRepository<AssetCategory>>();
        _mockAssetCategoryReadRepo = new Mock<IReadRepository<AssetCategory>>();
        _mockDepreciationMethodReadRepo = new Mock<IReadRepository<DepreciationMethod>>();
        _mockFixedAssetReadRepo = new Mock<IReadRepository<FixedAsset>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateAssetCategoryHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateAssetCategoryHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdateAssetCategoryHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateAssetCategoryHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetAssetCategoryHandler>>();
        _mockDeleteLocalizer = new Mock<IStringLocalizer<DeleteAssetCategoryHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeleteAssetCategoryHandler>>();

        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockUpdateLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        SetupDefaultLocalizationMock(_mockDeleteLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class =>
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] args) => new LocalizedString(name, name));

    private AssetCategory CreateSampleAssetCategory(Guid id, string name, Guid? depMethodId = null, int? life = 5, bool isActive = true) =>
        new AssetCategory(name, "Desc for " + name, depMethodId, life, isActive) { Id = id };

    private DepreciationMethod CreateSampleDepreciationMethod(Guid id, string name = "SL") =>
        new DepreciationMethod(name, "Straight Line") { Id = id };

    // === CreateAssetCategoryHandler Tests ===
    [Fact]
    public async Task CreateAssetCategoryHandler_Should_CreateCategory_WhenValid()
    {
        var depMethodId = Guid.NewGuid();
        var request = new CreateAssetCategoryRequest { Name = "New Category", DefaultDepreciationMethodId = depMethodId, DefaultUsefulLifeYears = 10, IsActive = true };
        _mockAssetCategoryRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<AssetCategoryByNameSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync((AssetCategory)null!);
        _mockDepreciationMethodReadRepo.Setup(r => r.GetByIdAsync(depMethodId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleDepreciationMethod(depMethodId));
        _mockAssetCategoryRepo.Setup(r => r.AddAsync(It.IsAny<AssetCategory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AssetCategory ac, CancellationToken ct) => { ac.Id = Guid.NewGuid(); return ac; });

        var handler = new CreateAssetCategoryHandler(_mockAssetCategoryRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object, _mockDepreciationMethodReadRepo.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeEmpty();
        _mockAssetCategoryRepo.Verify(r => r.AddAsync(It.Is<AssetCategory>(ac => ac.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAssetCategoryHandler_Should_ThrowConflict_WhenNameExists()
    {
        var request = new CreateAssetCategoryRequest { Name = "Existing Category" };
        _mockAssetCategoryRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<AssetCategoryByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSampleAssetCategory(Guid.NewGuid(), request.Name));
        var handler = new CreateAssetCategoryHandler(_mockAssetCategoryRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object, _mockDepreciationMethodReadRepo.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAssetCategoryHandler_Should_ThrowNotFound_WhenDepreciationMethodNotFound()
    {
        var request = new CreateAssetCategoryRequest { Name = "Category", DefaultDepreciationMethodId = Guid.NewGuid() };
        _mockAssetCategoryRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<AssetCategoryByNameSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync((AssetCategory)null!);
        _mockDepreciationMethodReadRepo.Setup(r => r.GetByIdAsync(request.DefaultDepreciationMethodId.Value, It.IsAny<CancellationToken>())).ReturnsAsync((DepreciationMethod)null!);
        var handler = new CreateAssetCategoryHandler(_mockAssetCategoryRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object, _mockDepreciationMethodReadRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === UpdateAssetCategoryHandler Tests ===
    [Fact]
    public async Task UpdateAssetCategoryHandler_Should_UpdateCategory_WhenValid()
    {
        var categoryId = Guid.NewGuid();
        var depMethodId = Guid.NewGuid();
        var request = new UpdateAssetCategoryRequest { Id = categoryId, Name = "Updated Cat", DefaultDepreciationMethodId = depMethodId };
        var existingCategory = CreateSampleAssetCategory(categoryId, "Old Cat Name");
        _mockAssetCategoryRepo.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCategory);
        _mockAssetCategoryRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<AssetCategoryByNameSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync((AssetCategory)null!);
        _mockDepreciationMethodReadRepo.Setup(r => r.GetByIdAsync(depMethodId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleDepreciationMethod(depMethodId));
        var handler = new UpdateAssetCategoryHandler(_mockAssetCategoryRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object, _mockDepreciationMethodReadRepo.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(categoryId);
        _mockAssetCategoryRepo.Verify(r => r.UpdateAsync(It.Is<AssetCategory>(ac => ac.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
        existingCategory.Name.Should().Be(request.Name);
    }


    // === GetAssetCategoryHandler Tests ===
    [Fact]
    public async Task GetAssetCategoryHandler_Should_ReturnDtoWithMethodName_WhenFound()
    {
        var categoryId = Guid.NewGuid();
        var depMethodId = Guid.NewGuid();
        var request = new GetAssetCategoryRequest(categoryId);
        var categoryEntity = CreateSampleAssetCategory(categoryId, "TestGetCat", depMethodId);
        var depMethodEntity = CreateSampleDepreciationMethod(depMethodId, "Fetched SL");

        _mockAssetCategoryReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<AssetCategoryByIdWithDetailsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryEntity); // Assuming spec returns entity
        _mockDepreciationMethodReadRepo.Setup(r => r.GetByIdAsync(depMethodId, It.IsAny<CancellationToken>())).ReturnsAsync(depMethodEntity);

        var handler = new GetAssetCategoryHandler(_mockAssetCategoryReadRepo.Object, _mockGetLocalizer.Object, _mockDepreciationMethodReadRepo.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(categoryId);
        result.Name.Should().Be(categoryEntity.Name);
        result.DefaultDepreciationMethodName.Should().Be(depMethodEntity.Name);
    }

    // === SearchAssetCategoriesHandler Tests ===
    [Fact]
    public async Task SearchAssetCategoriesHandler_Should_ReturnPaginatedDtosWithMethodNames()
    {
        var request = new SearchAssetCategoriesRequest { PageNumber = 1, PageSize = 10 };
        var depMethodId1 = Guid.NewGuid();
        var depMethod1 = CreateSampleDepreciationMethod(depMethodId1, "SL-Search");
        var categoriesList = new List<AssetCategory>
        {
            CreateSampleAssetCategory(Guid.NewGuid(), "Search Cat 1", depMethodId1),
            CreateSampleAssetCategory(Guid.NewGuid(), "Search Cat 2", null) // No method
        };
        _mockAssetCategoryReadRepo.Setup(r => r.ListAsync(It.IsAny<AssetCategoriesBySearchFilterSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(categoriesList);
        _mockAssetCategoryReadRepo.Setup(r => r.CountAsync(It.IsAny<AssetCategoriesBySearchFilterSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(categoriesList.Count);
        _mockDepreciationMethodReadRepo.Setup(r => r.ListAsync(It.Is<DepreciationMethodsByIdsSpec>(s => s.Ids.Contains(depMethodId1)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DepreciationMethod> { depMethod1 });

        var handler = new SearchAssetCategoriesHandler(_mockAssetCategoryReadRepo.Object, _mockDepreciationMethodReadRepo.Object);
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(categoriesList.Count);
        result.Data.First(dto => dto.Id == categoriesList[0].Id).DefaultDepreciationMethodName.Should().Be(depMethod1.Name);
        result.Data.First(dto => dto.Id == categoriesList[1].Id).DefaultDepreciationMethodName.Should().BeNull();
    }

    // === DeleteAssetCategoryHandler Tests ===
    [Fact]
    public async Task DeleteAssetCategoryHandler_Should_Delete_WhenNoDependencies()
    {
        var categoryId = Guid.NewGuid();
        var request = new DeleteAssetCategoryRequest(categoryId);
        var categoryEntity = CreateSampleAssetCategory(categoryId, "ToDeleteCat");
        _mockAssetCategoryRepo.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(categoryEntity);
        _mockFixedAssetReadRepo.Setup(r => r.AnyAsync(It.IsAny<FixedAssetByAssetCategorySpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new DeleteAssetCategoryHandler(_mockAssetCategoryRepo.Object, _mockFixedAssetReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(categoryId);
        _mockAssetCategoryRepo.Verify(r => r.DeleteAsync(categoryEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAssetCategoryHandler_Should_ThrowConflict_WhenUsedByFixedAsset()
    {
        var categoryId = Guid.NewGuid();
        var request = new DeleteAssetCategoryRequest(categoryId);
        var categoryEntity = CreateSampleAssetCategory(categoryId, "CatInUse");
        _mockAssetCategoryRepo.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(categoryEntity);
        _mockFixedAssetReadRepo.Setup(r => r.AnyAsync(It.IsAny<FixedAssetByAssetCategorySpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(true); // Dependency found
        var handler = new DeleteAssetCategoryHandler(_mockAssetCategoryRepo.Object, _mockFixedAssetReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }
}
