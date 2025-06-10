using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.Suppliers;
using System;

namespace Application.Tests.Accounting.Suppliers;

public class UpdateSupplierRequestValidatorTests
{
    private readonly UpdateSupplierRequestValidator _validator = new();

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid_With_All_Fields()
    {
        // Arrange
        var request = new UpdateSupplierRequest
        {
            Id = Guid.NewGuid(),
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
    public void Should_Not_Have_Error_When_Request_Is_Valid_With_Minimal_Fields()
    {
        // Arrange
        var request = new UpdateSupplierRequest
        {
            Id = Guid.NewGuid(), // Id is required
            Name = "Only Name Updated" // Only name is provided for update
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        // Arrange
        var request = new UpdateSupplierRequest { Id = Guid.Empty, Name = "Valid Name" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("'Id' must not be empty."); // Default from NotEmptyGuid()
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty_If_Provided()
    {
        // Arrange
        var request = new UpdateSupplierRequest { Id = Guid.NewGuid(), Name = string.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name); // Default NotEmpty message
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength_If_Provided()
    {
        // Arrange
        var request = new UpdateSupplierRequest { Id = Guid.NewGuid(), Name = new string('A', 257) };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Supplier Name must not exceed 256 characters.");
    }


    [Fact]
    public void Should_Not_Have_Error_For_Name_When_Name_Is_Null_And_Not_Provided()
    {
        // Arrange
        var request = new UpdateSupplierRequest { Id = Guid.NewGuid(), ContactInfo = "Some Contact" }; // Name is null

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }


    // Similar tests can be added for ContactInfo, Address, TaxId, BankDetails max length checks when provided.
    // Example for ContactInfo:
    [Fact]
    public void Should_Have_Error_When_ContactInfo_Exceeds_MaxLength_If_Provided()
    {
        // Arrange
        var request = new UpdateSupplierRequest { Id = Guid.NewGuid(), ContactInfo = new string('A', 257) };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContactInfo)
            .WithErrorMessage("Contact Information must not exceed 256 characters.");
    }

    // Test for DefaultPaymentTermId (NotEmptyGuid when HasValue)
    // The current UpdateSupplierRequestValidator doesn't have NotEmptyGuid for DefaultPaymentTermId.
    // It only checks NotEmptyGuid for Id.
    // If DefaultPaymentTermId had a .NotEmptyGuid().When(p => p.DefaultPaymentTermId.HasValue) rule:
    // [Fact]
    // public void Should_Have_Error_When_DefaultPaymentTermId_Is_EmptyGuid_If_Provided()
    // {
    //     // Arrange
    //     var request = new UpdateSupplierRequest { Id = Guid.NewGuid(), DefaultPaymentTermId = Guid.Empty };
    //
    //     // Act
    //     var result = _validator.TestValidate(request);
    //
    //     // Assert
    //     result.ShouldHaveValidationErrorFor(x => x.DefaultPaymentTermId)
    //         .WithErrorMessage("'Default Payment Term Id' must not be empty.");
    // }

    [Fact]
    public void Should_Not_Have_Error_When_DefaultPaymentTermId_Is_Null()
    {
        // Arrange
        var request = new UpdateSupplierRequest { Id = Guid.NewGuid(), DefaultPaymentTermId = null };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DefaultPaymentTermId);
    }
}
