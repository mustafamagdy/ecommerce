using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For CreditMemo, CreditMemoApplication, CreditMemoStatus
using System;
using System.Linq;

namespace Application.Tests.Accounting.CreditMemos;

public class CreditMemoTests
{
    private CreditMemo CreateTestCreditMemo(
        decimal totalAmount = 500m,
        CreditMemoStatus status = CreditMemoStatus.Approved, // Assume approved for easier application testing
        Guid? customerId = null,
        string creditMemoNumber = "CR-TEST-001")
    {
        return new CreditMemo(
            customerId ?? Guid.NewGuid(),
            creditMemoNumber,
            DateTime.UtcNow.Date,
            "Test Credit Reason",
            totalAmount,
            "USD",
            "Test credit memo notes",
            null, // OriginalCustomerInvoiceId
            status
        );
    }

    [Fact]
    public void Constructor_Should_InitializeCreditMemoCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var number = "CR2024-001";
        var date = DateTime.UtcNow.Date.AddDays(-2);
        var reason = "Return of goods";
        var amount = 250.75m;
        var currency = "CAD";
        var notes = "Credit for damaged items";
        var originalInvoiceId = Guid.NewGuid();
        var status = CreditMemoStatus.Draft;

        // Act
        var creditMemo = new CreditMemo(customerId, number, date, reason, amount, currency, notes, originalInvoiceId, status);

