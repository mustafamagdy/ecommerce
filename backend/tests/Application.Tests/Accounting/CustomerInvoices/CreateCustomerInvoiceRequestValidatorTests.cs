using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.CustomerInvoices;
using System;
using System.Collections.Generic;

namespace Application.Tests.Accounting.CustomerInvoices;

public class CreateCustomerInvoiceRequestValidatorTests
{
    private readonly CreateCustomerInvoiceRequestValidator _validator = new();

    private CreateCustomerInvoiceRequest CreateValidRequest()
    {
        return new CreateCustomerInvoiceRequest
        {
            CustomerId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(), // Optional, can be null
            InvoiceDate = DateTime.UtcNow.Date,
            DueDate = DateTime.UtcNow.Date.AddDays(30),
            Currency = "USD",
            Notes = "Valid notes",
            InvoiceItems = new List<CreateCustomerInvoiceItemRequest>
            {
                new CreateCustomerInvoiceItemRequest
                {
                    Description = "Valid Item 1",
                    ProductId = Guid.NewGuid(), // Optional
                    Quantity = 1,
                    UnitPrice = 100m,
                    TaxAmount = 10m
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

    [Fact]
    public void Should_Not_Have_Error_When_Optional_OrderId_And_ProductId_Are_Null()
    {
        var request = CreateValidRequest();
        request.OrderId = null;
        request.InvoiceItems[0].ProductId = null;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.CustomerId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId).WithErrorMessage("'Customer Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_OrderId_Is_Empty_If_Provided()
    {
        var request = CreateValidRequest();
        request.OrderId = Guid.Empty; // Intentionally providing an empty Guid
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.OrderId).WithErrorMessage("'Order Id' must not be empty.");
    }


    [Fact]
    public void Should_Have_Error_When_InvoiceDate_Is_Default()
    {
        var request = CreateValidRequest();
        request.InvoiceDate = default;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InvoiceDate);
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
    public void Should_Have_Error_When_Currency_Is_Not_CorrectLength()
    {
        var request = CreateValidRequest();
        request.Currency = "US"; // Too short
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Currency);

        request.Currency = "USDOLLARS"; // Too long
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Should_Have_Error_When_InvoiceItems_Is_Empty()
    {
        var request = CreateValidRequest();
        request.InvoiceItems = new List<CreateCustomerInvoiceItemRequest>();
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InvoiceItems)
            .WithErrorMessage("Invoice must have at least one item.");
    }

    // Tests for nested CreateCustomerInvoiceItemRequestValidator
    [Fact]
    public void Item_Should_Have_Error_When_Description_Is_NullOrEmpty()
    {
        var request = CreateValidRequest();
        request.InvoiceItems[0].Description = null!;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].Description");

        request.InvoiceItems[0].Description = string.Empty;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].Description");
    }

    [Fact]
    public void Item_Should_Have_Error_When_ProductId_Is_Empty_If_Provided()
    {
        var request = CreateValidRequest();
        request.InvoiceItems[0].ProductId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].ProductId").WithErrorMessage("'Product Id' must not be empty.");
    }

    [Fact]
    public void Item_Should_Have_Error_When_Quantity_Is_ZeroOrNegative()
    {
        var request = CreateValidRequest();
        request.InvoiceItems[0].Quantity = 0;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].Quantity");

        request.InvoiceItems[0].Quantity = -1;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].Quantity");
    }

    [Fact]
    public void Item_Should_Have_Error_When_UnitPrice_Is_ZeroOrNegative()
    {
        // The rule is GreaterThan(0), so 0 is invalid.
        var request = CreateValidRequest();
        request.InvoiceItems[0].UnitPrice = 0;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].UnitPrice");

        request.InvoiceItems[0].UnitPrice = -10;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].UnitPrice");
    }

    [Fact]
    public void Item_Should_Have_Error_When_TaxAmount_Is_Negative()
    {
        var request = CreateValidRequest();
        request.InvoiceItems[0].TaxAmount = -5;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("InvoiceItems[0].TaxAmount");
    }

    [Fact]
    public void Item_Should_Not_Have_Error_When_TaxAmount_Is_Zero()
    {
        var request = CreateValidRequest();
        request.InvoiceItems[0].TaxAmount = 0;
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor("InvoiceItems[0].TaxAmount");
    }
}
