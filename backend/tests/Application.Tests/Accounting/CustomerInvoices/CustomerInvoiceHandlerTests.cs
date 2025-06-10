using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.CustomerInvoices; // Requests, Handlers, DTOs, Specs
using FSH.WebApi.Domain.Accounting; // CustomerInvoice, CustomerInvoiceItem, CustomerInvoiceStatus
using FSH.WebApi.Domain.Operation.Customers; // Customer entity
using FSH.WebApi.Domain.Operation.Orders;    // Order entity
using FSH.WebApi.Domain.Catalog;           // Product entity
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

namespace Application.Tests.Accounting.CustomerInvoices;

public class CustomerInvoiceHandlerTests
{
    private readonly Mock<IRepository<CustomerInvoice>> _mockInvoiceRepo;
    private readonly Mock<IReadRepository<CustomerInvoice>> _mockInvoiceReadRepo;
    private readonly Mock<IReadRepository<Customer>> _mockCustomerReadRepo;
    private readonly Mock<IReadRepository<Order>> _mockOrderReadRepo;
    private readonly Mock<IReadRepository<Product>> _mockProductReadRepo;

    private readonly Mock<IStringLocalizer<CreateCustomerInvoiceHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateCustomerInvoiceHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdateCustomerInvoiceHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdateCustomerInvoiceHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<GetCustomerInvoiceHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<SearchCustomerInvoicesHandler>> _mockSearchLocalizer;
    private readonly Mock<IStringLocalizer<DeleteCustomerInvoiceHandler>> _mockDeleteLocalizer;
    private readonly Mock<ILogger<DeleteCustomerInvoiceHandler>> _mockDeleteLogger;

