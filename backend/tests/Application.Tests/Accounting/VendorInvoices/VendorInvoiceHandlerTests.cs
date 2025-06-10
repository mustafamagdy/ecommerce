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

    [Fact]
    public async Task UpdateVendorInvoiceHandler_Should_Successfully_Update_Invoice_Items_When_Updating_Invoice()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var existingItemId1 = Guid.NewGuid();
        var existingItemId2 = Guid.NewGuid(); // This item will be removed
        var productId1 = Guid.NewGuid();
        var productIdNew = Guid.NewGuid();

        var existingInvoice = new VendorInvoice(supplierId, "INV-ITEM-TEST", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 0, "USD", VendorInvoiceStatus.Draft);
        existingInvoice.Id = invoiceId;

        var item1 = new VendorInvoiceItem(invoiceId, "Existing Item 1", 1, 100m, 10m, productId1); // Total 100 + 10 tax = 110
        item1.Id = existingItemId1;
        var item2 = new VendorInvoiceItem(invoiceId, "Existing Item 2 To Remove", 1, 50m, 5m); // Total 50 + 5 tax = 55
        item2.Id = existingItemId2;

        existingInvoice.AddInvoiceItem(item1);
        existingInvoice.AddInvoiceItem(item2);
        // Initial TotalAmount = 110 + 55 = 165

        var product1 = CreateSampleProduct(productId1, "Product 1");
        var productNew = CreateSampleProduct(productIdNew, "New Product");

        _mockInvoiceRepo.Setup(r => r.FirstOrDefaultAsync(It.Is<VendorInvoiceByIdWithItemsSpec>(spec => spec.Id == invoiceId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvoice);
        _mockProductReadRepo.Setup(r => r.GetByIdAsync(productId1, It.IsAny<CancellationToken>())).ReturnsAsync(product1);
        _mockProductReadRepo.Setup(r => r.GetByIdAsync(productIdNew, It.IsAny<CancellationToken>())).ReturnsAsync(productNew);
        _mockInvoiceRepo.Setup(r => r.UpdateAsync(It.IsAny<VendorInvoice>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);


        var request = new UpdateVendorInvoiceRequest
        {
            Id = invoiceId,
            InvoiceItems = new List<UpdateVendorInvoiceItemRequest>
            {
                // Update existing item 1
                new UpdateVendorInvoiceItemRequest
                {
                    Id = existingItemId1, // Existing
                    Description = "Updated Item 1 Description",
                    Quantity = 2,
                    UnitPrice = 110m, // Price changed
                    TaxAmount = 22m,  // Tax changed
                    ProductId = productId1,
                    TotalAmount = 220m // 2 * 110
                },
                // Item 2 (existingItemId2) is omitted, so it should be removed.

                // Add new item
                new UpdateVendorInvoiceItemRequest
                {
                    Id = null, // New item
                    Description = "New Added Item",
                    Quantity = 1,
                    UnitPrice = 75m,
                    TaxAmount = 7.5m,
                    ProductId = productIdNew,
                    TotalAmount = 75m
                }
            }
            // Notes, dates etc. can be null if not changing
        };
        // Expected total:
        // Item 1 updated: (2 * 110) + 22 = 220 + 22 = 242
        // New Item: (1 * 75) + 7.5 = 75 + 7.5 = 82.5
        // Total = 242 + 82.5 = 324.5
        request.TotalAmount = 324.5m; // The handler will validate this against the sum of new items or recalculate.
                                      // The UpdateVendorInvoiceHandler recalculates TotalAmount from items if request.InvoiceItems is not null.

        var handler = new UpdateVendorInvoiceHandler(_mockInvoiceRepo.Object, _mockSupplierReadRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object, _mockProductReadRepo.Object);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        _mockInvoiceRepo.Verify(r => r.UpdateAsync(It.Is<VendorInvoice>(inv =>
            inv.Id == invoiceId &&
            inv.InvoiceItems.Count == 2 && // Item2 removed, new item added
            inv.InvoiceItems.Any(i => i.Id == existingItemId1 && i.Description == "Updated Item 1 Description" && i.Quantity == 2 && i.UnitPrice == 110m && i.TaxAmount == 22m && i.TotalAmount == 220m) &&
            inv.InvoiceItems.Any(i => i.Id != existingItemId1 && i.Id != existingItemId2 && i.Description == "New Added Item" && i.Quantity == 1 && i.UnitPrice == 75m && i.TaxAmount == 7.5m && i.TotalAmount == 75m) &&
            !inv.InvoiceItems.Any(i => i.Id == existingItemId2) && // Item2 is removed
            Math.Abs(inv.TotalAmount - 324.5m) < 0.001m // Check recalculated total
        ), It.IsAny<CancellationToken>()), Times.Once);

        // Also check the state of the `existingInvoice` object that was passed to the handler,
        // as the handler modifies this instance directly.
        existingInvoice.InvoiceItems.Should().HaveCount(2);
        existingInvoice.InvoiceItems.Should().ContainSingle(i => i.Id == existingItemId1 && i.Description == "Updated Item 1 Description");
        existingInvoice.InvoiceItems.Should().ContainSingle(i => i.Description == "New Added Item");
        existingInvoice.InvoiceItems.Should().NotContain(i => i.Id == existingItemId2);
        existingInvoice.TotalAmount.Should().Be(324.5m);
    }
}
