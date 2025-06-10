using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using FSH.WebApi.Application.Accounting.BankStatements;
using FSH.WebApi.Domain.Accounting; // For BankTransactionType enum
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Tests.Accounting.BankStatements;

public class CreateBankStatementRequestValidatorTests
{
    private readonly CreateBankStatementRequestValidator _validator = new();

    private CreateBankStatementRequest CreateValidRequest(
        decimal openingBalance = 1000m,
        decimal closingBalance = 1100m,
        List<CreateBankStatementTransactionRequestItem>? transactions = null)
    {
        if (transactions == null)
        {
            // Default transactions that reconcile with default balances
            transactions = new List<CreateBankStatementTransactionRequestItem>
            {
                new CreateBankStatementTransactionRequestItem { TransactionDate = DateTime.UtcNow.Date, Description = "Credit 150", Amount = 150m, Type = BankTransactionType.Credit },
                new CreateBankStatementTransactionRequestItem { TransactionDate = DateTime.UtcNow.Date, Description = "Debit 50", Amount = 50m, Type = BankTransactionType.Debit }
            }; // Net +100, so 1000 + 100 = 1100
        }

        return new CreateBankStatementRequest
        {
            BankAccountId = Guid.NewGuid(),
            StatementDate = DateTime.UtcNow.Date,
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance,
            ReferenceNumber = "STMT-VALID-001",
            Transactions = transactions
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
    public void Should_Have_Error_When_BankAccountId_Is_Empty()
    {
        var request = CreateValidRequest();
        request.BankAccountId = Guid.Empty;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BankAccountId).WithErrorMessage("'Bank Account Id' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_StatementDate_Is_Default()
    {
        var request = CreateValidRequest();
        request.StatementDate = default;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.StatementDate);
    }

    [Fact]
    public void Should_Have_Error_When_Balances_And_Transactions_Do_Not_Reconcile()
    {
        // Opening 1000, Closing 1100. Transactions sum to +150 (200 Cr - 50 Dr). Expected Closing = 1150.
        var transactions = new List<CreateBankStatementTransactionRequestItem>
        {
            new CreateBankStatementTransactionRequestItem { TransactionDate = DateTime.UtcNow.Date, Description = "Credit Big", Amount = 200m, Type = BankTransactionType.Credit },
            new CreateBankStatementTransactionRequestItem { TransactionDate = DateTime.UtcNow.Date, Description = "Debit Small", Amount = 50m, Type = BankTransactionType.Debit }
        };
        var request = CreateValidRequest(openingBalance: 1000m, closingBalance: 1100m, transactions: transactions); // Closing should be 1150

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x) // Root object due to cross-property rule
            .WithErrorMessage("Closing balance, opening balance, and transaction totals do not reconcile.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Balances_And_Transactions_Reconcile_With_No_Transactions()
    {
        // Opening 1000, Closing 1000. Transactions sum to 0.
        var request = CreateValidRequest(openingBalance: 1000m, closingBalance: 1000m, transactions: new List<CreateBankStatementTransactionRequestItem>());

        var result = _validator.TestValidate(request);
        // The .When(p => p.Transactions != null && p.Transactions.Any()) on the balance rule means it won't fire for empty transactions.
        // This behavior is correct as per the current validator.
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public void Item_Should_Have_Error_When_TransactionDate_Is_Default()
    {
        var request = CreateValidRequest();
        request.Transactions[0].TransactionDate = default;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Transactions[0].TransactionDate");
    }

    [Fact]
    public void Item_Should_Have_Error_When_Description_Is_NullOrEmpty()
    {
        var request = CreateValidRequest();
        request.Transactions[0].Description = null!;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Transactions[0].Description");

        request.Transactions[0].Description = string.Empty;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Transactions[0].Description");
    }

    [Fact]
    public void Item_Should_Have_Error_When_Amount_Is_ZeroOrNegative()
    {
        var request = CreateValidRequest();
        request.Transactions[0].Amount = 0;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Transactions[0].Amount");

        request.Transactions[0].Amount = -10;
        result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Transactions[0].Amount");
    }

    [Fact]
    public void Item_Should_Have_Error_When_Type_Is_InvalidEnumValue()
    {
        var request = CreateValidRequest();
        request.Transactions[0].Type = (BankTransactionType)99; // Invalid enum value
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Transactions[0].Type");
    }
}
