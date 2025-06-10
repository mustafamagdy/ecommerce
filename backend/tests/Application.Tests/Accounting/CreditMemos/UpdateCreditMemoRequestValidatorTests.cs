using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.CreditMemos;
using System;

namespace Application.Tests.Accounting.CreditMemos;

public class UpdateCreditMemoRequestValidatorTests
{
    private readonly UpdateCreditMemoRequestValidator _validator = new();

    private UpdateCreditMemoRequest CreateValidRequest(Guid? id = null, string? reason = "Updated Reason") => new()
    {
        Id = id ?? Guid.NewGuid(),
        Date = DateTime.UtcNow.Date,
        Reason = reason,
        TotalAmount = 150m,
        Currency = "EUR",
        Notes = "Updated notes",
        OriginalCustomerInvoiceId = Guid.NewGuid()
    };

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid_With_All_Fields()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Only_Id_And_One_Field_Is_Provided()
    {
        var request = new UpdateCreditMemoRequest { Id = Guid.NewGuid(), Notes = "Only notes updated" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var request = CreateValidRequest(id: Guid.Empty);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id).WithErrorMessage("'Id' must not be empty.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Date_Is_Null_Not_Provided()
    {
        var request = new UpdateCreditMemoRequest { Id = Guid.NewGuid(), Reason = "Test" }; // Date is null
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void Should_Have_Error_When_Date_Is_Default_If_Provided()
    {
        var request = CreateValidRequest();
        request.Date = default(DateTime); // Explicitly set to default
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Date);
    }


    [Fact]
    public void Should_Have_Error_When_Reason_Is_Empty_If_Provided()
    {
        var request = CreateValidRequest(reason: string.Empty);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Should_Have_Error_When_TotalAmount_Is_Negative_If_Provided()
    {
        var request = CreateValidRequest();
        request.TotalAmount = -50m;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TotalAmount);
    }

    [Fact]
    public void Should_Have_Error_When_Currency_Is_InvalidLength_If_Provided()
    {
        var request = CreateValidRequest();
        request.Currency = "E";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Should_Have_Error_When_OriginalCustomerInvoiceId_Is_Empty_If_Provided()
    {
        var request = CreateValidRequest();
        request.OriginalCustomerInvoiceId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.OriginalCustomerInvoiceId).WithErrorMessage("'Original Customer Invoice Id' must not be empty.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_OriginalCustomerInvoiceId_Is_Null_Not_Provided()
    {
        var request = new UpdateCreditMemoRequest { Id = Guid.NewGuid(), Notes = "No Original Invoice Id" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.OriginalCustomerInvoiceId);
    }
}
