using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For Supplier
using System;

namespace Application.Tests.Accounting.Suppliers;

public class SupplierTests
{
    private Supplier CreateTestSupplier(
        string name = "Test Supplier Inc.",
        string? contactInfo = "test@supplier.com",
        string? address = "123 Test St",
        string? taxId = "TAXID12345",
        Guid? defaultPaymentTermId = null,
        string? bankDetails = "Test Bank - 1234567890")
    {
        return new Supplier(name, contactInfo, address, taxId, defaultPaymentTermId ?? Guid.NewGuid(), bankDetails);
    }

    [Fact]
    public void Constructor_Should_InitializeSupplierCorrectly()
    {
        // Arrange
        var name = "New Supplier Ltd.";
        var contact = "contact@newsupplier.com";
        var address = "Main Road 1";
        var taxId = "ID999";
        var paymentTermId = Guid.NewGuid();
        var bankDetails = "Bank of Tests - Acc 001";

        // Act
        var supplier = new Supplier(name, contact, address, taxId, paymentTermId, bankDetails);

        // Assert
        supplier.Id.Should().NotBe(Guid.Empty);
        supplier.Name.Should().Be(name);
        supplier.ContactInfo.Should().Be(contact);
        supplier.Address.Should().Be(address);
        supplier.TaxId.Should().Be(taxId);
        supplier.DefaultPaymentTermId.Should().Be(paymentTermId);
        supplier.BankDetails.Should().Be(bankDetails);
        // AuditableEntity properties (CreatedOn etc.) are handled by the base class/EF.
    }

    [Fact]
    public void Update_Should_ModifyPropertiesCorrectly()
    {
        // Arrange
        var supplier = CreateTestSupplier();
        var originalId = supplier.Id;

        var newName = "Updated Supplier Co.";
        var newContact = "updated_contact@supplier.com";
        var newAddress = "456 Updated Ave";
        var newTaxId = "NEWTAXID";
        var newPaymentTermId = Guid.NewGuid();
        var newBankDetails = "New Bank - Acc 987";

        // Act
        supplier.Update(newName, newContact, newAddress, newTaxId, newPaymentTermId, newBankDetails);

        // Assert
        supplier.Id.Should().Be(originalId); // ID should not change
        supplier.Name.Should().Be(newName);
        supplier.ContactInfo.Should().Be(newContact);
        supplier.Address.Should().Be(newAddress);
        supplier.TaxId.Should().Be(newTaxId);
        supplier.DefaultPaymentTermId.Should().Be(newPaymentTermId);
        supplier.BankDetails.Should().Be(newBankDetails);
    }

    [Fact]
    public void Update_WithNullValues_Should_UpdatePropertiesToNullWhereAllowed()
    {
        // Arrange
        var paymentTermId = Guid.NewGuid();
        var supplier = CreateTestSupplier(defaultPaymentTermId: paymentTermId); // Start with some non-null values

        // Act
        supplier.Update(
            name: "Supplier With Nulls", // Keep name non-null as it's usually required
            contactInfo: null,
            address: null,
            taxId: null,
            defaultPaymentTermId: null, // Explicitly set to null
            bankDetails: null
        );

        // Assert
        supplier.Name.Should().Be("Supplier With Nulls");
        supplier.ContactInfo.Should().BeNull();
        supplier.Address.Should().BeNull();
        supplier.TaxId.Should().BeNull();
        supplier.DefaultPaymentTermId.Should().BeNull();
        supplier.BankDetails.Should().BeNull();
    }

    [Fact]
    public void Update_OnlySomeProperties_Should_LeaveOthersUnchanged()
    {
        // Arrange
        var originalName = "Original Name";
        var originalContact = "original@contact.com";
        var originalAddress = "Original Address";
        var originalTaxId = "OriginalTaxID";
        var originalPaymentTermId = Guid.NewGuid();
        var originalBankDetails = "Original Bank Details";

        var supplier = new Supplier(originalName, originalContact, originalAddress, originalTaxId, originalPaymentTermId, originalBankDetails);

        var newName = "New Name Only";
        var newBankDetails = "New Bank Details Only";

        // Act
        supplier.Update(newName, null, null, null, null, newBankDetails);

        // Assert
        supplier.Name.Should().Be(newName);
        supplier.ContactInfo.Should().Be(originalContact); // Unchanged
        supplier.Address.Should().Be(originalAddress);     // Unchanged
        supplier.TaxId.Should().Be(originalTaxId);         // Unchanged
        supplier.DefaultPaymentTermId.Should().Be(originalPaymentTermId); // Unchanged
        supplier.BankDetails.Should().Be(newBankDetails);
    }
}
