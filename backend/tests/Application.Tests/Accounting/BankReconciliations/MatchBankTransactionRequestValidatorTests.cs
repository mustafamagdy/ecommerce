using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.BankReconciliations;
using System;

namespace Application.Tests.Accounting.BankReconciliations;

public class MatchBankTransactionRequestValidatorTests
{
    private readonly MatchBankTransactionRequestValidator _validator = new();

    private MatchBankTransactionRequest CreateValidMatchRequest() => new()
    {
        BankReconciliationId = Guid.NewGuid(),
        BankStatementTransactionId = Guid.NewGuid(),
        IsMatched = true,
        SystemTransactionId = Guid.NewGuid(),
        SystemTransactionType = "CustomerPayment"
    };

    private MatchBankTransactionRequest CreateValidUnmatchRequest() => new()
    {
        BankReconciliationId = Guid.NewGuid(),
        BankStatementTransactionId = Guid.NewGuid(),
        IsMatched = false
        // SystemTransactionId and SystemTransactionType can be null for unmatching
    };

    [Fact]
    public void Should_Not_Have_Error_When_Match_Request_Is_Valid()
    {
        var request = CreateValidMatchRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Unmatch_Request_Is_Valid()
    {
        var request = CreateValidUnmatchRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_BankReconciliationId_Is_Empty()
    {
        var request = CreateValidMatchRequest();
        request.BankReconciliationId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BankReconciliationId).WithErrorMessage("'Bank Reconciliation Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_BankStatementTransactionId_Is_Empty()
    {
        var request = CreateValidMatchRequest();
        request.BankStatementTransactionId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BankStatementTransactionId).WithErrorMessage("'Bank Statement Transaction Id' must not be empty.");
    }

    [Fact]
    public void When_IsMatched_True_Should_Have_Error_If_SystemTransactionId_Is_NullOrEmpty()
    {
        var request = CreateValidMatchRequest();
        request.SystemTransactionId = null;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SystemTransactionId).WithErrorMessage("System Transaction ID is required when matching.");

        request.SystemTransactionId = Guid.Empty;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SystemTransactionId).WithErrorMessage("System Transaction ID is required when matching.");
    }

    [Fact]
    public void When_IsMatched_True_Should_Have_Error_If_SystemTransactionType_Is_NullOrEmpty()
    {
        var request = CreateValidMatchRequest();
        request.SystemTransactionType = null;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SystemTransactionType).WithErrorMessage("System Transaction Type is required when matching.");

        request.SystemTransactionType = string.Empty;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SystemTransactionType).WithErrorMessage("System Transaction Type is required when matching.");
    }

    [Fact]
    public void When_IsMatched_True_Should_Have_Error_If_SystemTransactionType_Exceeds_MaxLength()
    {
        var request = CreateValidMatchRequest();
        request.SystemTransactionType = new string('A', 51);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SystemTransactionType);
    }


    [Fact]
    public void When_IsMatched_False_Should_Not_Require_SystemTransactionId_Or_Type()
    {
        var request = CreateValidUnmatchRequest(); // SystemTransactionId and Type are null by default in helper
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.SystemTransactionId);
        result.ShouldNotHaveValidationErrorFor(x => x.SystemTransactionType);
    }
}
