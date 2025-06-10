using Xunit;
using FluentAssertions;
using Moq;
using FSH.WebApi.Application.Accounting.CreditMemos; // Requests, Handlers, DTOs, Specs
using FSH.WebApi.Application.Accounting.CustomerInvoices; // For CustomerInvoice DTO for context
using FSH.WebApi.Domain.Accounting; // Domain entities
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

namespace Application.Tests.Accounting.CreditMemos;

public class CreditMemoHandlerTests
{
    private readonly Mock<IRepository<CreditMemo>> _mockCreditMemoRepo;
    private readonly Mock<IReadRepository<CreditMemo>> _mockCreditMemoReadRepo;
    private readonly Mock<IRepository<CustomerInvoice>> _mockInvoiceRepo; // Writable for applying credit
    private readonly Mock<IReadRepository<Customer>> _mockCustomerReadRepo;
    private readonly Mock<IReadRepository<CustomerInvoice>> _mockInvoiceReadRepo; // For validating OriginalCustomerInvoiceId

    private readonly Mock<IStringLocalizer<CreateCreditMemoHandler>> _mockCreateLocalizer;
    private readonly Mock<ILogger<CreateCreditMemoHandler>> _mockCreateLogger;
    private readonly Mock<IStringLocalizer<UpdateCreditMemoHandler>> _mockUpdateLocalizer;
    private readonly Mock<ILogger<UpdateCreditMemoHandler>> _mockUpdateLogger;
    private readonly Mock<IStringLocalizer<ApplyCreditMemoToInvoiceHandler>> _mockApplyLocalizer;
    private readonly Mock<ILogger<ApplyCreditMemoToInvoiceHandler>> _mockApplyLogger;
    private readonly Mock<IStringLocalizer<GetCreditMemoHandler>> _mockGetLocalizer;
    private readonly Mock<IStringLocalizer<SearchCreditMemosHandler>> _mockSearchLocalizer;

    public CreditMemoHandlerTests()
    {
        _mockCreditMemoRepo = new Mock<IRepository<CreditMemo>>();
        _mockCreditMemoReadRepo = new Mock<IReadRepository<CreditMemo>>();
        _mockInvoiceRepo = new Mock<IRepository<CustomerInvoice>>();
        _mockCustomerReadRepo = new Mock<IReadRepository<Customer>>();
        _mockInvoiceReadRepo = new Mock<IReadRepository<CustomerInvoice>>();


        _mockCreateLocalizer = new Mock<IStringLocalizer<CreateCreditMemoHandler>>();
        _mockCreateLogger = new Mock<ILogger<CreateCreditMemoHandler>>();
        _mockUpdateLocalizer = new Mock<IStringLocalizer<UpdateCreditMemoHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateCreditMemoHandler>>();
        _mockApplyLocalizer = new Mock<IStringLocalizer<ApplyCreditMemoToInvoiceHandler>>();
        _mockApplyLogger = new Mock<ILogger<ApplyCreditMemoToInvoiceHandler>>();
        _mockGetLocalizer = new Mock<IStringLocalizer<GetCreditMemoHandler>>();
        _mockSearchLocalizer = new Mock<IStringLocalizer<SearchCreditMemosHandler>>();

        SetupDefaultLocalizationMock(_mockCreateLocalizer);
        SetupDefaultLocalizationMock(_mockUpdateLocalizer);
        SetupDefaultLocalizationMock(_mockApplyLocalizer);
        SetupDefaultLocalizationMock(_mockGetLocalizer);
        SetupDefaultLocalizationMock(_mockSearchLocalizer);
    }