        // Assert
        creditMemo.Id.Should().NotBe(Guid.Empty);
        creditMemo.CustomerId.Should().Be(customerId);
        creditMemo.CreditMemoNumber.Should().Be(number);
        creditMemo.Date.Should().Be(date);
        creditMemo.Reason.Should().Be(reason);
        creditMemo.TotalAmount.Should().Be(amount);
        creditMemo.Currency.Should().Be(currency);
        creditMemo.Notes.Should().Be(notes);
        creditMemo.OriginalCustomerInvoiceId.Should().Be(originalInvoiceId);
        creditMemo.Status.Should().Be(status);
        creditMemo.Applications.Should().BeEmpty();
        creditMemo.GetAppliedAmount().Should().Be(0);
        creditMemo.GetAvailableBalance().Should().Be(amount);
    }

    [Fact]
    public void Constructor_WithZeroOrNegativeTotalAmount_Should_ThrowArgumentOutOfRangeException()
    {
        Action actZero = () => new CreditMemo(Guid.NewGuid(), "CR-ZERO", DateTime.UtcNow, "Test", 0, "USD");
        actZero.Should().Throw<ArgumentOutOfRangeException>();

        Action actNegative = () => new CreditMemo(Guid.NewGuid(), "CR-NEG", DateTime.UtcNow, "Test", -100, "USD");
        actNegative.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddApplication_Should_AddApplicationAndDecreaseAvailableBalance_AndUpdateStatus()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(totalAmount: 500m, status: CreditMemoStatus.Approved);
        var invoiceId = Guid.NewGuid();
        var amountToApply = 200m;

        // Act
        // Domain method: AddApplication(Guid customerInvoiceId, decimal amountToApply)
        creditMemo.AddApplication(invoiceId, amountToApply);

        // Assert
        creditMemo.Applications.Should().HaveCount(1);
        var app = creditMemo.Applications.First();
        app.CustomerInvoiceId.Should().Be(invoiceId);
        app.AmountApplied.Should().Be(amountToApply);
        creditMemo.GetAppliedAmount().Should().Be(200m);
        creditMemo.GetAvailableBalance().Should().Be(300m);
        creditMemo.Status.Should().Be(CreditMemoStatus.PartiallyApplied);
    }

    [Fact]
    public void AddApplication_FullApplication_Should_SetStatusToApplied()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(totalAmount: 200m, status: CreditMemoStatus.Approved);

        // Act
        creditMemo.AddApplication(Guid.NewGuid(), 200m);

        // Assert
        creditMemo.GetAvailableBalance().Should().Be(0);
        creditMemo.Status.Should().Be(CreditMemoStatus.Applied);
    }

    [Fact]
    public void AddApplication_ExceedingAvailableBalance_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(totalAmount: 100m, status: CreditMemoStatus.Approved);
        creditMemo.AddApplication(Guid.NewGuid(), 50m); // 50 available

        // Act
        Action act = () => creditMemo.AddApplication(Guid.NewGuid(), 60m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Amount to apply exceeds available credit balance.");
    }

    [Fact]
    public void RemoveApplication_ExistingApplication_Should_RemoveAndIncreaseAvailableBalance_AndUpdateStatus()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(totalAmount: 500m, status: CreditMemoStatus.Approved);
        var invoiceId1 = Guid.NewGuid();
        creditMemo.AddApplication(invoiceId1, 200m); // Status becomes PartiallyApplied
        var appToRemoveId = creditMemo.Applications.First(a => a.CustomerInvoiceId == invoiceId1).Id;
        creditMemo.AddApplication(Guid.NewGuid(), 100m); // Total applied 300, balance 200. Status PartiallyApplied.

        // Act
        creditMemo.RemoveApplication(appToRemoveId);

        // Assert
        creditMemo.Applications.Should().HaveCount(1);
        creditMemo.GetAppliedAmount().Should().Be(100m);
        creditMemo.GetAvailableBalance().Should().Be(400m);
        creditMemo.Status.Should().Be(CreditMemoStatus.PartiallyApplied); // Still partially applied as 100m is applied out of 500m
    }

    [Fact]
    public void RemoveApplication_LastApplication_Should_RevertStatusToApproved()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(totalAmount: 200m, status: CreditMemoStatus.Approved);
        creditMemo.AddApplication(Guid.NewGuid(), 200m); // Status becomes Applied
        var appToRemoveId = creditMemo.Applications.First().Id;

        // Act
        creditMemo.RemoveApplication(appToRemoveId);

        // Assert
        creditMemo.Applications.Should().BeEmpty();
        creditMemo.GetAppliedAmount().Should().Be(0);
        creditMemo.GetAvailableBalance().Should().Be(200m);
        creditMemo.Status.Should().Be(CreditMemoStatus.Approved); // Reverted
    }


    [Fact]
    public void Approve_DraftCreditMemo_Should_ChangeStatusToApproved()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(status: CreditMemoStatus.Draft);

        // Act
        creditMemo.Approve();

        // Assert
        creditMemo.Status.Should().Be(CreditMemoStatus.Approved);
    }

    [Fact]
    public void Approve_NonDraftCreditMemo_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(status: CreditMemoStatus.Approved);

        // Act
        Action act = () => creditMemo.Approve();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Void_ApprovedCreditMemo_Should_ChangeStatusToVoid()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(status: CreditMemoStatus.Approved);

        // Act
        creditMemo.Void();

        // Assert
        creditMemo.Status.Should().Be(CreditMemoStatus.Void);
    }

    [Fact]
    public void Void_AppliedCreditMemo_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(status: CreditMemoStatus.Approved, totalAmount: 100m);
        creditMemo.AddApplication(Guid.NewGuid(), 50m); // PartiallyApplied

        // Act
        Action act = () => creditMemo.Void();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot void a credit memo that has been partially or fully applied. Unapply first or handle applied amounts.");
    }

    [Fact]
    public void Update_Should_ModifyEditableProperties_WhenDraft()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(status: CreditMemoStatus.Draft);
        var newDate = DateTime.UtcNow.AddDays(1).Date;
        var newReason = "Updated Reason";
        var newTotalAmount = 600m; // Modifying total amount for a draft CM

        // Act
        creditMemo.Update(newDate, newReason, newTotalAmount, "CAD", "New Notes", Guid.NewGuid());

        // Assert
        creditMemo.Date.Should().Be(newDate);
        creditMemo.Reason.Should().Be(newReason);
        creditMemo.TotalAmount.Should().Be(newTotalAmount);
        creditMemo.Currency.Should().Be("CAD");
        creditMemo.Notes.Should().Be("New Notes");
        creditMemo.OriginalCustomerInvoiceId.Should().NotBeNull();
    }

    [Fact]
    public void Update_TotalAmount_WhenApplied_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var creditMemo = CreateTestCreditMemo(status: CreditMemoStatus.Approved, totalAmount: 100m);
        creditMemo.AddApplication(Guid.NewGuid(), 50m); // PartiallyApplied

        // Act
        Action act = () => creditMemo.Update(null, null, 200m, null, null, null);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot change total amount after credit memo has been applied.");
    }
}
