using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.PaymentTerms; // Requests, Handlers, DTOs, Specs
using FSH.WebApi.Domain.Accounting; // PaymentTerm, Supplier entities
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

namespace Application.Tests.Accounting.PaymentTerms;

public class PaymentTermHandlerTests
{
    private readonly Mock<IRepository<PaymentTerm>> _mockPaymentTermRepo;
    private readonly Mock<IReadRepository<PaymentTerm>> _mockPaymentTermReadRepo;
    private readonly Mock<IReadRepository<Supplier>> _mockSupplierReadRepo; // For dependency check in Delete

    private readonly Mock<IStringLocalizer<CreatePaymentTermHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreatePaymentTermHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdatePaymentTermHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdatePaymentTermHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<GetPaymentTermHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<SearchPaymentTermsHandler>> _mockSearchLocalizer; // No specific localizer, SearchPaymentTermsHandler has no IStringLocalizer
    private readonly Mock<IStringLocalizer<DeletePaymentTermHandler>> _mockDeleteLocalizer;
    private readonly Mock<ILogger<DeletePaymentTermHandler>> _mockDeleteLogger;

    public PaymentTermHandlerTests()
    {
        _mockPaymentTermRepo = new Mock<IRepository<PaymentTerm>>();
        _mockPaymentTermReadRepo = new Mock<IReadRepository<PaymentTerm>>();
        _mockSupplierReadRepo = new Mock<IReadRepository<Supplier>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreatePaymentTermHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreatePaymentTermHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdatePaymentTermHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdatePaymentTermHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetPaymentTermHandler>>();
        _mockSearchLocalizer = new Mock<IStringLocalizer<SearchPaymentTermsHandler>>(); // Not used by handler
        _mockDeleteLocalizer = new Mock<IStringLocalizer<DeletePaymentTermHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeletePaymentTermHandler>>();

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

    private PaymentTerm CreateSamplePaymentTerm(Guid id, string name, int days = 30, bool isActive = true) =>
        new PaymentTerm(name, days, $"{name} Description", isActive) { Id = id };

    // === CreatePaymentTermHandler Tests ===
    [Fact]
    public async Task CreatePaymentTermHandler_Should_CreateTerm_WhenNameIsUnique()
    {
        // Arrange
        var request = new CreatePaymentTermRequest { Name = "Net 45", DaysUntilDue = 45, IsActive = true };
        _mockPaymentTermRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<PaymentTermByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentTerm)null!);
        _mockPaymentTermRepo.Setup(r => r.AddAsync(It.IsAny<PaymentTerm>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentTerm pt, CancellationToken ct) => { pt.Id = Guid.NewGuid(); return pt; });

        var handler = new CreatePaymentTermHandler(_mockPaymentTermRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockPaymentTermRepo.Verify(r => r.AddAsync(It.Is<PaymentTerm>(pt => pt.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentTermHandler_Should_ThrowConflictException_WhenNameIsNotUnique()
    {
        // Arrange
        var request = new CreatePaymentTermRequest { Name = "Net 30" };
        var existingTerm = CreateSamplePaymentTerm(Guid.NewGuid(), request.Name);
        _mockPaymentTermRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<PaymentTermByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTerm);
        var handler = new CreatePaymentTermHandler(_mockPaymentTermRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === UpdatePaymentTermHandler Tests ===
    [Fact]
    public async Task UpdatePaymentTermHandler_Should_UpdateTerm_WhenFoundAndNameIsUnique()
    {
        // Arrange
        var termId = Guid.NewGuid();
        var request = new UpdatePaymentTermRequest { Id = termId, Name = "Net 60 Updated", DaysUntilDue = 60 };
        var existingTerm = CreateSamplePaymentTerm(termId, "Net 60");
        _mockPaymentTermRepo.Setup(r => r.GetByIdAsync(termId, It.IsAny<CancellationToken>())).ReturnsAsync(existingTerm);
        _mockPaymentTermRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<PaymentTermByNameSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentTerm)null!); // New name is unique

        var handler = new UpdatePaymentTermHandler(_mockPaymentTermRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(termId);
        _mockPaymentTermRepo.Verify(r => r.UpdateAsync(It.Is<PaymentTerm>(pt => pt.Id == termId && pt.Name == request.Name), It.IsAny<CancellationToken>()), Times.Once);
        existingTerm.Name.Should().Be(request.Name);
        existingTerm.DaysUntilDue.Should().Be(request.DaysUntilDue);
    }

    // === GetPaymentTermHandler Tests ===
    [Fact]
    public async Task GetPaymentTermHandler_Should_ReturnDto_WhenFound()
    {
        // Arrange
        var termId = Guid.NewGuid();
        var request = new GetPaymentTermRequest(termId);
        var termEntity = CreateSamplePaymentTerm(termId, "Net Test Get");
        _mockPaymentTermReadRepo.Setup(r => r.GetByIdAsync(termId, It.IsAny<CancellationToken>())).ReturnsAsync(termEntity);
        var handler = new GetPaymentTermHandler(_mockPaymentTermReadRepo.Object, _mockGetLocalizer.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(termId);
        result.Name.Should().Be(termEntity.Name);
    }

    // === SearchPaymentTermsHandler Tests ===
    [Fact]
    public async Task SearchPaymentTermsHandler_Should_ReturnPaginatedDtos()
    {
        // Arrange
        var request = new SearchPaymentTermsRequest { PageNumber = 1, PageSize = 10 };
        var termsList = new List<PaymentTerm> { CreateSamplePaymentTerm(Guid.NewGuid(), "Search Term 1") };
        _mockPaymentTermReadRepo.Setup(r => r.ListAsync(It.IsAny<PaymentTermsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(termsList);
        _mockPaymentTermReadRepo.Setup(r => r.CountAsync(It.IsAny<PaymentTermsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(termsList.Count);
        var handler = new SearchPaymentTermsHandler(_mockPaymentTermReadRepo.Object); // This handler does not take IStringLocalizer

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(termsList.Count);
    }

    // === DeletePaymentTermHandler Tests ===
    [Fact]
    public async Task DeletePaymentTermHandler_Should_DeleteTerm_WhenFoundAndNoDependencies()
    {
        // Arrange
        var termId = Guid.NewGuid();
        var request = new DeletePaymentTermRequest(termId);
        var termEntity = CreateSamplePaymentTerm(termId, "Net Test Delete");
        _mockPaymentTermRepo.Setup(r => r.GetByIdAsync(termId, It.IsAny<CancellationToken>())).ReturnsAsync(termEntity);
        _mockSupplierReadRepo.Setup(r => r.AnyAsync(It.IsAny<SuppliersByPaymentTermSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // No dependencies

        var handler = new DeletePaymentTermHandler(_mockPaymentTermRepo.Object, _mockSupplierReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(termId);
        _mockPaymentTermRepo.Verify(r => r.DeleteAsync(termEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePaymentTermHandler_Should_ThrowConflictException_WhenDependenciesExist()
    {
        // Arrange
        var termId = Guid.NewGuid();
        var request = new DeletePaymentTermRequest(termId);
        var termEntity = CreateSamplePaymentTerm(termId, "Net Test Delete Conflict");
        _mockPaymentTermRepo.Setup(r => r.GetByIdAsync(termId, It.IsAny<CancellationToken>())).ReturnsAsync(termEntity);
        _mockSupplierReadRepo.Setup(r => r.AnyAsync(It.IsAny<SuppliersByPaymentTermSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Dependencies exist

        var handler = new DeletePaymentTermHandler(_mockPaymentTermRepo.Object, _mockSupplierReadRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }
}
