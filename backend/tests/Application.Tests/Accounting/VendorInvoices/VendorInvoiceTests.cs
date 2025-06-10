using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For VendorInvoice, VendorInvoiceItem, VendorInvoiceStatus
using System;
using System.Linq; // For LINQ operations like .First()

namespace Application.Tests.Accounting.VendorInvoices;

public class VendorInvoiceTests
{
    // Helper to create a VendorInvoice with some default valid parameters for tests
    private static VendorInvoice CreateDefaultVendorInvoice(Guid? supplierId = null)
    {
        return new VendorInvoice(
            supplierId ?? Guid.NewGuid(),
            "INV-TEST-001",
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(30),
            0, // TotalAmount will be calculated by items
            "USD",
            VendorInvoiceStatus.Draft,
            "Test notes"
        );
    }

    // Helper to create a VendorInvoiceItem
    // The actual VendorInvoiceItem constructor takes VendorInvoiceId, Description, Quantity, UnitPrice, TaxAmount, ProductId
    // For simplicity in tests, we might add items via VendorInvoice methods directly if they take these params.
    // The VendorInvoice.AddInvoiceItem method in the domain entity was:
    // AddInvoiceItem(VendorInvoiceItem item) OR AddInvoiceItem(string description, decimal quantity, decimal unitPrice, decimal taxAmount, Guid? productId = null)
    // Let's assume the latter for easier testing setup, if not, this helper needs to change or we construct VendorInvoiceItem directly.
    // The existing domain entity VendorInvoice.cs has `AddInvoiceItem(VendorInvoiceItem item)`
    // and VendorInvoiceItem constructor is `VendorInvoiceItem(Guid vendorInvoiceId, string description, decimal quantity, decimal unitPrice, decimal taxAmount, Guid? productId = null)`

    [Fact]
    public void Constructor_Should_InitializeVendorInvoiceCorrectly()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var invoiceNumber = "INV2024-001";
        var invoiceDate = DateTime.UtcNow.Date;
        var dueDate = invoiceDate.AddDays(30);
        var currency = "EUR";
        var notes = "Initial notes";

        // Act
        var invoice = new VendorInvoice(supplierId, invoiceNumber, invoiceDate, dueDate, 0, currency, VendorInvoiceStatus.Draft, notes);

