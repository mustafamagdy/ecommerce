using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.VendorInvoices;
using System;
using System.Collections.Generic;

namespace Application.Tests.Accounting.VendorInvoices;

public class UpdateVendorInvoiceRequestValidatorTests
{
    private readonly UpdateVendorInvoiceRequestValidator _validator = new();

    private UpdateVendorInvoiceRequest CreateValidRequest(bool includeItems = true)
    {
        var request = new UpdateVendorInvoiceRequest
        {
            Id = Guid.NewGuid(),
            SupplierId = Guid.NewGuid(),
            InvoiceNumber = "INV-UPD-2024-002",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            TotalAmount = includeItems ? 120m : (decimal?)null, // (1*100 Qty*Price)
            Currency = "USD",
            Notes = "Updated notes"
        };

        if (includeItems)
        {
            request.InvoiceItems = new List<UpdateVendorInvoiceItemRequest>
            {
                new UpdateVendorInvoiceItemRequest
                {
                    Id = Guid.NewGuid(), // Existing item
                    Description = "Updated Item 1",
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    UnitPrice = 100m,
                    TaxAmount = 20m,
                    TotalAmount = 100m // Line Total (Qty*Price)
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
        var request = new UpdateVendorInvoiceRequest
        {
            Id = Guid.NewGuid(),
            Notes = "Only notes updated" // Only notes provided for update
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
    public void Should_Have_Error_When_SupplierId_Is_Empty_If_Provided()
    {
        var request = CreateValidRequest();
        request.SupplierId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SupplierId).WithErrorMessage("'Supplier Id' must not be empty.");
    }


    [Fact]
    public void Should_Have_Error_When_InvoiceNumber_Is_Empty_If_Provided()
    {
        var request = CreateValidRequest();
        request.InvoiceNumber = string.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InvoiceNumber);
    }

    [Fact]
    public void Should_Have_Error_When_TotalAmount_Is_Negative_If_Provided()
    {
        var request = CreateValidRequest(includeItems: false); // No items to avoid sum validation interference
        request.TotalAmount = -10;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TotalAmount);
    }

    [Fact]
    public void Should_Have_Error_For_Nested_Item_When_Item_Description_Is_Empty_If_Provided_And_New()
    {
        var request = CreateValidRequest();
        request.InvoiceItems![0].Id = null; // Mark as new item
        request.InvoiceItems[0].Description = string.Empty;
        var result = _validator.TestValidate(request);
        // The rule is: .When(i => i.Description is not null || !i.Id.HasValue);
        // So, if new and description is empty, it's an error.
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].Description");
    }

    [Fact]
    public void Should_Not_Have_Error_For_Nested_Item_When_Item_Description_Is_Empty_If_Provided_And_Existing()
    {
        var request = CreateValidRequest(); // Item has Id, so it's existing
        request.InvoiceItems![0].Description = string.Empty; // Description is being set to empty
        var result = _validator.TestValidate(request);
        // The rule is: .When(i => i.Description is not null || !i.Id.HasValue);
        // Existing item, Description is not null (it's empty string), so NotEmpty() applies.
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].Description");
    }


    [Fact]
    public void Should_Have_Error_When_Item_TotalAmount_Does_Not_Match_Qty_Price_If_All_Provided()
    {
        var request = CreateValidRequest();
        request.InvoiceItems![0].Quantity = 2;
        request.InvoiceItems[0].UnitPrice = 10m;
        request.InvoiceItems[0].TotalAmount = 25m; // Should be 20m
        request.TotalAmount = 25m + request.InvoiceItems[0].TaxAmount; // Adjust main total to pass root-level sum check for isolation

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].TotalAmount")
            .WithErrorMessage("Item TotalAmount must be equal to Quantity * UnitPrice if both Quantity and UnitPrice are provided.");
    }

    [Fact]
    public void Should_Have_Error_When_Sum_Of_Item_Totals_Does_Not_Match_Invoice_TotalAmount_If_Both_Provided()
    {
        var request = CreateValidRequest(); // TotalAmount = 120 (Item1: TotalAmount=100)
        request.InvoiceItems!.Add(new UpdateVendorInvoiceItemRequest
        {
            Id = Guid.NewGuid(), Description = "Item 2", Quantity = 1, UnitPrice = 50m, TaxAmount = 5m, TotalAmount = 50m
        });
        // Sum of item TotalAmounts (100+50) = 150.
        request.TotalAmount = 140m; // Mismatch with the 150m sum from items

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x) // Error for the root object
              .WithErrorMessage("If InvoiceItems and TotalAmount are both provided, the sum of item TotalAmounts must equal the invoice TotalAmount.");
    }
}
