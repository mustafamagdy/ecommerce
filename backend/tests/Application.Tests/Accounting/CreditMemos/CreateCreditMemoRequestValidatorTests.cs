using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.CreditMemos;
using System;

namespace Application.Tests.Accounting.CreditMemos;

public class CreateCreditMemoRequestValidatorTests
{
    private readonly CreateCreditMemoRequestValidator _validator = new();

    private CreateCreditMemoRequest CreateValidRequest() => new()
    {
        CustomerId = Guid.NewGuid(),
        Date = DateTime.UtcNow.Date,
        Reason = "Valid Reason for Credit",
        TotalAmount = 100m,
        Currency = "USD",
        Notes = "Some notes here",
        OriginalCustomerInvoiceId = Guid.NewGuid() // Optional, can be null
    };

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Optional_OriginalCustomerInvoiceId_And_Notes_Are_Null()
    {
        var request = CreateValidRequest();
        request.OriginalCustomerInvoiceId = null;
        request.Notes = null;
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
    public void Should_Have_Error_When_Date_Is_Default()
    {
        var request = CreateValidRequest();
        request.Date = default;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Should_Have_Error_When_Reason_Is_NullOrEmpty()
    {
        var request = CreateValidRequest();
        request.Reason = null!;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Reason);

        request.Reason = string.Empty;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Should_Have_Error_When_Reason_Exceeds_MaxLength()
    {
        var request = CreateValidRequest();
        request.Reason = new string('A', 501);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Reason);
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
        request.Currency = "US";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Currency);

        request.Currency = "USDOLLARS";
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Should_Have_Error_When_Notes_Exceeds_MaxLength()
    {
        var request = CreateValidRequest();
        request.Notes = new string('A', 2001);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Should_Have_Error_When_OriginalCustomerInvoiceId_Is_Empty_If_Provided()
    {
        var request = CreateValidRequest();
        request.OriginalCustomerInvoiceId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.OriginalCustomerInvoiceId).WithErrorMessage("'Original Customer Invoice Id' must not be empty.");
    }
}
