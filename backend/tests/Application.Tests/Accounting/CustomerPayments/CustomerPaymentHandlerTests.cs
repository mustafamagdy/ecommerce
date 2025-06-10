using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.CustomerPayments; // Requests, Handlers, DTOs, Specs
using FSH.WebApi.Application.Accounting.CustomerInvoices; // For CustomerInvoice DTO for context
using FSH.WebApi.Domain.Accounting; // CustomerPayment, CustomerPaymentApplication, CustomerInvoice, PaymentMethod
using FSH.WebApi.Domain.Operation.Customers; // Customer entity
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

namespace Application.Tests.Accounting.CustomerPayments;

public class CustomerPaymentHandlerTests
{
    private readonly Mock<IRepository<CustomerPayment>> _mockPaymentRepo;
    private readonly Mock<IReadRepository<CustomerPayment>> _mockPaymentReadRepo;
    private readonly Mock<IRepository<CustomerInvoice>> _mockInvoiceRepo;
    private readonly Mock<IReadRepository<Customer>> _mockCustomerReadRepo;
    private readonly Mock<IReadRepository<PaymentMethod>> _mockPaymentMethodReadRepo;

    private readonly Mock<IStringLocalizer<CreateCustomerPaymentHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateCustomerPaymentHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<GetCustomerPaymentHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<SearchCustomerPaymentsHandler>> _mockSearchLocalizer;

