using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.CreditMemos;
using System;

namespace Application.Tests.Accounting.CreditMemos;

public class ApplyCreditMemoToInvoiceRequestValidatorTests
{
    private readonly ApplyCreditMemoToInvoiceRequestValidator _validator = new();

    private ApplyCreditMemoToInvoiceRequest CreateValidRequest() => new()
    {
        CreditMemoId = Guid.NewGuid(),
        CustomerInvoiceId = Guid.NewGuid(),
        AmountToApply = 100m
    };

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_CreditMemoId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.CreditMemoId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CreditMemoId).WithErrorMessage("'Credit Memo Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerInvoiceId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.CustomerInvoiceId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CustomerInvoiceId).WithErrorMessage("'Customer Invoice Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_AmountToApply_Is_Zero()
    {
        var request = CreateValidRequest();
        request.AmountToApply = 0;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AmountToApply);
    }

    [Fact]
    public void Should_Have_Error_When_AmountToApply_Is_Negative()
    {
        var request = CreateValidRequest();
        request.AmountToApply = -50m;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AmountToApply);
    }
}
