using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For VendorPayment, VendorPaymentApplication
using System;
using System.Linq;

namespace Application.Tests.Accounting.VendorPayments;

public class VendorPaymentTests
{
    private VendorPayment CreateTestVendorPayment(
        Guid? supplierId = null,
        DateTime? paymentDate = null,
        decimal amountPaid = 1000m,
        Guid? paymentMethodId = null,
        string? referenceNumber = "REF123",
        string? notes = "Test payment notes")
    {
        return new VendorPayment(
            supplierId ?? Guid.NewGuid(),
            paymentDate ?? DateTime.UtcNow.Date,
            amountPaid,
            paymentMethodId ?? Guid.NewGuid(),
            referenceNumber,
            notes
        );
    }

    [Fact]
    public void Constructor_Should_InitializeVendorPaymentCorrectly()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var paymentDate = DateTime.UtcNow.Date.AddDays(-1);
        var amountPaid = 1500.75m;
        var paymentMethodId = Guid.NewGuid();
        var reference = "PO456-PAY";
        var notes = "Payment for PO456";

        // Act
        var payment = new VendorPayment(supplierId, paymentDate, amountPaid, paymentMethodId, reference, notes);

        // Assert
        payment.Id.Should().NotBe(Guid.Empty);
        payment.SupplierId.Should().Be(supplierId);
        payment.PaymentDate.Should().Be(paymentDate);
        payment.AmountPaid.Should().Be(amountPaid);
        payment.PaymentMethodId.Should().Be(paymentMethodId);
        payment.ReferenceNumber.Should().Be(reference);
        payment.Notes.Should().Be(notes);
        payment.AppliedInvoices.Should().BeEmpty();
    }

    [Fact]
    public void AddPaymentApplication_Should_AddApplicationToAppliedInvoices()
    {
        // Arrange
        var payment = CreateTestVendorPayment(amountPaid: 500m);
        // VendorPaymentApplication constructor: VendorPaymentId, VendorInvoiceId, AmountApplied
        // The domain entity VendorPayment.AddPaymentApplication takes VendorPaymentApplication object
        var invoiceId = Guid.NewGuid();
        var amountToApply = 200m;
        var application = new VendorPaymentApplication(payment.Id, invoiceId, amountToApply);


        // Act
        payment.AddPaymentApplication(application);

        // Assert
        payment.AppliedInvoices.Should().HaveCount(1);
        payment.AppliedInvoices.First().Should().Be(application);
        payment.AppliedInvoices.First().VendorInvoiceId.Should().Be(invoiceId);
        payment.AppliedInvoices.First().AmountApplied.Should().Be(amountToApply);
    }

    [Fact]
    public void AddPaymentApplication_MultipleApplications_Should_AddAll()
    {
        // Arrange
        var payment = CreateTestVendorPayment(amountPaid: 1000m);
        var app1 = new VendorPaymentApplication(payment.Id, Guid.NewGuid(), 300m);
        var app2 = new VendorPaymentApplication(payment.Id, Guid.NewGuid(), 400m);

        // Act
        payment.AddPaymentApplication(app1);
        payment.AddPaymentApplication(app2);

        // Assert
        payment.AppliedInvoices.Should().HaveCount(2);
        payment.AppliedInvoices.Should().Contain(app1);
        payment.AppliedInvoices.Should().Contain(app2);
    }

    [Fact]
    public void RemovePaymentApplication_ExistingApplication_Should_RemoveIt()
    {
        // Arrange
        var payment = CreateTestVendorPayment(amountPaid: 500m);
        var app1 = new VendorPaymentApplication(payment.Id, Guid.NewGuid(), 200m);
        var app2 = new VendorPaymentApplication(payment.Id, Guid.NewGuid(), 100m);
        payment.AddPaymentApplication(app1);
        payment.AddPaymentApplication(app2);

        // Act
        payment.RemovePaymentApplication(app1.Id);

        // Assert
        payment.AppliedInvoices.Should().HaveCount(1);
        payment.AppliedInvoices.Should().NotContain(app1);
        payment.AppliedInvoices.Should().Contain(app2);
    }

    [Fact]
    public void RemovePaymentApplication_NonExistingApplication_Should_NotChangeApplications()
    {
        // Arrange
        var payment = CreateTestVendorPayment(amountPaid: 500m);
        var app1 = new VendorPaymentApplication(payment.Id, Guid.NewGuid(), 200m);
        payment.AddPaymentApplication(app1);
        var initialCount = payment.AppliedInvoices.Count;

        // Act
        payment.RemovePaymentApplication(Guid.NewGuid()); // Non-existing ID

        // Assert
        payment.AppliedInvoices.Should().HaveCount(initialCount);
    }

    [Fact]
    public void ClearPaymentApplications_Should_RemoveAllApplications()
    {
        // Arrange
        var payment = CreateTestVendorPayment(amountPaid: 500m);
        var app1 = new VendorPaymentApplication(payment.Id, Guid.NewGuid(), 200m);
        var app2 = new VendorPaymentApplication(payment.Id, Guid.NewGuid(), 100m);
        payment.AddPaymentApplication(app1);
        payment.AddPaymentApplication(app2);

        // Act
        payment.ClearPaymentApplications();

        // Assert
        payment.AppliedInvoices.Should().BeEmpty();
    }


    [Fact]
    public void Update_Should_ModifyEditableProperties()
    {
        // Arrange
        var payment = CreateTestVendorPayment();
        var originalId = payment.Id;
        var originalSupplierId = payment.SupplierId; // SupplierId is not updatable via Update method

        var newPaymentDate = DateTime.UtcNow.Date.AddDays(-5);
        var newAmountPaid = 1200.50m;
        var newPaymentMethodId = Guid.NewGuid();
        var newReference = "UPDATED-REF";
        var newNotes = "Updated payment notes";

        // Act
        payment.Update(newPaymentDate, newAmountPaid, newPaymentMethodId, newReference, newNotes);

        // Assert
        payment.Id.Should().Be(originalId);
        payment.SupplierId.Should().Be(originalSupplierId); // Unchanged
        payment.PaymentDate.Should().Be(newPaymentDate);
        payment.AmountPaid.Should().Be(newAmountPaid);
        payment.PaymentMethodId.Should().Be(newPaymentMethodId);
        payment.ReferenceNumber.Should().Be(newReference);
        payment.Notes.Should().Be(newNotes);
    }

    [Fact]
    public void Update_WithNullValues_Should_UpdatePropertiesToNullWhereAllowed()
    {
        // Arrange
        var payment = CreateTestVendorPayment(referenceNumber: "InitialRef", notes: "InitialNotes");

        // Act
        payment.Update(
            paymentDate: null, // Keep date
            amountPaid: null,  // Keep amount
            paymentMethodId: null, // Keep method
            referenceNumber: null, // Set to null
            notes: null            // Set to null
        );

        // Assert
        payment.ReferenceNumber.Should().BeNull();
        payment.Notes.Should().BeNull();
        // Other properties should remain unchanged from their initial values in CreateTestVendorPayment
        payment.PaymentDate.Should().NotBeNull();
        payment.AmountPaid.Should().Be(1000m); // Default from helper
    }
}
