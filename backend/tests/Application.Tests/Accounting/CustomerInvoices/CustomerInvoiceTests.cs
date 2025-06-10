using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For CustomerInvoice, CustomerInvoiceItem, CustomerInvoiceStatus
using System;
using System.Linq;

namespace Application.Tests.Accounting.CustomerInvoices;

public class CustomerInvoiceTests
{
    // Helper to create a CustomerInvoice for tests
    private static CustomerInvoice CreateDefaultCustomerInvoice(
        Guid? customerId = null,
        string invoiceNumber = "INV-CUST-001",
        decimal initialTotalAmount = 0m, // Usually calculated from items
        CustomerInvoiceStatus initialStatus = CustomerInvoiceStatus.Draft)
    {
        // The CustomerInvoice constructor requires items to calculate TotalAmount.
        // Or, if we are testing methods that add items, start with 0.
        // Let's assume for most tests, items will be added, so TotalAmount starts at 0.
        // The constructor: CustomerId, InvoiceNumber, InvoiceDate, DueDate, Currency, Notes, OrderId, Status
        var invoice = new CustomerInvoice(
            customerId ?? Guid.NewGuid(),
            invoiceNumber,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(30),
            "USD",
            "Test notes",
            null, // OrderId
            initialStatus
        );

        // If an initial total amount is provided for specific test scenarios (e.g. testing ApplyPayment without adding items first)
        // we might need a way to set it, or add a dummy item.
        // For now, methods like AddInvoiceItem will set the TotalAmount.
        // If initialTotalAmount > 0 and no items, this might be inconsistent with domain logic.
        // Let's stick to adding items to set TotalAmount.

        return invoice;
    }

    // Helper to create CustomerInvoiceItem for adding to CustomerInvoice
    // The CustomerInvoice.AddInvoiceItem method takes individual properties.
    // CustomerInvoiceItem constructor: CustomerInvoiceId, Description, Quantity, UnitPrice, TaxAmount, ProductId

    [Fact]
    public void Constructor_Should_InitializeCustomerInvoiceCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var invoiceNumber = "CUST-INV-2024-100";
        var invoiceDate = DateTime.UtcNow.Date.AddDays(-5);
        var dueDate = invoiceDate.AddDays(15);
        var currency = "EUR";
        var notes = "Constructor test";
        var orderId = Guid.NewGuid();
        var status = CustomerInvoiceStatus.Sent;

        // Act
        var invoice = new CustomerInvoice(customerId, invoiceNumber, invoiceDate, dueDate, currency, notes, orderId, status);

