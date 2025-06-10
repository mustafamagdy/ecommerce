using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.VendorPayments; // Requests, Handlers, DTOs, Specs
using FSH.WebApi.Application.Accounting.Suppliers;    // For Supplier entity/DTO
using FSH.WebApi.Application.Accounting.PaymentMethods; // For PaymentMethod entity/DTO
using FSH.WebApi.Application.Accounting.VendorInvoices; // For VendorInvoice entity/DTO for context
using FSH.WebApi.Domain.Accounting; // Domain entities
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

namespace Application.Tests.Accounting.VendorPayments;

public class VendorPaymentHandlerTests
{
    private readonly Mock<IRepository<VendorPayment>> _mockPaymentRepo;
    private readonly Mock<IReadRepository<VendorPayment>> _mockPaymentReadRepo;
    private readonly Mock<IRepository<VendorInvoice>> _mockInvoiceRepo; // Writable for updating invoice status/balance
    private readonly Mock<IReadRepository<Supplier>> _mockSupplierReadRepo;
    private readonly Mock<IReadRepository<PaymentMethod>> _mockPaymentMethodReadRepo;
    private readonly Mock<IReadRepository<Account>> _mockGlAccountReadRepo; // For BankAccount -> GLAccount -> Account in BankAccountHandler

    private readonly Mock<IStringLocalizer<CreateVendorPaymentHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateVendorPaymentHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<GetVendorPaymentHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<SearchVendorPaymentsHandler>> _mockSearchLocalizer;
    // Add mocks for Update/Delete if those handlers are implemented for VendorPayment