    private void SetupDefaultLocalizationMock<T>(Mock<IStringLocalizer<T>> mock) where T : class
    {
        mock.Setup(l => l[It.IsAny<string>()]).Returns((string name, object[] arguments) => new LocalizedString(name, name));
        mock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments), false, typeof(T).FullName));
    }

    // Helper Methods
    private Customer CreateSampleCustomer(Guid id, string name = "Test Customer") => new Customer(name, "1122334455", false) { Id = id };
    private CreditMemo CreateSampleCreditMemo(Guid id, Guid customerId, string number = "CM-001", decimal totalAmount = 100m, CreditMemoStatus status = CreditMemoStatus.Approved)
    {
        var cm = new CreditMemo(customerId, number, DateTime.UtcNow, "Reason", totalAmount, "USD", "Notes", null, status);
        cm.Id = id;
        return cm;
    }
    private CustomerInvoice CreateSampleCustomerInvoice(Guid id, Guid customerId, decimal totalAmount = 200m, string invNum = "INV-001", CustomerInvoiceStatus status = CustomerInvoiceStatus.Sent)
    {
        var invoice = new CustomerInvoice(customerId, invNum, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(20), "USD", null, null, status);
        invoice.Id = id;
        // Add item to reflect total amount for balance calculation
        if (totalAmount > 0 && !invoice.InvoiceItems.Any())
             invoice.AddInvoiceItem("Sample Item", 1, totalAmount, 0m);
        return invoice;
    }

    // === CreateCreditMemoHandler Tests ===
    [Fact]
    public async Task CreateCreditMemoHandler_Should_CreateCreditMemo_WhenValidRequest()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateCreditMemoRequest { CustomerId = customerId, Date = DateTime.UtcNow, Reason = "Test", TotalAmount = 100m, Currency = "USD" };
        var customer = CreateSampleCustomer(customerId);

        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _mockCreditMemoRepo.Setup(r => r.ListAsync(It.IsAny<CreditMemoByNumberPrefixSpec>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CreditMemo>()); // For number generation
        _mockCreditMemoRepo.Setup(r => r.AddAsync(It.IsAny<CreditMemo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditMemo cm, CancellationToken ct) => { cm.Id = Guid.NewGuid(); return cm; });

        var handler = new CreateCreditMemoHandler(_mockCreditMemoRepo.Object, _mockCustomerReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object, _mockInvoiceReadRepo.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _mockCreditMemoRepo.Verify(r => r.AddAsync(It.Is<CreditMemo>(cm => cm.CustomerId == customerId && cm.TotalAmount == request.TotalAmount), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCreditMemoHandler_Should_ThrowNotFoundException_WhenCustomerNotFound()
    {
        // Arrange
        var request = new CreateCreditMemoRequest { CustomerId = Guid.NewGuid(), Date = DateTime.UtcNow, Reason = "Test", TotalAmount = 100m, Currency = "USD" };
        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(request.CustomerId, It.IsAny<CancellationToken>())).ReturnsAsync((Customer)null!);
        var handler = new CreateCreditMemoHandler(_mockCreditMemoRepo.Object, _mockCustomerReadRepo.Object, _mockCreateLocalizer.Object, _mockCreateLogger.Object, _mockInvoiceReadRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === UpdateCreditMemoHandler Tests ===
    [Fact]
    public async Task UpdateCreditMemoHandler_Should_UpdateCreditMemo_WhenDraftAndValid()
    {
        // Arrange
        var creditMemoId = Guid.NewGuid();
        var request = new UpdateCreditMemoRequest { Id = creditMemoId, Reason = "Updated Reason", Notes = "New Notes" };
        var existingCreditMemo = CreateSampleCreditMemo(creditMemoId, Guid.NewGuid(), status: CreditMemoStatus.Draft);

        _mockCreditMemoRepo.Setup(r => r.GetByIdAsync(creditMemoId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCreditMemo);
        var handler = new UpdateCreditMemoHandler(_mockCreditMemoRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object, _mockInvoiceReadRepo.Object, _mockCustomerReadRepo.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(creditMemoId);
        _mockCreditMemoRepo.Verify(r => r.UpdateAsync(It.Is<CreditMemo>(cm => cm.Id == creditMemoId && cm.Reason == request.Reason && cm.Notes == request.Notes), It.IsAny<CancellationToken>()), Times.Once);
        existingCreditMemo.Reason.Should().Be(request.Reason);
    }

    [Fact]
    public async Task UpdateCreditMemoHandler_Should_ThrowConflictException_WhenNotDraftAndAttemptingMajorUpdate()
    {
        // Arrange
        var creditMemoId = Guid.NewGuid();
        var request = new UpdateCreditMemoRequest { Id = creditMemoId, Reason = "Attempting Update", TotalAmount = 200m }; // TotalAmount is major change
        var existingCreditMemo = CreateSampleCreditMemo(creditMemoId, Guid.NewGuid(), status: CreditMemoStatus.Approved); // Not Draft

        _mockCreditMemoRepo.Setup(r => r.GetByIdAsync(creditMemoId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCreditMemo);
        var handler = new UpdateCreditMemoHandler(_mockCreditMemoRepo.Object, _mockUpdateLocalizer.Object, _mockUpdateLogger.Object, _mockInvoiceReadRepo.Object, _mockCustomerReadRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }

    // === ApplyCreditMemoToInvoiceHandler Tests ===
    [Fact]
    public async Task ApplyCreditMemoToInvoiceHandler_Should_ApplyCredit_WhenValid()
    {
        // Arrange
        var creditMemoId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var amountToApply = 50m;
        var request = new ApplyCreditMemoToInvoiceRequest { CreditMemoId = creditMemoId, CustomerInvoiceId = invoiceId, AmountToApply = amountToApply };

        var creditMemo = CreateSampleCreditMemo(creditMemoId, customerId, totalAmount: 100m, status: CreditMemoStatus.Approved);
        var invoice = CreateSampleCustomerInvoice(invoiceId, customerId, totalAmount: 100m, status: CustomerInvoiceStatus.Sent);

        _mockCreditMemoRepo.Setup(r => r.GetByIdAsync(creditMemoId, It.IsAny<CancellationToken>())).ReturnsAsync(creditMemo);
        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

        var handler = new ApplyCreditMemoToInvoiceHandler(_mockCreditMemoRepo.Object, _mockInvoiceRepo.Object, _mockApplyLocalizer.Object, _mockApplyLogger.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().Be(creditMemoId); // Handler returns CreditMemoId
        creditMemo.GetAvailableBalance().Should().Be(50m); // 100 - 50
        creditMemo.Status.Should().Be(CreditMemoStatus.PartiallyApplied);
        invoice.AmountPaid.Should().Be(amountToApply); // Assuming ApplyPayment method is called
        invoice.Status.Should().Be(CustomerInvoiceStatus.PartiallyPaid); // Assuming ApplyPayment updates status

        _mockCreditMemoRepo.Verify(r => r.UpdateAsync(creditMemo, It.IsAny<CancellationToken>()), Times.Once);
        _mockInvoiceRepo.Verify(r => r.UpdateAsync(invoice, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApplyCreditMemoToInvoiceHandler_Should_ThrowConflict_WhenCreditMemoNotApproved()
    {
        var request = new ApplyCreditMemoToInvoiceRequest { CreditMemoId = Guid.NewGuid(), CustomerInvoiceId = Guid.NewGuid(), AmountToApply = 50m };
        var creditMemo = CreateSampleCreditMemo(request.CreditMemoId, Guid.NewGuid(), status: CreditMemoStatus.Draft);
        _mockCreditMemoRepo.Setup(r => r.GetByIdAsync(request.CreditMemoId, It.IsAny<CancellationToken>())).ReturnsAsync(creditMemo);
        _mockInvoiceRepo.Setup(r => r.GetByIdAsync(request.CustomerInvoiceId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateSampleCustomerInvoice(request.CustomerInvoiceId, creditMemo.CustomerId));
        var handler = new ApplyCreditMemoToInvoiceHandler(_mockCreditMemoRepo.Object, _mockInvoiceRepo.Object, _mockApplyLocalizer.Object, _mockApplyLogger.Object);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(request, CancellationToken.None));
    }


    // === GetCreditMemoHandler Tests ===
    [Fact]
    public async Task GetCreditMemoHandler_Should_ReturnDto_WhenFound()
    {
        // Arrange
        var creditMemoId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var request = new GetCreditMemoRequest(creditMemoId);
        var creditMemoEntity = CreateSampleCreditMemo(creditMemoId, customerId, "CM-GET-001");
        var customerEntity = CreateSampleCustomer(customerId, "CustomerForCMGet");

        _mockCreditMemoReadRepo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<CreditMemoByIdWithDetailsSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditMemoEntity); // Spec includes applications and their invoices
        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(customerEntity);
        // OriginalInvoiceId is null in sample, so no mock for _mockInvoiceReadRepo needed here

        var handler = new GetCreditMemoHandler(_mockCreditMemoReadRepo.Object, _mockCustomerReadRepo.Object, _mockInvoiceReadRepo.Object, _mockGetLocalizer.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(creditMemoId);
        result.CreditMemoNumber.Should().Be("CM-GET-001");
        result.CustomerName.Should().Be(customerEntity.Name);
        result.AvailableBalance.Should().Be(creditMemoEntity.GetAvailableBalance());
    }

    // === SearchCreditMemosHandler Tests ===
    [Fact]
    public async Task SearchCreditMemosHandler_Should_ReturnPaginatedDtos()
    {
        // Arrange
        var request = new SearchCreditMemosRequest { PageNumber = 1, PageSize = 5 };
        var customerId1 = Guid.NewGuid();
        var customer1 = CreateSampleCustomer(customerId1, "Customer CM Search");
        var creditMemoList = new List<CreditMemo>
        {
            CreateSampleCreditMemo(Guid.NewGuid(), customerId1, "CM-S-001"),
            CreateSampleCreditMemo(Guid.NewGuid(), customerId1, "CM-S-002")
        };

        _mockCreditMemoReadRepo.Setup(r => r.ListAsync(It.IsAny<CreditMemosBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditMemoList);
        _mockCreditMemoReadRepo.Setup(r => r.CountAsync(It.IsAny<CreditMemosBySearchFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(creditMemoList.Count);
        _mockCustomerReadRepo.Setup(r => r.GetByIdAsync(customerId1, It.IsAny<CancellationToken>())).ReturnsAsync(customer1);

        var handler = new SearchCreditMemosHandler(_mockCreditMemoReadRepo.Object, _mockCustomerReadRepo.Object, _mockInvoiceReadRepo.Object, _mockSearchLocalizer.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(creditMemoList.Count);
        result.Data.All(dto => dto.CustomerName == customer1.Name).Should().BeTrue();
        result.Data.First().AvailableBalance.Should().Be(creditMemoList.First().GetAvailableBalance());
    }
}
