using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For BankStatementTransaction, BankTransactionType
using System;

namespace Application.Tests.Accounting.BankStatements;

public class BankStatementTransactionTests
{
    private BankStatementTransaction CreateTestTransaction(
        Guid? bankStatementId = null,
        DateTime? transactionDate = null,
        string description = "Test Transaction",
        decimal amount = 100m,
        BankTransactionType type = BankTransactionType.Debit,
        string? reference = "REF001",
        string? bankProvidedId = "BANKID001")
    {
        return new BankStatementTransaction(
            bankStatementId ?? Guid.NewGuid(),
            transactionDate ?? DateTime.UtcNow.Date,
            description,
            amount,
            type,
            reference,
            bankProvidedId);
    }

    [Fact]
    public void Constructor_Should_InitializeTransactionCorrectly()
    {
        // Arrange
        var statementId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date.AddDays(-1);
        var desc = "Payment Received";
        var amt = 250.75m;
        var transType = BankTransactionType.Credit;
        var refNum = "Dep001";
        var bankId = "TRN-BANK-XYZ";

        // Act
        var transaction = new BankStatementTransaction(statementId, date, desc, amt, transType, refNum, bankId);

        // Assert
        transaction.Id.Should().NotBe(Guid.Empty);
        transaction.BankStatementId.Should().Be(statementId);
        transaction.TransactionDate.Should().Be(date);
        transaction.Description.Should().Be(desc);
        transaction.Amount.Should().Be(amt);
        transaction.Type.Should().Be(transType);
        transaction.Reference.Should().Be(refNum);
        transaction.BankProvidedId.Should().Be(bankId);
        transaction.IsReconciled.Should().BeFalse(); // Default
        transaction.SystemTransactionId.Should().BeNull();
        transaction.SystemTransactionType.Should().BeNull();
        transaction.BankReconciliationId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNegativeAmount_Should_ThrowArgumentOutOfRangeException()
    {
        Action act = () => CreateTestTransaction(amount: -50m);
        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("amount");
    }

    [Fact]
    public void Constructor_WithZeroAmount_Should_ThrowArgumentOutOfRangeException()
    {
        // Amount must be positive, type indicates direction.
        Action act = () => CreateTestTransaction(amount: 0m);
        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("amount");
    }

    [Fact]
    public void MarkAsReconciled_Should_SetReconciliationFieldsCorrectly()
    {
        // Arrange
        var transaction = CreateTestTransaction();
        var reconciliationId = Guid.NewGuid();
        var systemTransId = Guid.NewGuid();
        var systemTransType = "CustomerPayment";

        // Act
        transaction.MarkAsReconciled(reconciliationId, systemTransId, systemTransType);

        // Assert
        transaction.IsReconciled.Should().BeTrue();
        transaction.BankReconciliationId.Should().Be(reconciliationId);
        transaction.SystemTransactionId.Should().Be(systemTransId);
        transaction.SystemTransactionType.Should().Be(systemTransType);
    }

    [Fact]
    public void MarkAsReconciled_WithNullSystemFields_Should_SetThemAsNull()
    {
        // Arrange
        var transaction = CreateTestTransaction();
        var reconciliationId = Guid.NewGuid();

        // Act
        transaction.MarkAsReconciled(reconciliationId, null, null);

        // Assert
        transaction.IsReconciled.Should().BeTrue();
        transaction.BankReconciliationId.Should().Be(reconciliationId);
        transaction.SystemTransactionId.Should().BeNull();
        transaction.SystemTransactionType.Should().BeNull();
    }

    [Fact]
    public void UnmarkAsReconciled_Should_ClearReconciliationFields()
    {
        // Arrange
        var transaction = CreateTestTransaction();
        // First, mark it as reconciled
        transaction.MarkAsReconciled(Guid.NewGuid(), Guid.NewGuid(), "SomeType");

        // Act
        transaction.UnmarkAsReconciled();

        // Assert
        transaction.IsReconciled.Should().BeFalse();
        transaction.BankReconciliationId.Should().BeNull();
        transaction.SystemTransactionId.Should().BeNull();
        transaction.SystemTransactionType.Should().BeNull();
    }

    [Fact]
    public void UpdateDetails_Should_ModifyEditableProperties()
    {
        // Arrange
        var transaction = CreateTestTransaction();
        var originalDate = transaction.TransactionDate; // Date is not updatable by UpdateDetails

        var newDescription = "Updated Transaction Description";
        var newReference = "UPD-REF-002";
        var newBankProvidedId = "UPD-BANKID-002";

        // Act
        transaction.UpdateDetails(newDescription, newReference, newBankProvidedId);

        // Assert
        transaction.Description.Should().Be(newDescription);
        transaction.Reference.Should().Be(newReference);
        transaction.BankProvidedId.Should().Be(newBankProvidedId);
        transaction.TransactionDate.Should().Be(originalDate); // Unchanged by this method
        transaction.Amount.Should().Be(100m); // Unchanged by this method
        transaction.Type.Should().Be(BankTransactionType.Debit); // Unchanged by this method
    }

    [Fact]
    public void UpdateDetails_WithNullValues_Should_UpdatePropertiesToNullWhereAllowed()
    {
        // Arrange
        var transaction = CreateTestTransaction(reference: "InitialRef", bankProvidedId: "InitialBankId");

        // Act
        transaction.UpdateDetails(description: null, reference: null, bankProvidedId: null);

        // Assert
        transaction.Description.Should().Be("Test Transaction"); // Not changed if null passed to UpdateDetails for description
        transaction.Reference.Should().BeNull();
        transaction.BankProvidedId.Should().BeNull();
    }
}
