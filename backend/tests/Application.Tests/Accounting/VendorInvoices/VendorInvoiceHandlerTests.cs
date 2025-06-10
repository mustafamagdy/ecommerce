using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.VendorInvoices; // Requests, Handlers, DTOs, Specs
using FSH.WebApi.Application.Accounting.Suppliers; // For Supplier entity/dto for context
using FSH.WebApi.Domain.Accounting; // VendorInvoice, VendorInvoiceItem, Supplier entities
using FSH.WebApi.Domain.Catalog; // Product entity
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
using Ardalis.Specification; // For ISpecification

namespace Application.Tests.Accounting.VendorInvoices;

public class VendorInvoiceHandlerTests
{
    private readonly Mock<IRepository<VendorInvoice>> _mockInvoiceRepo;
    private readonly Mock<IReadRepository<VendorInvoice>> _mockInvoiceReadRepo;
    private readonly Mock<IReadRepository<Supplier>> _mockSupplierReadRepo; // Changed from IRepository to IReadRepository
    private readonly Mock<IReadRepository<Product>> _mockProductReadRepo;   // Changed from IRepository to IReadRepository

    // Using more specific mocks for localizer and logger per handler
    private readonly Mock<IStringLocalizer<CreateVendorInvoiceHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateVendorInvoiceHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdateVendorInvoiceHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdateVendorInvoiceHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<GetVendorInvoiceHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<SearchVendorInvoicesHandler>> _mockSearchLocalizer;
    private readonly Mock<IStringLocalizer<DeleteVendorInvoiceHandler>> _mockDeleteLocalizer;
    private readonly Mock<ILogger<DeleteVendorInvoiceHandler>> _mockDeleteLogger;

