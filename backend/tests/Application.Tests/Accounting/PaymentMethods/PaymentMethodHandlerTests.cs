using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.PaymentMethods; // Requests, Handlers, DTOs, Specs
using FSH.WebApi.Domain.Accounting; // PaymentMethod, VendorPayment entities
using FSH.WebApi.Application.Common.Persistence;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Models; // PaginationResponse
using Ardalis.Specification;

namespace Application.Tests.Accounting.PaymentMethods;

public class PaymentMethodHandlerTests
{
    private readonly Mock<IRepository<PaymentMethod>> _mockPaymentMethodRepo;
    private readonly Mock<IReadRepository<PaymentMethod>> _mockPaymentMethodReadRepo;
    private readonly Mock<IReadRepository<VendorPayment>> _mockVendorPaymentReadRepo; // For dependency check in Delete

    private readonly Mock<IStringLocalizer<CreatePaymentMethodHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreatePaymentMethodHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdatePaymentMethodHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdatePaymentMethodHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<GetPaymentMethodHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<DeletePaymentMethodHandler>> _mockDeleteLocalizer;
    private readonly Mock<ILogger<DeletePaymentMethodHandler>> _mockDeleteLogger;
    // SearchPaymentMethodsHandler does not have IStringLocalizer or ILogger injected.

