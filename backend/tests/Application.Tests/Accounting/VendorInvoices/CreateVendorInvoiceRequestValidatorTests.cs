using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.VendorInvoices;
using FSH.WebApi.Domain.Accounting; // For VendorInvoiceStatus if used in request/validator
using System;
using System.Collections.Generic;

namespace Application.Tests.Accounting.VendorInvoices;

public class CreateVendorInvoiceRequestValidatorTests
{
    private readonly CreateVendorInvoiceRequestValidator _validator = new();

    private CreateVendorInvoiceRequest CreateValidRequest()
    {
        return new CreateVendorInvoiceRequest
        {
            SupplierId = Guid.NewGuid(),
            InvoiceNumber = "INV-2024-001",
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            TotalAmount = 120m, // 100 (item) + 20 (item tax)
            Currency = "USD",
            Notes = "Valid notes",
            InvoiceItems = new List<CreateVendorInvoiceItemRequest>
            {
                new CreateVendorInvoiceItemRequest
                {
                    Description = "Valid Item 1",
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    UnitPrice = 100m,
                    TaxAmount = 20m, // Tax for this item
                    TotalAmount = 100m // Line total (Qty * Price)
                }
            }
        };
    }

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact],
    public void Should_Have_Error_When_SupplierId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.SupplierId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SupplierId).WithErrorMessage("'Supplier Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_InvoiceNumber_Is_NullOrEmpty()
    {
        var request = CreateValidRequest();
        request.InvoiceNumber = null!;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InvoiceNumber);

        request.InvoiceNumber = string.Empty;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InvoiceNumber);
    }

    [Fact]
    public void Should_Have_Error_When_InvoiceNumber_Exceeds_MaxLength()
    {
        var request = CreateValidRequest();
        request.InvoiceNumber = new string('A', 101);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InvoiceNumber);
    }

    [Fact]
    public void Should_Have_Error_When_InvoiceDate_Is_Default() // NotEmpty equivalent for DateTime
    {
        var request = CreateValidRequest();
        request.InvoiceDate = default;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InvoiceDate);
    }

     [Fact]
    public void Should_Have_Error_When_InvoiceDate_In_Distant_Future()
    {
        var request = CreateValidRequest();
        request.InvoiceDate = DateTime.UtcNow.AddDays(5); // Validator allows up to AddDays(1)
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InvoiceDate)
            .WithErrorMessage("Invoice date must be today or in the past, or not too far in the future.");
    }


    [Fact]
    public void Should_Have_Error_When_DueDate_Is_Before_InvoiceDate()
    {
        var request = CreateValidRequest();
        request.InvoiceDate = DateTime.UtcNow.Date;
        request.DueDate = request.InvoiceDate.AddDays(-1);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DueDate)
            .WithErrorMessage("Due date must be on or after the invoice date.");
    }

    [Fact]
    public void Should_Have_Error_When_TotalAmount_Is_ZeroOrNegative()
    {
        var request = CreateValidRequest();
        request.TotalAmount = 0;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TotalAmount);

        request.TotalAmount = -10;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TotalAmount);
    }

    [Fact]
    public void Should_Have_Error_When_Currency_Is_Not_CorrectLength()
    {
        var request = CreateValidRequest();
        request.Currency = "US"; // Too short
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Currency);

        request.Currency = "USDOLL"; // Too long
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Should_Have_Error_When_InvoiceItems_Is_Empty()
    {
        var request = CreateValidRequest();
        request.InvoiceItems = new List<CreateVendorInvoiceItemRequest>();
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InvoiceItems)
            .WithErrorMessage("Invoice must have at least one item.");
    }

    [Fact]
    public void Should_Have_Error_For_Nested_Item_When_Item_Description_Is_Null()
    {
        var request = CreateValidRequest();
        request.InvoiceItems[0].Description = null!;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].Description");
    }

    [Fact]
    public void Should_Have_Error_When_Item_TotalAmount_Does_Not_Match_Qty_Price()
    {
        var request = CreateValidRequest();
        request.InvoiceItems[0].Quantity = 2;
        request.InvoiceItems[0].UnitPrice = 10m;
        request.InvoiceItems[0].TotalAmount = 25m; // Should be 20m
        // The main validator also checks sum of item.TotalAmount vs request.TotalAmount
        // For this test, adjust main TotalAmount to pass that rule, to isolate item rule
        request.TotalAmount = 25m + request.InvoiceItems[0].TaxAmount;


        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].TotalAmount")
            .WithErrorMessage("Item TotalAmount must be equal to Quantity * UnitPrice.");
    }

    [Fact]
    public void Should_Have_Error_When_Sum_Of_Item_Totals_Does_Not_Match_Invoice_TotalAmount()
    {
        var request = CreateValidRequest(); // TotalAmount = 120 (Item1: Q*P=100, Tax=20)
        request.InvoiceItems.Add(new CreateVendorInvoiceItemRequest
        {
            Description = "Item 2", Quantity = 1, UnitPrice = 50m, TaxAmount = 5m, TotalAmount = 50m
        });
        // Sum of items (100+50) = 150. Expected Invoice TotalAmount = 150
        // Or if item.TotalAmount is (Qty*Price + Tax)
        // Item1: 120, Item2: 55 => Sum = 175
        // The rule is: Sum(item.TotalAmount) == p.TotalAmount based on validator
        // Item1.TotalAmount = 100, Item2.TotalAmount = 50. Sum = 150.
        // But request.TotalAmount is still 120.
        request.TotalAmount = 140; // Mismatch

        var result = _validator.TestValidate(request);
        // This rule "The sum of item TotalAmounts must equal the invoice TotalAmount."
        result.ShouldHaveValidationErrorFor(x => x) // Error for the root object due to cross-property rule
              .WithErrorMessage("The sum of item TotalAmounts must equal the invoice TotalAmount.");
    }
}
