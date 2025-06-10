using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.BankReconciliations;
using System;

namespace Application.Tests.Accounting.BankReconciliations;

public class CreateBankReconciliationRequestValidatorTests
{
    private readonly CreateBankReconciliationRequestValidator _validator = new();

    private CreateBankReconciliationRequest CreateValidRequest() => new()
    {
        BankAccountId = Guid.NewGuid(),
        ReconciliationDate = DateTime.UtcNow.Date,
        BankStatementId = Guid.NewGuid()
        // SystemBalance is not part of the request based on previous definition,
        // it's calculated or fetched by the handler.
    };

    [Fact]
    public void Should_Not_Have_Error_When_Request_Is_Valid()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_BankAccountId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.BankAccountId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BankAccountId).WithErrorMessage("'Bank Account Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_ReconciliationDate_Is_Default()
    {
        var request = CreateValidRequest();
        request.ReconciliationDate = default;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ReconciliationDate);
    }

    [Fact]
    public void Should_Have_Error_When_BankStatementId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.BankStatementId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BankStatementId).WithErrorMessage("'Bank Statement Id' must not be empty.");
    }
}
