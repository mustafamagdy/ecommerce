using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.DepreciationMethods;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Application.Common.Persistence;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Models;
using Ardalis.Specification;

namespace Application.Tests.Accounting.DepreciationMethods;

public class DepreciationMethodHandlerTests
{
    private readonly Mock<IRepository<DepreciationMethod>> _mockDepreciationMethodRepo;
    private readonly Mock<IReadRepository<DepreciationMethod>> _mockDepreciationMethodReadRepo;
    private readonly Mock<IReadRepository<AssetCategory>> _mockAssetCategoryReadRepo;
    private readonly Mock<IReadRepository<FixedAsset>> _mockFixedAssetReadRepo;

    private readonly Mock<IStringLocalizer<CreateDepreciationMethodHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateDepreciationMethodHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdateDepreciationMethodHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdateDepreciationMethodHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<GetDepreciationMethodHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<DeleteDepreciationMethodHandler>> _mockDeleteLocalizer;
    private readonly Mock<ILogger<DeleteDepreciationMethodHandler>> _mockDeleteLogger;

    public DepreciationMethodHandlerTests()
    {
        _mockDepreciationMethodRepo = new Mock<IRepository<DepreciationMethod>>();
        _mockDepreciationMethodReadRepo = new Mock<IReadRepository<DepreciationMethod>>();
        _mockAssetCategoryReadRepo = new Mock<IReadRepository<AssetCategory>>();
        _mockFixedAssetReadRepo = new Mock<IReadRepository<FixedAsset>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateDepreciationMethodHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateDepreciationMethodHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdateDepreciationMethodHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateDepreciationMethodHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetDepreciationMethodHandler>>();
        _mockDeleteLocalizer = new Mock<IStringLocalizer<DeleteDepreciationMethodHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeleteDepreciationMethodHandler>>();

        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockUpdateLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        SetupDefaultLocalizationMock(_mockDeleteLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class =>
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] args) => new LocalizedString(name, name));

    private DepreciationMethod CreateSampleDepreciationMethod(Guid id, string name) =>
        new DepreciationMethod(name, "Description for " + name) { Id = id };

    // === CreateDepreciationMethodHandler Tests ===
    [Fact]
    public async Task CreateDepreciationMethodHandler_Should_CreateMethod_WhenNameIsUnique()
    {
        var request = new CreateDepreciationMethodRequest { Name = "New Method", Description = "Desc" };
        _mockDepreciationMethodRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<DepreciationMethodByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DepreciationMethod)null!);
        _mockDepreciationMethodRepo.Setup(r => r.AddAsync(It.IsAny<DepreciationMethod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DepreciationMethod dm, CancellationToken ct) => { dm.Id = Guid.NewGuid(); return dm; });
        var handler = new CreateDepreciationMethodHandler(_mockDepreciationMethodRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeEmpty();
        _mockDepreciationMethodRepo.Verify(r => r.AddAsync(It.Is<DepreciationMethod>(dm => dm.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateDepreciationMethodHandler_Should_ThrowConflict_WhenNameExists()
    {
        var request = new CreateDepreciationMethodRequest { Name = "Existing Method" };
        _mockDepreciationMethodRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<DepreciationMethodByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSampleDepreciationMethod(Guid.NewGuid(), request.Name));
        var handler = new CreateDepreciationMethodHandler(_mockDepreciationMethodRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === UpdateDepreciationMethodHandler Tests ===
    [Fact]
    public async Task UpdateDepreciationMethodHandler_Should_UpdateMethod_WhenFoundAndNameIsUnique()
    {
        var methodId = Guid.NewGuid();
        var request = new UpdateDepreciationMethodRequest { Id = methodId, Name = "Updated Name", Description = "Updated Desc" };
        var existingMethod = CreateSampleDepreciationMethod(methodId, "Old Name");
        _mockDepreciationMethodRepo.Setup(r => r.GetByIdAsync(methodId, It.IsAny<CancellationToken>())).ReturnsAsync(existingMethod);
        _mockDepreciationMethodRepo.Setup(r => r.FirstOrDefaultAsync(It.Is<DepreciationMethodByNameSpec>(s => s.Name == request.Name), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DepreciationMethod)null!); // New name is unique
        var handler = new UpdateDepreciationMethodHandler(_mockDepreciationMethodRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(methodId);
        _mockDepreciationMethodRepo.Verify(r => r.UpdateAsync(It.Is<DepreciationMethod>(dm => dm.Id == methodId && dm.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
        existingMethod.Name.Should().Be(request.Name);
    }

    // === GetDepreciationMethodHandler Tests ===
    [Fact]
    public async Task GetDepreciationMethodHandler_Should_ReturnDto_WhenFound()
    {
        var methodId = Guid.NewGuid();
        var request = new GetDepreciationMethodRequest(methodId);
        var methodEntity = CreateSampleDepreciationMethod(methodId, "TestGet");
        _mockDepreciationMethodReadRepo.Setup(r => r.GetByIdAsync(methodId, It.IsAny<CancellationToken>())).ReturnsAsync(methodEntity);
        var handler = new GetDepreciationMethodHandler(_mockDepreciationMethodReadRepo.Object, _mockGetLocalizer.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(methodId);
        result.Name.Should().Be(methodEntity.Name);
    }

    // === SearchDepreciationMethodsHandler Tests ===
    [Fact]
    public async Task SearchDepreciationMethodsHandler_Should_ReturnPaginatedDtos()
    {
        var request = new SearchDepreciationMethodsRequest { PageNumber = 1, PageSize = 10 };
        var methodsList = new List<DepreciationMethod> { CreateSampleDepreciationMethod(Guid.NewGuid(), "SearchDM1") };
        _mockDepreciationMethodReadRepo.Setup(r => r.ListAsync(It.IsAny<DepreciationMethodsBySearchFilterSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(methodsList);
        _mockDepreciationMethodReadRepo.Setup(r => r.CountAsync(It.IsAny<DepreciationMethodsBySearchFilterSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(methodsList.Count);
        var handler = new SearchDepreciationMethodsHandler(_mockDepreciationMethodReadRepo.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Data.Should().HaveCount(methodsList.Count);
    }

    // === DeleteDepreciationMethodHandler Tests ===
    [Fact]
    public async Task DeleteDepreciationMethodHandler_Should_Delete_WhenNoDependencies()
    {
        var methodId = Guid.NewGuid();
        var request = new DeleteDepreciationMethodRequest(methodId);
        var methodEntity = CreateSampleDepreciationMethod(methodId, "ToDeleteDM");
        _mockDepreciationMethodRepo.Setup(r => r.GetByIdAsync(methodId, It.IsAny<CancellationToken>())).ReturnsAsync(methodEntity);
        _mockAssetCategoryReadRepo.Setup(r => r.AnyAsync(It.IsAny<AssetCategoryByDepreciationMethodSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mockFixedAssetReadRepo.Setup(r => r.AnyAsync(It.IsAny<FixedAssetByDepreciationMethodSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var handler = new DeleteDepreciationMethodHandler(_mockDepreciationMethodRepo.Object, _mockAssetCategoryReadRepo.Object, _mockFixedAssetReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().Be(methodId);
        _mockDepreciationMethodRepo.Verify(r => r.DeleteAsync(methodEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDepreciationMethodHandler_Should_ThrowConflict_WhenUsedByAssetCategory()
    {
        var methodId = Guid.NewGuid();
        var request = new DeleteDepreciationMethodRequest(methodId);
        var methodEntity = CreateSampleDepreciationMethod(methodId, "DMInUse");
        _mockDepreciationMethodRepo.Setup(r => r.GetByIdAsync(methodId, It.IsAny<CancellationToken>())).ReturnsAsync(methodEntity);
        _mockAssetCategoryReadRepo.Setup(r => r.AnyAsync(It.IsAny<AssetCategoryByDepreciationMethodSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(true); // Dependency found
        var handler = new DeleteDepreciationMethodHandler(_mockDepreciationMethodRepo.Object, _mockAssetCategoryReadRepo.Object, _mockFixedAssetReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }
}
