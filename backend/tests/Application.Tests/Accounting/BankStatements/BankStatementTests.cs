using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For BankStatement, BankStatementTransaction, BankTransactionType
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Tests.Accounting.BankStatements;

public class BankStatementTests
{
    private BankStatement CreateTestBankStatement(
        Guid? bankAccountId = null,
        DateTime? statementDate = null,
        decimal openingBalance = 1000m,
        decimal closingBalance = 1500m,
        string? referenceNumber = "STMT-001")
    {
        return new BankStatement(
            bankAccountId ?? Guid.NewGuid(),
            statementDate ?? DateTime.UtcNow.Date,
            openingBalance,
            closingBalance,
            referenceNumber);
    }

    private BankStatementTransaction CreateTestTransaction(Guid statementId, decimal amount, BankTransactionType type, DateTime? date = null)
    {
        return new BankStatementTransaction(
            statementId,
            date ?? DateTime.UtcNow.Date,
            type == BankTransactionType.Credit ? "Credit Transaction" : "Debit Transaction",
            amount,
            type);
    }

    [Fact]
    public void Constructor_Should_InitializeBankStatementCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date.AddDays(-1);
        var openBal = 500.25m;
        var closeBal = 1200.75m;
        var refNum = "STMT-MAIN-001";

        // Act
        var statement = new BankStatement(accountId, date, openBal, closeBal, refNum);

        // Assert
        statement.Id.Should().NotBe(Guid.Empty);
        statement.BankAccountId.Should().Be(accountId);
        statement.StatementDate.Should().Be(date);
        statement.OpeningBalance.Should().Be(openBal);
        statement.ClosingBalance.Should().Be(closeBal);
        statement.ReferenceNumber.Should().Be(refNum);
        statement.ImportDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5)); // Defaulted to UtcNow
        statement.Transactions.Should().BeEmpty();
    }

    [Fact]
    public void AddTransaction_SingleTransaction_Should_AddToTransactionsList()
    {
        // Arrange
        var statement = CreateTestBankStatement();
        var transaction = CreateTestTransaction(statement.Id, 100m, BankTransactionType.Credit);

        // Act
        statement.AddTransaction(transaction);

        // Assert
        statement.Transactions.Should().HaveCount(1);
        statement.Transactions.First().Should().Be(transaction);
    }

    [Fact]
    public void AddTransactions_MultipleTransactions_Should_AddAllToTransactionsList()
    {
        // Arrange
        var statement = CreateTestBankStatement();
        var transactions = new List<BankStatementTransaction>
        {
            CreateTestTransaction(statement.Id, 100m, BankTransactionType.Credit),
            CreateTestTransaction(statement.Id, 50m, BankTransactionType.Debit)
        };

        // Act
        statement.AddTransactions(transactions);

        // Assert
        statement.Transactions.Should().HaveCount(2);
        statement.Transactions.Should().Contain(transactions);
    }

    [Fact]
    public void AddTransaction_BelongingToAnotherStatement_Should_ThrowArgumentException()
    {
        // Arrange
        var statement = CreateTestBankStatement();
        var otherStatementId = Guid.NewGuid();
        var transactionForOtherStatement = CreateTestTransaction(otherStatementId, 50m, BankTransactionType.Credit);

        // Act
        Action act = () => statement.AddTransaction(transactionForOtherStatement);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Transaction belongs to another statement. (Parameter 'transaction')");
    }

    [Fact]
    public void Update_Should_ModifyEditableProperties()
    {
        // Arrange
        var statement = CreateTestBankStatement();
        var originalAccountId = statement.BankAccountId; // Not updatable via this method
        var originalImportDate = statement.ImportDate; // Not updatable via this method

        var newDate = DateTime.UtcNow.Date.AddDays(-2);
        var newOpenBal = 400m;
        var newCloseBal = 1100m;
        var newRef = "STMT-UPD-002";

        // Act
        statement.Update(newDate, newOpenBal, newCloseBal, newRef);

        // Assert
        statement.StatementDate.Should().Be(newDate);
        statement.OpeningBalance.Should().Be(newOpenBal);
        statement.ClosingBalance.Should().Be(newCloseBal);
        statement.ReferenceNumber.Should().Be(newRef);
        statement.BankAccountId.Should().Be(originalAccountId); // Unchanged
        statement.ImportDate.Should().Be(originalImportDate);   // Unchanged
    }

    [Fact]
    public void Update_WithNullValues_Should_NotChangeExistingValues()
    {
        // Arrange
        var originalDate = DateTime.UtcNow.Date.AddDays(-5);
        var originalOpen = 100m;
        var originalClose = 200m;
        var originalRef = "REF-ORIG";
        var statement = CreateTestBankStatement(statementDate: originalDate, openingBalance: originalOpen, closingBalance: originalClose, referenceNumber: originalRef);

        // Act
        statement.Update(null, null, null, null); // Pass all nulls

        // Assert
        statement.StatementDate.Should().Be(originalDate);
        statement.OpeningBalance.Should().Be(originalOpen);
        statement.ClosingBalance.Should().Be(originalClose);
        statement.ReferenceNumber.Should().Be(originalRef);
    }

    [Fact]
    public void SetImportDate_Should_UpdateImportDate()
    {
        // Arrange
        var statement = CreateTestBankStatement();
        var newImportDate = DateTime.UtcNow.AddHours(-1).Date; // Different from default UtcNow

        // Act
        statement.SetImportDate(newImportDate);

        // Assert
        statement.ImportDate.Should().Be(newImportDate);
    }

    // Note: The BankStatement entity does not currently enforce that
    // OpeningBalance + Credits - Debits = ClosingBalance upon adding transactions.
    // This validation is present in CreateBankStatementRequestValidator.
    // If such logic were added to the BankStatement domain entity (e.g., in AddTransaction or a ValidateTotals method),
    // it would be tested here. For now, AddTransaction only adds to the list.
}