    public VendorInvoiceHandlerTests()
    {
        _mockInvoiceRepo = new Mock<IRepository<VendorInvoice>>();
        _mockInvoiceReadRepo = new Mock<IReadRepository<VendorInvoice>>();
        _mockSupplierReadRepo = new Mock<IReadRepository<Supplier>>();
        _mockProductReadRepo = new Mock<IReadRepository<Product>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateVendorInvoiceHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateVendorInvoiceHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdateVendorInvoiceHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateVendorInvoiceHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetVendorInvoiceHandler>>();
        _mockSearchLocalizer = new Mock<IStringLocalizer<SearchVendorInvoicesHandler>>();
        _mockDeleteLocalizer = new Mock<IStringLocalizer<DeleteVendorInvoiceHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeleteVendorInvoiceHandler>>();

        // Default localization setup
        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockUpdateLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        SetupDefaultLocalizationMock(_mockSearchLocalizer);
        SetupDefaultLocalizationMock(_mockDeleteLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class
    {
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] arguments) => new LocalizedString(name, name));
        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments), false, typeof(T).FullName));

    }


    private Supplier CreateSampleSupplier(Guid id, string name = "Test Supplier") =>
        new Supplier(name, "sup@test.com", "1 Sup St", "SUP123", Guid.NewGuid(), "Sup Bank") { Id = id };

    private Product CreateSampleProduct(Guid id, string name = "Test Product") =>
        new Product(name, "Prod Desc", 100m, "imgpath", Guid.NewGuid(), 10) { Id = id }; // Assuming Product constructor

    private VendorInvoice CreateSampleVendorInvoice(Guid id, Guid supplierId, string invoiceNumber = "VI-001", decimal totalAmount = 100m) =>
        new VendorInvoice(supplierId, invoiceNumber, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), totalAmount, "USD", VendorInvoiceStatus.Draft) { Id = id };


    // === CreateVendorInvoiceHandler Tests ===
    [Fact]
    public async Task CreateVendorInvoiceHandler_Should_CreateInvoice_WhenRequestIsValid()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new CreateVendorInvoiceRequest
        {
            SupplierId = supplierId,
            InvoiceNumber = "INV-NEW-001",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 120m, // (1 * 100) + 20 tax
            Currency = "USD",
            InvoiceItems = new List<CreateVendorInvoiceItemRequest>
            {
                new CreateVendorInvoiceItemRequest { Description = "Item 1", ProductId = productId, Quantity = 1, UnitPrice = 100m, TaxAmount = 20m, TotalAmount = 100m }
            }
        };

        var supplier = CreateSampleSupplier(supplierId);
        var product = CreateSampleProduct(productId);

        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(supplier);
        _mockProductReadRepo.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _mockInvoiceRepo.Setup(r => r.AddAsync(It.IsAny<VendorInvoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VendorInvoice vi, CancellationToken ct) => { vi.Id = Guid.NewGuid(); return vi; });

        var handler = new CreateVendorInvoiceHandler(_mockInvoiceRepo.Object, _mockSupplierReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object, _mockProductReadRepo.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockInvoiceRepo.Verify(r => r.AddAsync(It.Is<VendorInvoice>(vi =>
            vi.SupplierId == supplierId &&
            vi.InvoiceNumber == request.InvoiceNumber &&
            vi.InvoiceItems.Count == 1 &&
            vi.InvoiceItems.First().Description == "Item 1" &&
            Math.Abs(vi.TotalAmount - request.TotalAmount) < 0.001m // Check calculated total if handler recalculates
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVendorInvoiceHandler_Should_ThrowNotFoundException_WhenSupplierNotFound()
    {
        // Arrange
        var request = new CreateVendorInvoiceRequest { SupplierId = Guid.NewGuid(), InvoiceNumber = "INV-X", InvoiceItems = new List<CreateVendorInvoiceItemRequest>() };
        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(request.SupplierId, It.IsAny<CancellationToken>())).ReturnsAsync((Supplier)null!);
        var handler = new CreateVendorInvoiceHandler(_mockInvoiceRepo.Object, _mockSupplierReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object, _mockProductReadRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateVendorInvoiceHandler_Should_ThrowNotFoundException_WhenProductNotFound()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var productId = Guid.NewGuid(); // Non-existent product
        var request = new CreateVendorInvoiceRequest
        {
            SupplierId = supplierId,
            InvoiceNumber = "INV-PFAIL",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 120m, Currency = "USD",
            InvoiceItems = new List<CreateVendorInvoiceItemRequest>
            { new CreateVendorInvoiceItemRequest { Description = "Item X", ProductId = productId, Quantity = 1, UnitPrice = 100m, TaxAmount = 20m, TotalAmount = 100m } }
        };
        var supplier = CreateSampleSupplier(supplierId);
        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(supplier);
        _mockProductReadRepo.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product)null!); // Product not found
        var handler = new CreateVendorInvoiceHandler(_mockInvoiceRepo.Object, _mockSupplierReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object, _mockProductReadRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === UpdateVendorInvoiceHandler Tests ===
    [Fact]
    public async Task UpdateVendorInvoiceHandler_Should_UpdateInvoice_WhenFoundAndValid()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var request = new UpdateVendorInvoiceRequest { Id = invoiceId, Notes = "Updated Notes" };
        var existingInvoice = CreateSampleVendorInvoice(invoiceId, supplierId);
        var supplier = CreateSampleSupplier(supplierId);


        _mockInvoiceRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<VendorInvoiceByIdWithItemsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvoice);
         // Assuming UpdateVendorInvoiceHandler also injects IRepository<Supplier> for supplier validation if SupplierId can be changed (it cannot based on current request)
        // _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(supplier);

        var handler = new UpdateVendorInvoiceHandler(_mockInvoiceRepo.Object, _mockSupplierReadRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object, _mockProductReadRepo.Object);


        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(invoiceId);
        _mockInvoiceRepo.Verify(r => r.UpdateAsync(It.Is<VendorInvoice>(vi => vi.Id == invoiceId && vi.Notes == request.Notes), It.IsAny<CancellationToken>()), Times.Once);
        existingInvoice.Notes.Should().Be(request.Notes);
    }

    [Fact]
    public async Task UpdateVendorInvoiceHandler_Should_ThrowNotFoundException_WhenInvoiceNotFound()
    {
        // Arrange
        var request = new UpdateVendorInvoiceRequest { Id = Guid.NewGuid() };
        _mockInvoiceRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<VendorInvoiceByIdWithItemsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VendorInvoice)null!);
        var handler = new UpdateVendorInvoiceHandler(_mockInvoiceRepo.Object, _mockSupplierReadRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object, _mockProductReadRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === GetVendorInvoiceHandler Tests ===
    [Fact]
    public async Task GetVendorInvoiceHandler_Should_ReturnDto_WhenFound()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var request = new GetVendorInvoiceRequest(invoiceId);
        var invoiceEntity = CreateSampleVendorInvoice(invoiceId, supplierId, "VI-GET-001");
        var supplierEntity = CreateSampleSupplier(supplierId, "SupplierForGet");

        // The spec VendorInvoiceByIdWithItemsSpec is Specification<VendorInvoice, VendorInvoiceDto>
        // So FirstOrDefaultAsync(spec) should return VendorInvoiceDto
        // However, the handler implementation fetches entity, then supplier, then product, then adapts.
        // Let's mock based on the handler's actual implementation.
        _mockInvoiceReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<VendorInvoiceByIdWithItemsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity); // Handler expects entity from this spec call
        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplierEntity);

        var handler = new GetVendorInvoiceHandler(_mockInvoiceReadRepo.Object, _mockSupplierReadRepo.Object, _mockGetLocalizer.Object, _mockProductReadRepo.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(invoiceId);
        result.InvoiceNumber.Should().Be("VI-GET-001");
        result.SupplierName.Should().Be(supplierEntity.Name);
    }


    // === SearchVendorInvoicesHandler Tests ===
    [Fact]
    public async Task SearchVendorInvoicesHandler_Should_ReturnPaginatedDtos()
    {
        // Arrange
        var request = new SearchVendorInvoicesRequest { PageNumber = 1, PageSize = 5 };
        var supplierId1 = Guid.NewGuid();
        var supplier1 = CreateSampleSupplier(supplierId1, "Supplier Alpha");
        var invoiceList = new List<VendorInvoice>
        {
            CreateSampleVendorInvoice(Guid.NewGuid(), supplierId1, "VI-S-001"),
            CreateSampleVendorInvoice(Guid.NewGuid(), supplierId1, "VI-S-002")
        };

        _mockInvoiceReadRepo.Setup(r => r.ListAsync(It.IsAny<VendorInvoicesBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceList); // Handler expects entities
        _mockInvoiceReadRepo.Setup(r => r.CountAsync(It.IsAny<VendorInvoicesBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceList.Count);
        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(supplierId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier1);


        var handler = new SearchVendorInvoicesHandler(_mockInvoiceReadRepo.Object, _mockSupplierReadRepo.Object, _mockSearchLocalizer.Object, _mockProductReadRepo.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(invoiceList.Count);
        result.TotalCount.Should().Be(invoiceList.Count);
        result.Data.First().SupplierName.Should().Be(supplier1.Name);
    }


    // === DeleteVendorInvoiceHandler Tests ===
    [Fact]
    public async Task DeleteVendorInvoiceHandler_Should_DeleteInvoice_WhenFoundAndAllowed()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var request = new DeleteVendorInvoiceRequest(invoiceId);
        var invoiceEntity = CreateSampleVendorInvoice(invoiceId, Guid.NewGuid(), "VI-DEL-001"); // Status Draft by default
        invoiceEntity.UpdateStatus(VendorInvoiceStatus.Draft); // Ensure it's in a deletable state

        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoiceEntity);

        var handler = new DeleteVendorInvoiceHandler(_mockInvoiceRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(invoiceId);
        _mockInvoiceRepo.Verify(r => r.DeleteAsync(invoiceEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteVendorInvoiceHandler_Should_ThrowConflictException_WhenInvoiceIsPaid()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var request = new DeleteVendorInvoiceRequest(invoiceId);
        var invoiceEntity = CreateSampleVendorInvoice(invoiceId, Guid.NewGuid(), "VI-PAID-001");
        invoiceEntity.UpdateStatus(VendorInvoiceStatus.Paid); // Set to Paid

        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoiceEntity);
        var handler = new DeleteVendorInvoiceHandler(_mockInvoiceRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }
}