        // Assert
        invoice.Id.Should().NotBe(Guid.Empty);
        invoice.CustomerId.Should().Be(customerId);
        invoice.InvoiceNumber.Should().Be(invoiceNumber);
        invoice.InvoiceDate.Should().Be(invoiceDate);
        invoice.DueDate.Should().Be(dueDate);
        invoice.Currency.Should().Be(currency);
        invoice.Notes.Should().Be(notes);
        invoice.OrderId.Should().Be(orderId);
        invoice.Status.Should().Be(status);
        invoice.TotalAmount.Should().Be(0); // No items added yet
        invoice.AmountPaid.Should().Be(0);
        invoice.InvoiceItems.Should().BeEmpty();
    }

    [Fact]
    public void AddInvoiceItem_Should_AddItemAndRecalculateTotalAmount()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice(); // TotalAmount = 0 initially

        // Act
        // AddInvoiceItem(string description, decimal quantity, decimal unitPrice, decimal taxAmount, Guid? productId = null)
        invoice.AddInvoiceItem("Item A", 1, 100m, 10m); // Line total = 100, Tax = 10. Invoice Total = 110

        // Assert
        invoice.InvoiceItems.Should().HaveCount(1);
        var item = invoice.InvoiceItems.First();
        item.Description.Should().Be("Item A");
        item.Quantity.Should().Be(1);
        item.UnitPrice.Should().Be(100m);
        item.TaxAmount.Should().Be(10m);
        item.TotalAmount.Should().Be(100m); // Line total (Qty * Price)
        // Invoice.TotalAmount is sum of (Item.TotalAmount + Item.TaxAmount)
        invoice.TotalAmount.Should().Be(110m);
    }

    [Fact]
    public void AddInvoiceItem_MultipleItems_Should_SumCorrectlyToTotalAmount()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice();

        // Act
        invoice.AddInvoiceItem("Item A", 1, 100m, 10m); // Line 100, Tax 10. Subtotal 110.
        invoice.AddInvoiceItem("Item B", 2, 50m, 5m);   // Line 100 (2*50), Tax 10 (2*5). Subtotal 110.
                                                       // Total invoice amount = 110 + 110 = 220

        // Assert
        invoice.InvoiceItems.Should().HaveCount(2);
        invoice.TotalAmount.Should().Be(220m);
    }

    [Fact]
    public void RemoveInvoiceItem_ExistingItem_Should_RemoveItemAndRecalculateTotal()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice();
        invoice.AddInvoiceItem("Item A", 1, 100m, 10m); // Total 110
        invoice.AddInvoiceItem("Item B", 2, 50m, 5m);   // Total 110
        var itemToRemoveId = invoice.InvoiceItems.First(i => i.Description == "Item A").Id;
        // Initial TotalAmount = 220m

        // Act
        invoice.RemoveInvoiceItem(itemToRemoveId);

        // Assert
        invoice.InvoiceItems.Should().HaveCount(1);
        invoice.InvoiceItems.Any(i => i.Description == "Item A").Should().BeFalse();
        invoice.TotalAmount.Should().Be(110m); // Remaining item B: (2*50) + (2*5) = 100 + 10 = 110
    }

    [Fact]
    public void ClearInvoiceItems_Should_RemoveAllItemsAndZeroTotalAmount()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice();
        invoice.AddInvoiceItem("Item A", 1, 100m, 10m);
        invoice.AddInvoiceItem("Item B", 2, 50m, 5m);

        // Act
        invoice.ClearInvoiceItems();

        // Assert
        invoice.InvoiceItems.Should().BeEmpty();
        invoice.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void ApplyPayment_PartialPayment_Should_UpdateAmountPaidAndStatusToPartiallyPaid()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice();
        invoice.AddInvoiceItem("Test Item", 1, 200m, 0m); // TotalAmount = 200
        var paymentAmount = 50m;

        // Act
        invoice.ApplyPayment(paymentAmount);

        // Assert
        invoice.AmountPaid.Should().Be(paymentAmount);
        invoice.Status.Should().Be(CustomerInvoiceStatus.PartiallyPaid);
        invoice.GetBalanceDue().Should().Be(150m);
    }

    [Fact]
    public void ApplyPayment_FullPayment_Should_UpdateAmountPaidAndStatusToPaid()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice();
        invoice.AddInvoiceItem("Test Item", 1, 200m, 20m); // TotalAmount = 220
        var paymentAmount = 220m;

        // Act
        invoice.ApplyPayment(paymentAmount);

        // Assert
        invoice.AmountPaid.Should().Be(paymentAmount);
        invoice.Status.Should().Be(CustomerInvoiceStatus.Paid);
        invoice.GetBalanceDue().Should().Be(0);
    }

    [Fact]
    public void ApplyPayment_OverPayment_Should_SetAmountPaidToTotalAndStatusToPaid()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice();
        invoice.AddInvoiceItem("Test Item", 1, 200m, 0m); // TotalAmount = 200
        var paymentAmount = 250m; // Overpayment

        // Act
        invoice.ApplyPayment(paymentAmount); // Current domain logic caps AmountPaid at TotalAmount

        // Assert
        invoice.AmountPaid.Should().Be(200m); // Capped at TotalAmount
        invoice.Status.Should().Be(CustomerInvoiceStatus.Paid);
        invoice.GetBalanceDue().Should().Be(0);
        // Note: The domain entity does not store/return overpayment amount.
        // This behavior (capping AmountPaid) is based on typical invoice logic.
    }


    [Fact]
    public void ApplyPayment_MultiplePartialPayments_Should_AccumulateAndSetStatusCorrectly()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice();
        invoice.AddInvoiceItem("Test Item", 1, 200m, 0m); // TotalAmount = 200

        // Act
        invoice.ApplyPayment(50m);

        // Assert
        invoice.AmountPaid.Should().Be(50m);
        invoice.Status.Should().Be(CustomerInvoiceStatus.PartiallyPaid);
        invoice.GetBalanceDue().Should().Be(150m);

        // Act
        invoice.ApplyPayment(150m);

        // Assert
        invoice.AmountPaid.Should().Be(200m);
        invoice.Status.Should().Be(CustomerInvoiceStatus.Paid);
        invoice.GetBalanceDue().Should().Be(0);
    }

    [Fact]
    public void ApplyPayment_ZeroOrNegativeAmount_Should_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice();
        invoice.AddInvoiceItem("Test Item", 1, 200m, 0m);

        // Act & Assert
        Action actZero = () => invoice.ApplyPayment(0);
        actZero.Should().Throw<ArgumentOutOfRangeException>();

        Action actNegative = () => invoice.ApplyPayment(-50m);
        actNegative.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetBalanceDue_Should_ReturnCorrectBalance()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice();
        invoice.AddInvoiceItem("Item 1", 1, 100m, 10m); // Total 110
        invoice.AddInvoiceItem("Item 2", 1, 40m, 0m);  // Total 40. Grand Total = 150.
        invoice.ApplyPayment(50m);

        // Act
        var balance = invoice.GetBalanceDue();

        // Assert
        balance.Should().Be(100m); // 150 (Total) - 50 (Paid)
    }

    [Fact]
    public void UpdateStatus_Should_ChangeStatus_WhenCalledDirectly()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice(initialStatus: CustomerInvoiceStatus.Draft);

        // Act
        invoice.UpdateStatus(CustomerInvoiceStatus.Sent);

        // Assert
        invoice.Status.Should().Be(CustomerInvoiceStatus.Sent);
    }

    [Fact]
    public void Update_Should_ModifyEditableInvoiceProperties()
    {
        // Arrange
        var invoice = CreateDefaultCustomerInvoice();
        var originalInvoiceNumber = invoice.InvoiceNumber;

        var newInvoiceDate = DateTime.UtcNow.AddDays(-2).Date;
        var newDueDate = DateTime.UtcNow.AddDays(25).Date;
        var newCurrency = "GBP";
        var newNotes = "Updated invoice general notes";
        var newStatus = CustomerInvoiceStatus.Sent; // Example direct status update

        // Act
        invoice.Update(
            invoiceDate: newInvoiceDate,
            dueDate: newDueDate,
            currency: newCurrency,
            notes: newNotes,
            status: newStatus
        );

        // Assert
        invoice.InvoiceDate.Should().Be(newInvoiceDate);
        invoice.DueDate.Should().Be(newDueDate);
        invoice.Currency.Should().Be(newCurrency);
        invoice.Notes.Should().Be(newNotes);
        invoice.Status.Should().Be(newStatus);
        invoice.InvoiceNumber.Should().Be(originalInvoiceNumber); // InvoiceNumber is not updatable via Update
    }
}
