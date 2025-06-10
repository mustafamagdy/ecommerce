using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For CustomerPayment, CustomerPaymentApplication
using System;
using System.Linq;

namespace Application.Tests.Accounting.CustomerPayments;

public class CustomerPaymentTests
{
    private CustomerPayment CreateTestCustomerPayment(
        decimal amountReceived = 1000m,
        Guid? customerId = null,
        Guid? paymentMethodId = null)
    {
        return new CustomerPayment(
            customerId ?? Guid.NewGuid(),
            DateTime.UtcNow.Date,
            amountReceived,
            paymentMethodId ?? Guid.NewGuid(),
            "REFCP001",
            "Test customer payment"
        );
    }

    // CustomerPaymentApplication constructor: CustomerPaymentId, CustomerInvoiceId, AmountApplied
    private CustomerPaymentApplication CreateTestPaymentApplication(Guid paymentId, Guid invoiceId, decimal amountApplied)
    {
        return new CustomerPaymentApplication(paymentId, invoiceId, amountApplied);
    }

    [Fact]
    public void Constructor_Should_InitializeCustomerPaymentCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var paymentDate = DateTime.UtcNow.Date.AddDays(-1);
        var amountReceived = 500.50m;
        var paymentMethodId = Guid.NewGuid();
        var reference = "CPAY-001";
        var notes = "Payment for services";

        // Act
        var payment = new CustomerPayment(customerId, paymentDate, amountReceived, paymentMethodId, reference, notes);

        // Assert
        payment.Id.Should().NotBe(Guid.Empty);
        payment.CustomerId.Should().Be(customerId);
        payment.PaymentDate.Should().Be(paymentDate);
        payment.AmountReceived.Should().Be(amountReceived);
        payment.PaymentMethodId.Should().Be(paymentMethodId);
        payment.ReferenceNumber.Should().Be(reference);
        payment.Notes.Should().Be(notes);
        payment.AppliedInvoices.Should().BeEmpty();
        payment.GetUnappliedAmount().Should().Be(amountReceived);
    }

    [Fact]
    public void AddPaymentApplication_Should_AddApplicationAndDecreaseUnappliedAmount()
    {
        // Arrange
        var payment = CreateTestCustomerPayment(amountReceived: 500m);
        var invoiceId = Guid.NewGuid();
        var amountToApply = 200m;
        // The domain entity CustomerPayment.AddPaymentApplication takes (Guid customerInvoiceId, decimal amountToApply)
        // and creates the CustomerPaymentApplication internally.

        // Act
        payment.AddPaymentApplication(invoiceId, amountToApply);

        // Assert
        payment.AppliedInvoices.Should().HaveCount(1);
        var app = payment.AppliedInvoices.First();
        app.CustomerInvoiceId.Should().Be(invoiceId);
        app.AmountApplied.Should().Be(amountToApply);
        payment.GetUnappliedAmount().Should().Be(500m - 200m);
    }

    [Fact]
    public void AddPaymentApplication_MultipleApplications_Should_SumCorrectly()
    {
        // Arrange
        var payment = CreateTestCustomerPayment(amountReceived: 1000m);

        // Act
        payment.AddPaymentApplication(Guid.NewGuid(), 300m);
        payment.AddPaymentApplication(Guid.NewGuid(), 400m);

        // Assert
        payment.AppliedInvoices.Should().HaveCount(2);
        payment.GetUnappliedAmount().Should().Be(1000m - 300m - 400m);
    }

    [Fact]
    public void AddPaymentApplication_ApplyingMoreThanAmountReceived_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var payment = CreateTestCustomerPayment(amountReceived: 100m);
        payment.AddPaymentApplication(Guid.NewGuid(), 50m); // 50 unapplied

        // Act
        Action act = () => payment.AddPaymentApplication(Guid.NewGuid(), 60m); // Trying to apply 60, only 50 left

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot apply more than the total amount received for the payment.");
    }

    [Fact]
    public void AddPaymentApplication_ApplyingZeroOrNegativeAmount_Should_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        var payment = CreateTestCustomerPayment(amountReceived: 100m);

        // Act
        Action actZero = () => payment.AddPaymentApplication(Guid.NewGuid(), 0);
        Action actNegative = () => payment.AddPaymentApplication(Guid.NewGuid(), -10m);

        // Assert
        actZero.Should().Throw<ArgumentOutOfRangeException>();
        actNegative.Should().Throw<ArgumentOutOfRangeException>();
    }


    [Fact]
    public void RemovePaymentApplication_ExistingApplication_Should_RemoveAndIncreaseUnappliedAmount()
    {
        // Arrange
        var payment = CreateTestCustomerPayment(amountReceived: 500m);
        var invoiceId1 = Guid.NewGuid();
        payment.AddPaymentApplication(invoiceId1, 200m);
        var appToRemoveId = payment.AppliedInvoices.First(a => a.CustomerInvoiceId == invoiceId1).Id;
        payment.AddPaymentApplication(Guid.NewGuid(), 100m); // Another application
        // Unapplied amount is 500 - 200 - 100 = 200

        // Act
        payment.RemovePaymentApplication(appToRemoveId);

        // Assert
        payment.AppliedInvoices.Should().HaveCount(1);
        payment.AppliedInvoices.Any(a => a.Id == appToRemoveId).Should().BeFalse();
        payment.GetUnappliedAmount().Should().Be(500m - 100m); // 200 (original unapplied) + 200 (from removed app) = 400. Total applied is 100.
    }

    [Fact]
    public void GetUnappliedAmount_NoApplications_Should_EqualAmountReceived()
    {
        // Arrange
        var payment = CreateTestCustomerPayment(amountReceived: 300m);

        // Act
        var unapplied = payment.GetUnappliedAmount();

        // Assert
        unapplied.Should().Be(300m);
    }

    [Fact]
    public void GetUnappliedAmount_FullyApplied_Should_BeZero()
    {
        // Arrange
        var payment = CreateTestCustomerPayment(amountReceived: 300m);
        payment.AddPaymentApplication(Guid.NewGuid(), 100m);
        payment.AddPaymentApplication(Guid.NewGuid(), 200m);

        // Act
        var unapplied = payment.GetUnappliedAmount();

        // Assert
        unapplied.Should().Be(0);
    }

    [Fact]
    public void Update_Should_ModifyEditableProperties()
    {
        // Arrange
        var payment = CreateTestCustomerPayment();
        var newPaymentDate = DateTime.UtcNow.Date.AddDays(-2);
        var newAmountReceived = 1500m; // Assume no applications for simplicity of testing this part
        var newPaymentMethodId = Guid.NewGuid();
        var newRef = "UPD-REF";
        var newNotes = "Updated notes";

        // Act
        payment.Update(newPaymentDate, newAmountReceived, newPaymentMethodId, newRef, newNotes);

        // Assert
        payment.PaymentDate.Should().Be(newPaymentDate);
        payment.AmountReceived.Should().Be(newAmountReceived);
        payment.PaymentMethodId.Should().Be(newPaymentMethodId);
        payment.ReferenceNumber.Should().Be(newRef);
        payment.Notes.Should().Be(newNotes);
    }

    [Fact]
    public void Update_AmountReceived_LessThanApplied_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var payment = CreateTestCustomerPayment(amountReceived: 200m);
        payment.AddPaymentApplication(Guid.NewGuid(), 100m); // 100 applied
        payment.AddPaymentApplication(Guid.NewGuid(), 50m);  // Total 150 applied

        // Act
        Action act = () => payment.Update(null, 100m, null, null, null); // New amount less than 150

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("New amount received cannot be less than the total amount already applied to invoices.");
    }
}
