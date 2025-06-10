using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.CustomerInvoices;
using System;
using System.Collections.Generic;

namespace Application.Tests.Accounting.CustomerInvoices;

public class UpdateCustomerInvoiceRequestValidatorTests
{
    private readonly UpdateCustomerInvoiceRequestValidator _validator = new();

    private UpdateCustomerInvoiceRequest CreateValidRequest(bool includeItems = true)
    {
        var request = new UpdateCustomerInvoiceRequest
        {
            Id = Guid.NewGuid(),
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            Currency = "USD",
            Notes = "Updated notes"
        };

        if (includeItems)
        {
            request.InvoiceItems = new List<UpdateCustomerInvoiceItemRequest>
            {
                new UpdateCustomerInvoiceItemRequest
                {
                    Id = Guid.NewGuid(), // Existing item
                    Description = "Updated Item 1",
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    UnitPrice = 100m,
                    TaxAmount = 10m
                }
            };
        }
        return request;
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid_With_All_Fields()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid_With_Minimal_Fields_No_Items()
    {
        var request = new UpdateCustomerInvoiceRequest
        {
            Id = Guid.NewGuid(),
            Notes = "Only notes updated"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var request = CreateValidRequest();
        request.Id = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage("'Id' must not be empty.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_InvoiceDate_Is_Null_Not_Provided()
    {
        var request = new UpdateCustomerInvoiceRequest { Id = Guid.NewGuid() }; // InvoiceDate is null
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.InvoiceDate);
    }

    [Fact]
    public void Should_Have_Error_When_DueDate_Is_Before_InvoiceDate_If_Both_Provided()
    {
        var request = CreateValidRequest();
        request.InvoiceDate = DateTime.UtcNow.Date;
        request.DueDate = request.InvoiceDate.Value.AddDays(-1); // DueDate before InvoiceDate
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DueDate)
            .WithErrorMessage("Due date must be on or after the invoice date.");
    }

    [Fact]
    public void Should_Have_Error_When_Currency_Is_InvalidLength_If_Provided()
    {
        var request = CreateValidRequest();
        request.Currency = "US"; // Invalid length
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    // Tests for nested UpdateCustomerInvoiceItemRequestValidator
    [Fact]
    public void Item_Should_Have_Error_When_Id_Is_EmptyGuid_If_Provided_And_NotEmpty()
    {
        var request = CreateValidRequest();
        request.InvoiceItems![0].Id = Guid.Empty; // Explicitly setting to empty Guid
        var result = _validator.TestValidate(request);
        // Rule: .NotEmptyGuid().When(i => i.Id.HasValue && i.Id.Value != Guid.Empty);
        // This rule is a bit counter-intuitive. NotEmptyGuid means "must not be Guid.Empty".
        // The .When condition means it only applies if Id has a value AND that value is not already Guid.Empty.
        // This effectively means the rule `NotEmptyGuid()` is only active if `Id.Value != Guid.Empty` is FALSE, i.e. `Id.Value == Guid.Empty`.
        // So if Id.Value is Guid.Empty, then NotEmptyGuid() runs and fails.
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].Id").WithErrorMessage("'Id' must not be empty.");
    }


    [Fact]
    public void Item_Should_Not_Have_Error_When_Id_Is_Null_For_New_Item()
    {
        var request = CreateValidRequest();
        request.InvoiceItems![0].Id = null; // New item
        request.InvoiceItems[0].Description = "New Item Desc"; // Description required for new item
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor("InvoiceItems[0].Id");
    }

    [Fact]
    public void Item_Should_Have_Error_When_Description_Is_Empty_If_New_Item()
    {
        var request = CreateValidRequest();
        request.InvoiceItems![0].Id = null; // New item
        request.InvoiceItems[0].Description = string.Empty;
        var result = _validator.TestValidate(request);
        // Rule: .NotEmpty().When(i => i.Description is not null || !i.Id.HasValue);
        // Here, Description is not null (it's empty), and !i.Id.HasValue is true. So NotEmpty() applies.
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].Description");
    }

    [Fact]
    public void Item_Should_Not_Have_Error_When_Description_Is_Null_If_Existing_Item_And_Not_Provided()
    {
        var request = CreateValidRequest(); // Item has Id, so it's existing
        request.InvoiceItems![0].Description = null; // Description is null (not provided for update)
        var result = _validator.TestValidate(request);
        // Rule: .NotEmpty().When(i => i.Description is not null || !i.Id.HasValue);
        // Here, Description is null, and !i.Id.HasValue is false. So NotEmpty() condition is false.
        result.ShouldNotHaveValidationErrorFor("InvoiceItems[0].Description");
    }


    [Fact]
    public void Item_Should_Have_Error_When_Quantity_Is_Negative_If_Provided()
    {
        var request = CreateValidRequest();
        request.InvoiceItems![0].Quantity = -1;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].Quantity");
    }
}