    public CustomerInvoiceHandlerTests()
    {
        _mockInvoiceRepo = new Mock<IRepository<CustomerInvoice>>();
        _mockInvoiceReadRepo = new Mock<IReadRepository<CustomerInvoice>>();
        _mockCustomerReadRepo = new Mock<IReadRepository<Customer>>();
        _mockOrderReadRepo = new Mock<IReadRepository<Order>>();
        _mockProductReadRepo = new Mock<IReadRepository<Product>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateCustomerInvoiceHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateCustomerInvoiceHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdateCustomerInvoiceHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateCustomerInvoiceHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetCustomerInvoiceHandler>>();
        _mockSearchLocalizer = new Mock<IStringLocalizer<SearchCustomerInvoicesHandler>>();
        _mockDeleteLocalizer = new Mock<IStringLocalizer<DeleteCustomerInvoiceHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeleteCustomerInvoiceHandler>>();

        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockUpdateLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        SetupDefaultLocalizationMock(_mockSearchLocalizer); // Though not used by Search handler
        SetupDefaultLocalizationMock(_mockDeleteLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class
    {
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] arguments) => new LocalizedString(name, name));
        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments), false, typeof(T).FullName));
    }

    // Helper Methods for Entities
    private Customer CreateSampleCustomer(Guid id, string name = "Test Customer") => new Customer(name, "1234567890", false) { Id = id };
    private Order CreateSampleOrder(Guid id, Guid customerId, string orderNumber = "ORD-001")
    {
        var customer = CreateSampleCustomer(customerId);
        return new Order(customer, orderNumber, DateTime.UtcNow) { Id = id };
    }
    private Product CreateSampleProduct(Guid id, string name = "Test Product") => new Product(name, "Desc", 10m, "path", Guid.NewGuid(), 1) { Id = id };
    private CustomerInvoice CreateSampleCustomerInvoice(Guid id, Guid customerId, string invoiceNumber = "CI-001", CustomerInvoiceStatus status = CustomerInvoiceStatus.Draft, decimal amountPaid = 0m)
    {
        var invoice = new CustomerInvoice(customerId, invoiceNumber, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "USD", null, null, status);
        invoice.Id = id;
        // Simulate AmountPaid for testing delete/update logic
        if (amountPaid > 0)
        {
            // This is a hack for testing, real AmountPaid is updated via ApplyPayment.
            // For handler tests, we construct the state as needed.
            // invoice.ApplyPayment(amountPaid); // This would change status too.
            // Instead, we might need to mock the state of AmountPaid if directly read by handler.
            // For now, let's assume handler checks status and AmountPaid property.
            // A better way is to ensure the entity reflects this state via its methods.
            // For simplicity, we'll rely on status for now for delete/update checks.
        }
        if (status == CustomerInvoiceStatus.Paid) invoice.GetType().GetProperty("AmountPaid")!.SetValue(invoice, invoice.TotalAmount);
        else if (status == CustomerInvoiceStatus.PartiallyPaid) invoice.GetType().GetProperty("AmountPaid")!.SetValue(invoice, invoice.TotalAmount > 0 ? invoice.TotalAmount / 2 : 0);


        return invoice;
    }


    // === CreateCustomerInvoiceHandler Tests ===
    [Fact]
    public async Task CreateCustomerInvoiceHandler_Should_CreateInvoice_WhenValidRequest()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new CreateCustomerInvoiceRequest
        {
            CustomerId = customerId, OrderId = orderId, InvoiceDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30), Currency = "USD",
            InvoiceItems = new List<CreateCustomerInvoiceItemRequest> { new CreateCustomerInvoiceItemRequest { Description = "Item 1", ProductId = productId, Quantity = 1, UnitPrice = 100, TaxAmount = 10 } }
        };

        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleCustomer(customerId));
        _mockOrderReadRepo.Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleOrder(orderId, customerId));
        _mockProductReadRepo.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleProduct(productId));
        _mockInvoiceRepo.Setup(r => r.ListAsync(It.IsAny<CustomerInvoiceByNumberPrefixSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CustomerInvoice>()); // For number generation
        _mockInvoiceRepo.Setup(r => r.AddAsync(It.IsAny<CustomerInvoice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerInvoice ci, CancellationToken ct) => { ci.Id = Guid.NewGuid(); return ci; });

        var handler = new CreateCustomerInvoiceHandler(_mockInvoiceRepo.Object, _mockCustomerReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object, _mockOrderReadRepo.Object, _mockProductReadRepo.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockInvoiceRepo.Verify(r => r.AddAsync(It.Is<CustomerInvoice>(ci => ci.CustomerId == customerId && ci.InvoiceItems.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCustomerInvoiceHandler_Should_ThrowNotFoundException_WhenCustomerNotFound()
    {
        // Arrange
        var request = new CreateCustomerInvoiceRequest { CustomerId = Guid.NewGuid(), InvoiceItems = new List<CreateCustomerInvoiceItemRequest>() };
        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(request.CustomerId, It.IsAny<CancellationToken>())).ReturnsAsync((Customer)null!);
        var handler = new CreateCustomerInvoiceHandler(_mockInvoiceRepo.Object, _mockCustomerReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object, _mockOrderReadRepo.Object, _mockProductReadRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === UpdateCustomerInvoiceHandler Tests ===
    [Fact]
    public async Task UpdateCustomerInvoiceHandler_Should_UpdateInvoice_WhenFoundAndMutable()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var request = new UpdateCustomerInvoiceRequest { Id = invoiceId, Notes = "Updated notes here." };
        var existingInvoice = CreateSampleCustomerInvoice(invoiceId, Guid.NewGuid(), status: CustomerInvoiceStatus.Draft);

        _mockInvoiceRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<CustomerInvoiceByIdWithDetailsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvoice);

        var handler = new UpdateCustomerInvoiceHandler(_mockInvoiceRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object, _mockProductReadRepo.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(invoiceId);
        _mockInvoiceRepo.Verify(r => r.UpdateAsync(It.Is<CustomerInvoice>(ci => ci.Id == invoiceId && ci.Notes == request.Notes), It.IsAny<CancellationToken>()), Times.Once);
        existingInvoice.Notes.Should().Be(request.Notes);
    }

    [Fact]
    public async Task UpdateCustomerInvoiceHandler_Should_ThrowConflictException_WhenInvoiceIsPaid()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var request = new UpdateCustomerInvoiceRequest { Id = invoiceId, Notes = "Attempt update on paid." };
        var existingInvoice = CreateSampleCustomerInvoice(invoiceId, Guid.NewGuid(), status: CustomerInvoiceStatus.Paid);
        _mockInvoiceRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<CustomerInvoiceByIdWithDetailsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingInvoice);
        var handler = new UpdateCustomerInvoiceHandler(_mockInvoiceRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object, _mockProductReadRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === GetCustomerInvoiceHandler Tests ===
    [Fact]
    public async Task GetCustomerInvoiceHandler_Should_ReturnDto_WhenFound()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var request = new GetCustomerInvoiceRequest(invoiceId);
        var invoiceEntity = CreateSampleCustomerInvoice(invoiceId, customerId, "CI-GET-001");
        var customerEntity = CreateSampleCustomer(customerId, "CustomerForGet");

        _mockInvoiceReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<CustomerInvoiceByIdWithDetailsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceEntity); // Handler expects entity from this spec call
        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerEntity);

        var handler = new GetCustomerInvoiceHandler(_mockInvoiceReadRepo.Object, _mockGetLocalizer.Object, _mockCustomerReadRepo.Object, _mockOrderReadRepo.Object, _mockProductReadRepo.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(invoiceId);
        result.InvoiceNumber.Should().Be("CI-GET-001");
        result.CustomerName.Should().Be(customerEntity.Name);
    }


    // === SearchCustomerInvoicesHandler Tests ===
    [Fact]
    public async Task SearchCustomerInvoicesHandler_Should_ReturnPaginatedDtos()
    {
        // Arrange
        var request = new SearchCustomerInvoicesRequest { PageNumber = 1, PageSize = 5 };
        var customerId1 = Guid.NewGuid();
        var customer1 = CreateSampleCustomer(customerId1, "Customer Alpha");
        var invoiceList = new List<CustomerInvoice>
        {
            CreateSampleCustomerInvoice(Guid.NewGuid(), customerId1, "CI-S-001"),
            CreateSampleCustomerInvoice(Guid.NewGuid(), customerId1, "CI-S-002")
        };

        _mockInvoiceReadRepo.Setup(r => r.ListAsync(It.IsAny<CustomerInvoicesBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceList);
        _mockInvoiceReadRepo.Setup(r => r.CountAsync(It.IsAny<CustomerInvoicesBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoiceList.Count);
        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(customerId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer1);

        var handler = new SearchCustomerInvoicesHandler(_mockInvoiceReadRepo.Object, _mockSearchLocalizer.Object, _mockCustomerReadRepo.Object, _mockProductReadRepo.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(invoiceList.Count);
        result.Data.First().CustomerName.Should().Be(customer1.Name);
    }


    // === DeleteCustomerInvoiceHandler Tests ===
    [Fact]
    public async Task DeleteCustomerInvoiceHandler_Should_DeleteInvoice_WhenDraftAndNoPayments()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var request = new DeleteCustomerInvoiceRequest(invoiceId);
        var invoiceEntity = CreateSampleCustomerInvoice(invoiceId, Guid.NewGuid(), "CI-DEL-001", CustomerInvoiceStatus.Draft, amountPaid: 0m);

        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoiceEntity);
        var handler = new DeleteCustomerInvoiceHandler(_mockInvoiceRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(invoiceId);
        _mockInvoiceRepo.Verify(r => r.DeleteAsync(invoiceEntity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomerInvoiceHandler_Should_ThrowConflictException_WhenInvoiceIsPaid()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var request = new DeleteCustomerInvoiceRequest(invoiceId);
        // CreateSampleCustomerInvoice helper sets AmountPaid via reflection hack for testing this state
        var invoiceEntity = CreateSampleCustomerInvoice(invoiceId, Guid.NewGuid(), "CI-PAID-001", CustomerInvoiceStatus.Paid);
        // Manually ensure AmountPaid is > 0 for this test case, if helper doesn't cover it for Paid status
        typeof(CustomerInvoice).GetProperty(nameof(CustomerInvoice.AmountPaid))!.SetValue(invoiceEntity, 100m);


        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoiceEntity);
        var handler = new DeleteCustomerInvoiceHandler(_mockInvoiceRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }

     [Fact]
    public async Task DeleteCustomerInvoiceHandler_Should_ThrowConflictException_WhenAmountPaidIsGreaterThanZero()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var request = new DeleteCustomerInvoiceRequest(invoiceId);
        var invoiceEntity = CreateSampleCustomerInvoice(invoiceId, Guid.NewGuid(), "CI-PARTPAID-001", CustomerInvoiceStatus.PartiallyPaid);
        // Manually set AmountPaid for this test case
        typeof(CustomerInvoice).GetProperty(nameof(CustomerInvoice.AmountPaid))!.SetValue(invoiceEntity, 50m);


        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoiceEntity);
        var handler = new DeleteCustomerInvoiceHandler(_mockInvoiceRepo.Object, _mockDeleteLocalizer.Object, _mockDeleteLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }
}