    public CustomerPaymentHandlerTests()
    {
        _mockPaymentRepo = new Mock<IRepository<CustomerPayment>>();
        _mockPaymentReadRepo = new Mock<IReadRepository<CustomerPayment>>();
        _mockInvoiceRepo = new Mock<IRepository<CustomerInvoice>>();
        _mockCustomerReadRepo = new Mock<IReadRepository<Customer>>();
        _mockPaymentMethodReadRepo = new Mock<IReadRepository<PaymentMethod>>();

        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateCustomerPaymentHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateCustomerPaymentHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetCustomerPaymentHandler>>();
        _mockSearchLocalizer = new Mock<IStringLocalizer<SearchCustomerPaymentsHandler>>(); // Not used by handler but good for consistency

        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        // SetupDefaultLocalizationMock(_mockSearchLocalizer); // Not needed for Search handler
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class
    {
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] arguments) => new LocalizedString(name, name));
        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments), false, typeof(T).FullName));
    }

    // Helper Methods
    private Customer CreateSampleCustomer(Guid id, string name = "Test Customer") => new Customer(name, "9876543210", false) { Id = id };
    private PaymentMethod CreateSamplePaymentMethod(Guid id, string name = "Bank Deposit") => new PaymentMethod(name, "Direct bank deposit", true) { Id = id };
    private CustomerInvoice CreateSampleCustomerInvoice(Guid id, Guid customerId, decimal totalAmount = 1000m, string invoiceNumber = "CI-PAY-001", CustomerInvoiceStatus status = CustomerInvoiceStatus.Sent)
    {
        var invoice = new CustomerInvoice(customerId, invoiceNumber, DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(15), "USD", "Sample invoice for payment", null, status);
        invoice.Id = id;
        // Add items to make TotalAmount realistic, assuming domain entity calculates from items.
        // The CustomerInvoice.ApplyPayment will reduce its internal balance.
        // For testing, we need the invoice to have a balance.
        // If invoice.TotalAmount is 0, GetBalanceDue will be 0.
        // Let's assume the TotalAmount is set by adding items (or directly for test simplicity if constructor allows)
        // The CustomerInvoice domain entity's AddInvoiceItem updates TotalAmount.
        if (totalAmount > 0 && !invoice.InvoiceItems.Any())
        {
            invoice.AddInvoiceItem("Test Item For Payment", 1, totalAmount, 0m); // Tax 0 for simplicity here
        }
        return invoice;
    }
    private CustomerPayment CreateSampleCustomerPayment(Guid id, Guid customerId, Guid paymentMethodId, decimal amountReceived)
    {
        var payment = new CustomerPayment(customerId, DateTime.UtcNow, amountReceived, paymentMethodId, $"CPAY-{id.ToString().Substring(0,4)}", "Notes");
        payment.Id = id;
        return payment;
    }


    // === CreateCustomerPaymentHandler Tests ===
    [Fact]
    public async Task CreateCustomerPaymentHandler_Should_CreatePaymentAndApplyToInvoice_WhenValid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var amountToApply = 200m;

        var request = new CreateCustomerPaymentRequest
        {
            CustomerId = customerId, PaymentDate = DateTime.UtcNow, AmountReceived = amountToApply, PaymentMethodId = paymentMethodId,
            Applications = new List<CustomerPaymentApplicationRequestItem> { new CustomerPaymentApplicationRequestItem { CustomerInvoiceId = invoiceId, AmountApplied = amountToApply } }
        };

        var customer = CreateSampleCustomer(customerId);
        var paymentMethod = CreateSamplePaymentMethod(paymentMethodId);
        var invoice = CreateSampleCustomerInvoice(invoiceId, customerId, totalAmount: amountToApply + 50m); // Invoice total 250, applying 200

        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _mockPaymentMethodReadRepo.Setup(r => r.GetByIdAsync(paymentMethodId, It.IsAny<CancellationToken>())).ReturnsAsync(paymentMethod);
        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
        _mockPaymentRepo.Setup(r => r.AddAsync(It.IsAny<CustomerPayment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerPayment cp, CancellationToken ct) => { cp.Id = Guid.NewGuid(); return cp; });

        var handler = new CreateCustomerPaymentHandler(_mockPaymentRepo.Object, _mockInvoiceRepo.Object, _mockCustomerReadRepo.Object, _mockPaymentMethodReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockPaymentRepo.Verify(r => r.AddAsync(It.Is<CustomerPayment>(cp => cp.AmountReceived == amountToApply && cp.AppliedInvoices.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
        _mockInvoiceRepo.Verify(r => r.UpdateAsync(It.Is<CustomerInvoice>(ci => ci.Id == invoiceId), It.IsAny<CancellationToken>()), Times.Once);
        // To check if invoice.ApplyPayment was effectively called:
        invoice.AmountPaid.Should().Be(amountToApply);
        invoice.Status.Should().Be(CustomerInvoiceStatus.PartiallyPaid); // (250 total - 200 paid)
    }

    [Fact]
    public async Task CreateCustomerPaymentHandler_Should_ThrowNotFoundException_WhenCustomerNotFound()
    {
        // Arrange
        var request = new CreateCustomerPaymentRequest { CustomerId = Guid.NewGuid() };
        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(request.CustomerId, It.IsAny<CancellationToken>())).ReturnsAsync((Customer)null!);
        var handler = new CreateCustomerPaymentHandler(_mockPaymentRepo.Object, _mockInvoiceRepo.Object, _mockCustomerReadRepo.Object, _mockPaymentMethodReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateCustomerPaymentHandler_Should_ThrowConflictException_WhenAppliedAmountExceedsInvoiceBalance()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var invoiceTotal = 100m;
        var amountToApply = 150m; // More than invoice total

        var request = new CreateCustomerPaymentRequest
        {
            CustomerId = customerId, PaymentDate = DateTime.UtcNow, AmountReceived = amountToApply, PaymentMethodId = paymentMethodId,
            Applications = new List<CustomerPaymentApplicationRequestItem> { new CustomerPaymentApplicationRequestItem { CustomerInvoiceId = invoiceId, AmountApplied = amountToApply } }
        };

        var customer = CreateSampleCustomer(customerId);
        var paymentMethod = CreateSamplePaymentMethod(paymentMethodId);
        var invoice = CreateSampleCustomerInvoice(invoiceId, customerId, totalAmount: invoiceTotal);

        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _mockPaymentMethodReadRepo.Setup(r => r.GetByIdAsync(paymentMethodId, It.IsAny<CancellationToken>())).ReturnsAsync(paymentMethod);
        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var handler = new CreateCustomerPaymentHandler(_mockPaymentRepo.Object, _mockInvoiceRepo.Object, _mockCustomerReadRepo.Object, _mockPaymentMethodReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === GetCustomerPaymentHandler Tests ===
    [Fact]
    public async Task GetCustomerPaymentHandler_Should_ReturnDto_WhenFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var request = new GetCustomerPaymentRequest(paymentId);

        var paymentEntity = CreateSampleCustomerPayment(paymentId, customerId, paymentMethodId, 300m);
        paymentEntity.AddPaymentApplication(invoiceId, 200m); // Partially applied

        var customerEntity = CreateSampleCustomer(customerId, "Customer ABC");
        var paymentMethodEntity = CreateSamplePaymentMethod(paymentMethodId, "CreditCard");
        var invoiceEntity = CreateSampleCustomerInvoice(invoiceId, customerId, 200m, "INV-FOR-PAY");


        // The spec CustomerPaymentByIdWithDetailsSpec includes PaymentMethod and Applications with their Invoices
        _mockPaymentReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<CustomerPaymentByIdWithDetailsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentEntity);
        // Simulate includes by EF Core
        paymentEntity.GetType().GetProperty("PaymentMethod")!.SetValue(paymentEntity, paymentMethodEntity, null);
        paymentEntity.Applications.First().GetType().GetProperty("CustomerInvoice")!.SetValue(paymentEntity.Applications.First(), invoiceEntity, null);


        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(customerEntity);

        var handler = new GetCustomerPaymentHandler(_mockPaymentReadRepo.Object, _mockCustomerReadRepo.Object, _mockGetLocalizer.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(paymentId);
        result.CustomerName.Should().Be(customerEntity.Name);
        result.PaymentMethodName.Should().Be(paymentMethodEntity.Name);
        result.AmountReceived.Should().Be(300m);
        result.UnappliedAmount.Should().Be(100m);
        result.AppliedInvoices.Should().HaveCount(1);
        result.AppliedInvoices.First().CustomerInvoiceId.Should().Be(invoiceId);
        result.AppliedInvoices.First().CustomerInvoiceNumber.Should().Be(invoiceEntity.InvoiceNumber);
        result.AppliedInvoices.First().AmountApplied.Should().Be(200m);
    }


    // === SearchCustomerPaymentsHandler Tests ===
    [Fact]
    public async Task SearchCustomerPaymentsHandler_Should_ReturnPaginatedDtos()
    {
        // Arrange
        var request = new SearchCustomerPaymentsRequest { PageNumber = 1, PageSize = 5 };
        var customerId1 = Guid.NewGuid();
        var customer1 = CreateSampleCustomer(customerId1, "Customer Search Co");
        var paymentMethodId1 = Guid.NewGuid();
        var paymentMethod1 = CreateSamplePaymentMethod(paymentMethodId1, "EFT");

        var paymentList = new List<CustomerPayment>
        {
            CreateSampleCustomerPayment(Guid.NewGuid(), customerId1, paymentMethodId1, 250m),
            CreateSampleCustomerPayment(Guid.NewGuid(), customerId1, paymentMethodId1, 350m)
        };
        // Simulate Includes by spec
        paymentList.ForEach(p => p.GetType().GetProperty("PaymentMethod")!.SetValue(p, paymentMethod1, null));


        _mockPaymentReadRepo.Setup(r => r.ListAsync(It.IsAny<CustomerPaymentsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentList);
        _mockPaymentReadRepo.Setup(r => r.CountAsync(It.IsAny<CustomerPaymentsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentList.Count);
        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(customerId1, It.IsAny<CancellationToken>())).ReturnsAsync(customer1);


        var handler = new SearchCustomerPaymentsHandler(_mockPaymentReadRepo.Object, _mockCustomerReadRepo.Object, _mockSearchLocalizer.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(paymentList.Count);
        result.TotalCount.Should().Be(paymentList.Count);
        result.Data.All(dto => dto.CustomerName == customer1.Name).Should().BeTrue();
        result.Data.All(dto => dto.PaymentMethodName == paymentMethod1.Name).Should().BeTrue();
        result.Data.First().UnappliedAmount.Should().Be(250m); // As no applications were added in helper
    }
}
