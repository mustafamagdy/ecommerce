using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.Suppliers;
using System;

namespace Application.Tests.Accounting.Suppliers;

public class CreateSupplierRequestValidatorTests
{
    private readonly CreateSupplierRequestValidator _validator = new();

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        // Arrange
        var request = new CreateSupplierRequest
        {
            Name = "Valid Supplier Name",
            ContactInfo = "contact@supplier.com",
            Address = "123 Supplier St",
            TaxId = "TAX123",
            DefaultPaymentTermId = Guid.NewGuid(),
            BankDetails = "Valid Bank Details"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = null! }; // Name is required

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("'Name' must not be empty."); // Default FluentValidation message
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = new string('A', 257) }; // MaxLength is 256

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Supplier Name must not exceed 256 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_ContactInfo_Exceeds_MaxLength()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = "Valid Name", ContactInfo = new string('A', 257) }; // MaxLength 256

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContactInfo)
            .WithErrorMessage("Contact Information must not exceed 256 characters.");
    }


    [Fact]
    public void Should_Have_Error_When_Address_Exceeds_MaxLength()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = "Valid Name", Address = new string('A', 1025) }; // MaxLength 1024

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address)
            .WithErrorMessage("Address must not exceed 1024 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_TaxId_Exceeds_MaxLength()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = "Valid Name", TaxId = new string('A', 51) }; // MaxLength 50

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaxId)
            .WithErrorMessage("Tax ID must not exceed 50 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_BankDetails_Exceeds_MaxLength()
    {
        // Arrange
        var request = new CreateSupplierRequest { Name = "Valid Name", BankDetails = new string('A', 1025) }; // MaxLength 1024

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BankDetails)
            .WithErrorMessage("Bank Details must not exceed 1024 characters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_OptionalFields_Are_Null()
    {
        // Arrange
        var request = new CreateSupplierRequest
        {
            Name = "Minimal Valid Supplier",
            ContactInfo = null,
            Address = null,
            TaxId = null,
            DefaultPaymentTermId = null,
            BankDetails = null
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // The DefaultPaymentTermId does not have specific validation rules in CreateSupplierRequestValidator
    // other than being a Guid?, so no NotEmptyGuid test here unless it was required.
}