    public PaymentMethodHandlerTests()
    {
        _mockPaymentMethodRepo = new Mock<IRepository<PaymentMethod>>();
        _mockPaymentMethodReadRepo = new Mock<IReadRepository<PaymentMethod>>();
        _mockVendorPaymentReadRepo = new Mock<IReadRepository<VendorPayment>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreatePaymentMethodHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreatePaymentMethodHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdatePaymentMethodHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdatePaymentMethodHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetPaymentMethodHandler>>();
        _mockDeleteLocalizer = new Mock<IStringLocalizer<DeletePaymentMethodHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeletePaymentMethodHandler>>();

        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockUpdateLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        SetupDefaultLocalizationMock(_mockDeleteLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class
    {
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] arguments) => new LocalizedString(name, name));
        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments), false, typeof(T).FullName));
    }

    private PaymentMethod CreateSamplePaymentMethod(Guid id, string name, bool isActive = true) =>
        new PaymentMethod(name, $"{name} Description", isActive) { Id = id };

    // === CreatePaymentMethodHandler Tests ===
    [Fact]
    public async Task CreatePaymentMethodHandler_Should_CreateMethod_WhenNameIsUnique()
    {
        // Arrange
        var request = new CreatePaymentMethodRequest { Name = "ACH", IsActive = true };
        _mockPaymentMethodRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<PaymentMethodByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentMethod)null!);
        _mockPaymentMethodRepo.Setup(r => r.AddAsync(It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentMethod pm, CancellationToken ct) => { pm.Id = Guid.NewGuid(); return pm; });

        var handler = new CreatePaymentMethodHandler(_mockPaymentMethodRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockPaymentMethodRepo.Verify(r => r.AddAsync(It.Is<PaymentMethod>(pm => pm.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentMethodHandler_Should_ThrowConflictException_WhenNameIsNotUnique()
    {
        // Arrange
        var request = new CreatePaymentMethodRequest { Name = "Credit Card" };
        var existingMethod = CreateSamplePaymentMethod(Guid.NewGuid(), request.Name);
        _mockPaymentMethodRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<PaymentMethodByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMethod);
        var handler = new CreatePaymentMethodHandler(_mockPaymentMethodRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === UpdatePaymentMethodHandler Tests ===
    [Fact]
    public async Task UpdatePaymentMethodHandler_Should_UpdateMethod_WhenFoundAndNameIsUnique()
    {
        // Arrange
        var methodId = Guid.NewGuid();
        var request = new UpdatePaymentMethodRequest { Id = methodId, Name = "Wire Transfer Updated", IsActive = false };
        var existingMethod = CreateSamplePaymentMethod(methodId, "Wire Transfer");
        _mockPaymentMethodRepo.Setup(r => r.GetByIdAsync(methodId, It.IsAny<CancellationToken>())).ReturnsAsync(existingMethod);
        _mockPaymentMethodRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<PaymentMethodByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentMethod)null!); // New name is unique

        var handler = new UpdatePaymentMethodHandler(_mockPaymentMethodRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(methodId);
        _mockPaymentMethodRepo.Verify(r => r.UpdateAsync(It.Is<PaymentMethod>(pm => pm.Id == methodId && pm.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
        existingMethod.Name.Should().Be(request.Name);
        existingMethod.IsActive.Should().Be(request.IsActive);
    }

    // === GetPaymentMethodHandler Tests ===
    [Fact]
    public async Task GetPaymentMethodHandler_Should_ReturnDto_WhenFound()
    {
        // Arrange
        var methodId = Guid.NewGuid();
        var request = new GetPaymentMethodRequest(methodId);
        var methodEntity = CreateSamplePaymentMethod(methodId, "Test Get Method");
        _mockPaymentMethodReadRepo.Setup(r => r.GetByIdAsync(methodId, It.IsAny<CancellationToken>())).ReturnsAsync(methodEntity);
        var handler = new GetPaymentMethodHandler(_mockPaymentMethodReadRepo.Object, _mockGetLocalizer.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(methodId);
        result.Name.Should().Be(methodEntity.Name);
    }

    // === SearchPaymentMethodsHandler Tests ===
    [Fact]
    public async Task SearchPaymentMethodsHandler_Should_ReturnPaginatedDtos()
    {
        // Arrange
        var request = new SearchPaymentMethodsRequest { PageNumber = 1, PageSize = 10 };
        var methodsList = new List<PaymentMethod> { CreateSamplePaymentMethod(Guid.NewGuid(), "Search Method 1") };
        _mockPaymentMethodReadRepo.Setup(r => r.ListAsync(It.IsAny<PaymentMethodsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(methodsList);
        _mockPaymentMethodReadRepo.Setup(r => r.CountAsync(It.IsAny<PaymentMethodsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(methodsList.Count);
        var handler = new SearchPaymentMethodsHandler(_mockPaymentMethodReadRepo.Object); // This handler does not take IStringLocalizer

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(methodsList.Count);
    }

    // === DeletePaymentMethodHandler Tests ===
    [Fact]
    public async Task DeletePaymentMethodHandler_Should_DeleteMethod_WhenFoundAndNoDependencies()
    {
        // Arrange
        var methodId = Guid.NewGuid();
        var request = new DeletePaymentMethodRequest(methodId);
        var methodEntity = CreateSamplePaymentMethod(methodId, "Test Delete Method");
        _mockPaymentMethodRepo.Setup(r => r.GetByIdAsync(methodId, It.IsAny<CancellationToken>())).ReturnsAsync(methodEntity);
        _mockVendorPaymentReadRepo.Setup(r => r.AnyAsync(It.IsAny<VendorPaymentsByPaymentMethodSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // No dependencies

        var handler = new DeletePaymentMethodHandler(_mockPaymentMethodRepo.Object, _mockVendorPaymentReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(methodId);
        _mockPaymentMethodRepo.Verify(r => r.DeleteAsync(methodEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePaymentMethodHandler_Should_ThrowConflictException_WhenDependenciesExist()
    {
        // Arrange
        var methodId = Guid.NewGuid();
        var request = new DeletePaymentMethodRequest(methodId);
        var methodEntity = CreateSamplePaymentMethod(methodId, "Test Delete Conflict Method");
        _mockPaymentMethodRepo.Setup(r => r.GetByIdAsync(methodId, It.IsAny<CancellationToken>())).ReturnsAsync(methodEntity);
        _mockVendorPaymentReadRepo.Setup(r => r.AnyAsync(It.IsAny<VendorPaymentsByPaymentMethodSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Dependencies exist

        var handler = new DeletePaymentMethodHandler(_mockPaymentMethodRepo.Object, _mockVendorPaymentReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }
}