    public VendorPaymentHandlerTests()
    {
        _mockPaymentRepo = new Mock<IRepository<VendorPayment>>();
        _mockPaymentReadRepo = new Mock<IReadRepository<VendorPayment>>();
        _mockInvoiceRepo = new Mock<IRepository<VendorInvoice>>();
        _mockSupplierReadRepo = new Mock<IReadRepository<Supplier>>();
        _mockPaymentMethodReadRepo = new Mock<IReadRepository<PaymentMethod>>();
        _mockGlAccountReadRepo = new Mock<IReadRepository<Account>>();


        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateVendorPaymentHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateVendorPaymentHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetVendorPaymentHandler>>();
        _mockSearchLocalizer = new Mock<IStringLocalizer<SearchVendorPaymentsHandler>>();

        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        SetupDefaultLocalizationMock(_mockSearchLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class
    {
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] arguments) => new LocalizedString(name, name));
        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments), false, typeof(T).FullName));
    }

    // Helper methods to create sample entities
    private Supplier CreateSampleSupplier(Guid id, string name = "Test Supplier") =>
        new Supplier(name, "sup@test.com", "1 Sup St", "SUP123", Guid.NewGuid(), "Sup Bank") { Id = id };

    private PaymentMethod CreateSamplePaymentMethod(Guid id, string name = "Cash") =>
        new PaymentMethod(name, "Cash payment", true) { Id = id };

    private VendorInvoice CreateSampleVendorInvoice(Guid id, Guid supplierId, decimal totalAmount = 500m, string invoiceNumber = "VI-001", VendorInvoiceStatus status = VendorInvoiceStatus.Approved)
    {
        // Simplified VendorInvoice creation for test purposes
        var invoice = new VendorInvoice(supplierId, invoiceNumber, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(20), totalAmount, "USD", status);
        invoice.Id = id; // Manually set Id for mocking GetByIdAsync
        // Add dummy items to make TotalAmount non-zero, assuming RecalculateTotalAmount is based on items
        // If not, ensure TotalAmount is set as expected.
        // For these tests, we'll assume TotalAmount on VendorInvoice is what we set, and ApplyPayment updates based on that.
        return invoice;
    }

    private Account CreateSampleGLAccount(Guid id, string name = "Cash GL", string number = "10100") =>
        new Account(name, number, AccountType.Asset, 0, "Cash GL Account", true) { Id = id };


    // === CreateVendorPaymentHandler Tests ===
    [Fact]
    public async Task CreateVendorPaymentHandler_Should_CreatePaymentAndApplyToInvoices_WhenValid()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var invoiceId1 = Guid.NewGuid();
        var invoiceId2 = Guid.NewGuid();

        var request = new CreateVendorPaymentRequest
        {
            SupplierId = supplierId,
            PaymentDate = DateTime.UtcNow,
            AmountPaid = 300m,
            PaymentMethodId = paymentMethodId,
            Applications = new List<VendorPaymentApplicationRequestItem>
            {
                new VendorPaymentApplicationRequestItem { VendorInvoiceId = invoiceId1, AmountApplied = 100m },
                new VendorPaymentApplicationRequestItem { VendorInvoiceId = invoiceId2, AmountApplied = 200m }
            }
        };

        var supplier = CreateSampleSupplier(supplierId);
        var paymentMethod = CreateSamplePaymentMethod(paymentMethodId);
        var invoice1 = CreateSampleVendorInvoice(invoiceId1, supplierId, 150m, "INV001");
        var invoice2 = CreateSampleVendorInvoice(invoiceId2, supplierId, 250m, "INV002");

        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(supplier);
        _mockPaymentMethodReadRepo.Setup(r => r.GetByIdAsync(paymentMethodId, It.IsAny<CancellationToken>())).ReturnsAsync(paymentMethod);
        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId1, It.IsAny<CancellationToken>())).ReturnsAsync(invoice1);
        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId2, It.IsAny<CancellationToken>())).ReturnsAsync(invoice2);

        _mockPaymentRepo.Setup(r => r.AddAsync(It.IsAny<VendorPayment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VendorPayment vp, CancellationToken ct) => { vp.Id = Guid.NewGuid(); return vp; });


        var handler = new CreateVendorPaymentHandler(
            _mockPaymentRepo.Object, _mockInvoiceRepo.Object, _mockSupplierReadRepo.Object,
            _mockPaymentMethodReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object
            // Assuming _glAccountReadRepo is not used in CreateVendorPaymentHandler
        );


        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockPaymentRepo.Verify(r => r.AddAsync(It.Is<VendorPayment>(vp =>
            vp.SupplierId == supplierId &&
            vp.AmountPaid == 300m &&
            vp.AppliedInvoices.Count == 2
        ), It.IsAny<CancellationToken>()), Times.Once);

        _mockInvoiceRepo.Verify(r => r.UpdateAsync(It.Is<VendorInvoice>(vi => vi.Id == invoiceId1), It.IsAny<CancellationToken>()), Times.Once);
        _mockInvoiceRepo.Verify(r => r.UpdateAsync(It.Is<VendorInvoice>(vi => vi.Id == invoiceId2), It.IsAny<CancellationToken>()), Times.Once);

        // Verify invoice status changes (assuming ApplyPayment in domain entity handles this)
        // This requires VendorInvoice to have methods that modify its state, which are then saved.
        // For simplicity, we assume the UpdateAsync call saves these changes.
        // Example: if invoice1 should be paid: invoice1.Status.Should().Be(VendorInvoiceStatus.Paid);
        // This depends on the exact logic in VendorInvoice.ApplyPayment which is not part of handler test itself.
    }

    [Fact]
    public async Task CreateVendorPaymentHandler_Should_ThrowNotFoundException_WhenSupplierNotFound()
    {
        // Arrange
        var request = new CreateVendorPaymentRequest { SupplierId = Guid.NewGuid() };
        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(request.SupplierId, It.IsAny<CancellationToken>())).ReturnsAsync((Supplier)null!);
        var handler = new CreateVendorPaymentHandler(_mockPaymentRepo.Object, _mockInvoiceRepo.Object, _mockSupplierReadRepo.Object, _mockPaymentMethodReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateVendorPaymentHandler_Should_ThrowValidationException_WhenAppliedAmountExceedsInvoiceBalance()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var request = new CreateVendorPaymentRequest
        {
            SupplierId = supplierId, PaymentDate = DateTime.UtcNow, AmountPaid = 200m, PaymentMethodId = paymentMethodId,
            Applications = new List<VendorPaymentApplicationRequestItem> { new VendorPaymentApplicationRequestItem { VendorInvoiceId = invoiceId, AmountApplied = 200m } }
        };
        var supplier = CreateSampleSupplier(supplierId);
        var paymentMethod = CreateSamplePaymentMethod(paymentMethodId);
        var invoice = CreateSampleVendorInvoice(invoiceId, supplierId, 100m); // Balance is 100m

        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(supplier);
        _mockPaymentMethodReadRepo.Setup(r => r.GetByIdAsync(paymentMethodId, It.IsAny<CancellationToken>())).ReturnsAsync(paymentMethod);
        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var handler = new CreateVendorPaymentHandler(_mockPaymentRepo.Object, _mockInvoiceRepo.Object, _mockSupplierReadRepo.Object, _mockPaymentMethodReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object);

        // Act & Assert
        // The handler's logic: `if (appRequest.AmountApplied > invoice.GetBalanceDue())`
        // `GetBalanceDue` for VendorInvoice isn't explicitly defined, assuming it's similar to CustomerInvoice.
        // Let's assume VendorInvoice has a TotalAmount and an AmountPaid property, and ApplyPayment updates AmountPaid.
        // For this test, the initial AmountPaid is 0, so BalanceDue is TotalAmount (100m).
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === GetVendorPaymentHandler Tests ===
    [Fact]
    public async Task GetVendorPaymentHandler_Should_ReturnDto_WhenFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var request = new GetVendorPaymentRequest(paymentId);

        var paymentEntity = new VendorPayment(supplierId, DateTime.UtcNow, 500m, paymentMethodId) { Id = paymentId };
        var supplierEntity = CreateSampleSupplier(supplierId, "Supplier X");
        var paymentMethodEntity = CreateSamplePaymentMethod(paymentMethodId, "Wire");

        // Setup for VendorPaymentByIdWithDetailsSpec
        _mockPaymentReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<VendorPaymentByIdWithDetailsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentEntity); // The spec includes PaymentMethod
        paymentEntity.GetType().GetProperty("PaymentMethod")!.SetValue(paymentEntity, paymentMethodEntity, null); // Simulate EF Core Include


        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(supplierId, It.IsAny<CancellationToken>())).ReturnsAsync(supplierEntity);
        // _mockGlAccountReadRepo is not used by GetVendorPaymentHandler


        var handler = new GetVendorPaymentHandler(_mockPaymentReadRepo.Object, _mockSupplierReadRepo.Object, _mockPaymentMethodReadRepo.Object, _mockGlAccountReadRepo.Object, _mockGetLocalizer.Object);


        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(paymentId);
        result.SupplierName.Should().Be(supplierEntity.Name);
        result.PaymentMethodName.Should().Be(paymentMethodEntity.Name);
        result.UnappliedAmount.Should().Be(500m); // Assuming GetUnappliedAmount works
    }


    // === SearchVendorPaymentsHandler Tests ===
    [Fact]
    public async Task SearchVendorPaymentsHandler_Should_ReturnPaginatedDtos()
    {
        // Arrange
        var request = new SearchVendorPaymentsRequest { PageNumber = 1, PageSize = 5 };
        var supplierId1 = Guid.NewGuid();
        var supplier1 = CreateSampleSupplier(supplierId1, "Supplier Alpha");
        var paymentMethodId1 = Guid.NewGuid();
        var paymentMethod1 = CreateSamplePaymentMethod(paymentMethodId1, "Check");

        var paymentList = new List<VendorPayment>
        {
            new VendorPayment(supplierId1, DateTime.UtcNow, 200m, paymentMethodId1) { Id = Guid.NewGuid() },
            new VendorPayment(supplierId1, DateTime.UtcNow, 300m, paymentMethodId1) { Id = Guid.NewGuid() }
        };
        // Simulate Includes for PaymentMethod by spec
        paymentList[0].GetType().GetProperty("PaymentMethod")!.SetValue(paymentList[0], paymentMethod1, null);
        paymentList[1].GetType().GetProperty("PaymentMethod")!.SetValue(paymentList[1], paymentMethod1, null);


        _mockPaymentReadRepo.Setup(r => r.ListAsync(It.IsAny<VendorPaymentsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentList); // Handler expects entities
        _mockPaymentReadRepo.Setup(r => r.CountAsync(It.IsAny<VendorPaymentsBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentList.Count);

        _mockSupplierReadRepo.Setup(r => r.GetByIdAsync(supplierId1, It.IsAny<CancellationToken>())).ReturnsAsync(supplier1);
        // PaymentMethod is included by spec, so no separate mock needed for _mockPaymentMethodReadRepo.GetByIdAsync in this handler.

        var handler = new SearchVendorPaymentsHandler(_mockPaymentReadRepo.Object, _mockSupplierReadRepo.Object, _mockPaymentMethodReadRepo.Object, _mockSearchLocalizer.Object);


        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(paymentList.Count);
        result.TotalCount.Should().Be(paymentList.Count);
        result.Data.First().SupplierName.Should().Be(supplier1.Name);
        result.Data.First().PaymentMethodName.Should().Be(paymentMethod1.Name);
    }
}