        // Assert
        invoice.Id.Should().NotBe(Guid.Empty);
        invoice.SupplierId.Should().Be(supplierId);
        invoice.InvoiceNumber.Should().Be(invoiceNumber);
        invoice.InvoiceDate.Should().Be(invoiceDate);
        invoice.DueDate.Should().Be(dueDate);
        invoice.Currency.Should().Be(currency);
        invoice.Notes.Should().Be(notes);
        invoice.Status.Should().Be(VendorInvoiceStatus.Draft);
        invoice.TotalAmount.Should().Be(0); // Initial total amount before items
        invoice.InvoiceItems.Should().BeEmpty();
        // AuditableEntity properties like CreatedOn should be set, but that's usually handled by base class/EF, not tested here directly.
    }

    [Fact]
    public void AddInvoiceItem_Should_AddItemToInvoiceAndRecalculateTotal()
    {
        // Arrange
        var invoice = CreateDefaultVendorInvoice();
        var item1 = new VendorInvoiceItem(invoice.Id, "Item 1", 1, 100m, 10m, Guid.NewGuid()); // 100 + 10 tax

        // Act
        invoice.AddInvoiceItem(item1);

        // Assert
        invoice.InvoiceItems.Should().HaveCount(1);
        invoice.InvoiceItems.Should().Contain(item1);
        // Assuming VendorInvoice.RecalculateTotalAmount sums item.TotalAmount (which is Qty*Price) + item.TaxAmount
        // If VendorInvoiceItem.TotalAmount itself includes tax, then just sum TotalAmount.
        // The current VendorInvoiceItem.TotalAmount is Qty*Price.
        // The current VendorInvoice.RecalculateTotalAmount sums item.TotalAmount + item.TaxAmount.
        // So, for item1: (1*100) + 10 = 110
        invoice.TotalAmount.Should().Be(110m);
    }

    [Fact]
    public void AddInvoiceItem_MultipleItems_Should_CorrectlySumTotalAmount()
    {
        // Arrange
        var invoice = CreateDefaultVendorInvoice();
        var item1 = new VendorInvoiceItem(invoice.Id, "Item 1", 1, 100m, 10m); // Total 110
        var item2 = new VendorInvoiceItem(invoice.Id, "Item 2", 2, 50m, 5m);  // Total 100 + 10 (2*5) = 110

        // Act
        invoice.AddInvoiceItem(item1);
        invoice.AddInvoiceItem(item2);

        // Assert
        invoice.InvoiceItems.Should().HaveCount(2);
        invoice.TotalAmount.Should().Be(220m); // 110 + 110
    }


    [Fact]
    public void RemoveInvoiceItem_ExistingItem_Should_RemoveItemAndRecalculateTotal()
    {
        // Arrange
        var invoice = CreateDefaultVendorInvoice();
        var item1 = new VendorInvoiceItem(invoice.Id, "Item 1", 1, 100m, 10m); // Total 110
        var item2 = new VendorInvoiceItem(invoice.Id, "Item 2", 2, 50m, 5m);  // Total 110
        invoice.AddInvoiceItem(item1);
        invoice.AddInvoiceItem(item2);
        var initialTotal = invoice.TotalAmount; // Should be 220m

        // Act
        invoice.RemoveInvoiceItem(item1.Id);

        // Assert
        invoice.InvoiceItems.Should().HaveCount(1);
        invoice.InvoiceItems.Should().NotContain(item1);
        invoice.InvoiceItems.Should().Contain(item2);
        invoice.TotalAmount.Should().Be(initialTotal - (item1.TotalAmount + item1.TaxAmount)); // 220 - 110 = 110
    }

    [Fact]
    public void RemoveInvoiceItem_NonExistingItem_Should_NotChangeInvoice()
    {
        // Arrange
        var invoice = CreateDefaultVendorInvoice();
        var item1 = new VendorInvoiceItem(invoice.Id, "Item 1", 1, 100m, 10m);
        invoice.AddInvoiceItem(item1);
        var initialItemCount = invoice.InvoiceItems.Count;
        var initialTotalAmount = invoice.TotalAmount;

        // Act
        invoice.RemoveInvoiceItem(Guid.NewGuid()); // Non-existing item ID

        // Assert
        invoice.InvoiceItems.Should().HaveCount(initialItemCount);
        invoice.TotalAmount.Should().Be(initialTotalAmount);
    }

    [Fact]
    public void ClearInvoiceItems_Should_RemoveAllItemsAndSetTotalToZero()
    {
        // Arrange
        var invoice = CreateDefaultVendorInvoice();
        var item1 = new VendorInvoiceItem(invoice.Id, "Item 1", 1, 100m, 10m);
        var item2 = new VendorInvoiceItem(invoice.Id, "Item 2", 2, 50m, 5m);
        invoice.AddInvoiceItem(item1);
        invoice.AddInvoiceItem(item2);

        // Act
        invoice.ClearInvoiceItems();

        // Assert
        invoice.InvoiceItems.Should().BeEmpty();
        invoice.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void UpdateStatus_Should_ChangeStatusCorrectly()
    {
        // Arrange
        var invoice = CreateDefaultVendorInvoice(); // Initial status is Draft

        // Act
        invoice.UpdateStatus(VendorInvoiceStatus.Submitted);

        // Assert
        invoice.Status.Should().Be(VendorInvoiceStatus.Submitted);

        // Act
        invoice.UpdateStatus(VendorInvoiceStatus.Approved);

        // Assert
        invoice.Status.Should().Be(VendorInvoiceStatus.Approved);
    }

    // Test RecalculateTotalAmount implicitly via Add/Remove/Clear item tests.
    // If it were public and complex, it could have its own tests.
    // For example, if RecalculateTotalAmount was public:
    // [Fact]
    // public void RecalculateTotalAmount_NoItems_Should_BeZero() { ... }

    [Fact]
    public void Update_Should_ModifyEditableProperties()
    {
        // Arrange
        var invoice = CreateDefaultVendorInvoice();
        var originalInvoiceNumber = invoice.InvoiceNumber; // Assuming InvoiceNumber is not updatable via Update method

        var newInvoiceDate = DateTime.UtcNow.AddDays(-1).Date;
        var newDueDate = DateTime.UtcNow.AddDays(20).Date;
        var newCurrency = "GBP";
        var newNotes = "Updated notes";
        var newStatus = VendorInvoiceStatus.Approved; // Assuming status can be updated this way, though specific methods are better

        // Act
        invoice.Update(
            invoiceNumber: null, // Not updating invoice number
            invoiceDate: newInvoiceDate,
            dueDate: newDueDate,
            totalAmount: null, // Total amount is driven by items
            currency: newCurrency,
            status: newStatus,
            notes: newNotes
        );

        // Assert
        invoice.InvoiceDate.Should().Be(newInvoiceDate);
        invoice.DueDate.Should().Be(newDueDate);
        invoice.Currency.Should().Be(newCurrency);
        invoice.Notes.Should().Be(newNotes);
        invoice.Status.Should().Be(newStatus);
        invoice.InvoiceNumber.Should().Be(originalInvoiceNumber); // Unchanged
    }
}
