using Xunit;
using FluentAssertions;
using FSH.WebApi.Domain.Accounting; // For BankReconciliation, ReconciliationStatus
using System;

namespace Application.Tests.Accounting.BankReconciliations;

public class BankReconciliationTests
{
    private BankReconciliation CreateTestBankReconciliation(
        Guid? bankAccountId = null,
        DateTime? reconciliationDate = null,
        Guid? bankStatementId = null,
        decimal statementBalance = 1000m,
        decimal systemBalance = 1000m,
        ReconciliationStatus status = ReconciliationStatus.Draft)
    {
        // The BankReconciliation constructor in the domain was:
        // BankReconciliation(Guid bankAccountId, DateTime reconciliationDate, Guid bankStatementId, decimal statementBalance, decimal systemBalance, ReconciliationStatus status = ReconciliationStatus.Draft)
        // It also initialized ManuallyAssignedUnclearedChecks and ManuallyAssignedDepositsInTransit to 0.

        return new BankReconciliation(
            bankAccountId ?? Guid.NewGuid(),
            reconciliationDate ?? DateTime.UtcNow.Date,
            bankStatementId ?? Guid.NewGuid(),
            statementBalance,
            systemBalance,
            status);
    }

    [Fact]
    public void Constructor_Should_InitializeBankReconciliationCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var reconDate = DateTime.UtcNow.Date.AddDays(-1);
        var stmtId = Guid.NewGuid();
        var stmtBal = 1250.50m;
        var sysBal = 1200.25m;
        var initialStatus = ReconciliationStatus.InProgress;

        // Act
        var reconciliation = new BankReconciliation(accountId, reconDate, stmtId, stmtBal, sysBal, initialStatus);

        // Assert
        reconciliation.Id.Should().NotBe(Guid.Empty);
        reconciliation.BankAccountId.Should().Be(accountId);
        reconciliation.ReconciliationDate.Should().Be(reconDate);
        reconciliation.BankStatementId.Should().Be(stmtId);
        reconciliation.StatementBalance.Should().Be(stmtBal);
        reconciliation.SystemBalance.Should().Be(sysBal);
        reconciliation.Status.Should().Be(initialStatus);
        reconciliation.ManuallyAssignedUnclearedChecks.Should().Be(0); // Default from constructor
        reconciliation.ManuallyAssignedDepositsInTransit.Should().Be(0); // Default from constructor

        // Initial Difference: StatementBalance - SystemBalance - 0 + 0
        reconciliation.Difference.Should().Be(stmtBal - sysBal);
    }

    [Fact]
    public void UpdateBalancesAndStatus_Should_UpdateProvidedValuesAndRecalculateDifference()
    {
        // Arrange
        var reconciliation = CreateTestBankReconciliation(statementBalance: 1000m, systemBalance: 950m); // Initial Diff = 50
        var newSystemBalance = 900m;
        var newStatus = ReconciliationStatus.InProgress;
        var newUnclearedChecks = 20m;
        var newDepositsInTransit = 30m;

        // Act
        // UpdateBalancesAndStatus(decimal? statementBalance, decimal? systemBalance, ReconciliationStatus? status, decimal? unclearedChecks = null, decimal? depositsInTransit = null)
        reconciliation.UpdateBalancesAndStatus(null, newSystemBalance, newStatus, newUnclearedChecks, newDepositsInTransit);

        // Assert
        reconciliation.StatementBalance.Should().Be(1000m); // Unchanged
        reconciliation.SystemBalance.Should().Be(newSystemBalance);
        reconciliation.Status.Should().Be(newStatus);
        reconciliation.ManuallyAssignedUnclearedChecks.Should().Be(newUnclearedChecks);
        reconciliation.ManuallyAssignedDepositsInTransit.Should().Be(newDepositsInTransit);

        // Expected Difference: 1000 (stmt) - 900 (sys) - 20 (uncleared) + 30 (transit) = 100 - 20 + 30 = 110
        reconciliation.Difference.Should().Be(1000m - 900m - 20m + 30m);
    }


    [Fact]
    public void UpdateBalancesAndStatus_OnlyStatus_Should_UpdateStatusAndNotAffectBalances()
    {
        // Arrange
        var statementBal = 1000m;
        var systemBal = 980m; // Initial Diff = 20
        var reconciliation = CreateTestBankReconciliation(statementBalance: statementBal, systemBalance: systemBal);
        var initialDifference = reconciliation.Difference;

        // Act
        reconciliation.UpdateBalancesAndStatus(null, null, ReconciliationStatus.PendingReview, null, null);

        // Assert
        reconciliation.Status.Should().Be(ReconciliationStatus.PendingReview);
        reconciliation.StatementBalance.Should().Be(statementBal);
        reconciliation.SystemBalance.Should().Be(systemBal);
        reconciliation.ManuallyAssignedUnclearedChecks.Should().Be(0);
        reconciliation.ManuallyAssignedDepositsInTransit.Should().Be(0);
        reconciliation.Difference.Should().Be(initialDifference);
    }

    [Fact]
    public void UpdateBalancesAndStatus_OnCompletedReconciliation_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var reconciliation = CreateTestBankReconciliation(status: ReconciliationStatus.Completed);

        // Act
        Action act = () => reconciliation.UpdateBalancesAndStatus(null, 1000m, ReconciliationStatus.InProgress, null, null);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Cannot update reconciliation in '{ReconciliationStatus.Completed}' status.");
    }

    [Fact]
    public void UpdateBalancesAndStatus_OnClosedReconciliation_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var reconciliation = CreateTestBankReconciliation(status: ReconciliationStatus.Closed);

        // Act
        Action act = () => reconciliation.UpdateBalancesAndStatus(null, 1000m, ReconciliationStatus.InProgress, null, null);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Cannot update reconciliation in '{ReconciliationStatus.Closed}' status.");
    }


    [Fact]
    public void Difference_Property_Should_CalculateCorrectly_WithVariousAdjustments()
    {
        // Arrange
        var reconciliation = CreateTestBankReconciliation(statementBalance: 1000m, systemBalance: 1000m); // Initial Diff = 0

        // Assert initial state
        reconciliation.Difference.Should().Be(0);

        // Act & Assert: System balance lower than statement (e.g. outstanding checks > deposits in transit)
        reconciliation.UpdateBalancesAndStatus(null, 900m, null, 150m, 50m);
        // Diff = 1000 - 900 - 150 + 50 = 100 - 150 + 50 = 0
        reconciliation.Difference.Should().Be(0);

        // Act & Assert: System balance higher than statement (e.g. deposits in transit > outstanding checks)
        reconciliation.UpdateBalancesAndStatus(null, 1100m, null, 50m, 150m);
        // Diff = 1000 - 1100 - 50 + 150 = -100 - 50 + 150 = 0
        reconciliation.Difference.Should().Be(0);

        // Act & Assert: Only uncleared checks
        reconciliation.UpdateBalancesAndStatus(null, 1000m, null, 100m, 0m);
        // Diff = 1000 - 1000 - 100 + 0 = -100
        reconciliation.Difference.Should().Be(-100m);

        // Act & Assert: Only deposits in transit
        reconciliation.UpdateBalancesAndStatus(null, 1000m, null, 0m, 100m);
        // Diff = 1000 - 1000 - 0 + 100 = 100
        reconciliation.Difference.Should().Be(100m);
    }

    [Fact]
    public void CompleteReconciliation_WhenDifferenceIsZero_Should_SetStatusToCompleted()
    {
        // Arrange
        // Statement 1000, System 900, Uncleared 150, Transit 50. Diff = 1000 - 900 - 150 + 50 = 0
        var reconciliation = CreateTestBankReconciliation(statementBalance: 1000m, systemBalance: 900m);
        reconciliation.UpdateBalancesAndStatus(null, null, null, 150m, 50m);
        reconciliation.Difference.Should().Be(0); // Pre-condition
        reconciliation.UpdateBalancesAndStatus(null,null, ReconciliationStatus.InProgress, null,null); // Set to InProgress first

        // Act
        reconciliation.CompleteReconciliation();

        // Assert
        reconciliation.Status.Should().Be(ReconciliationStatus.Completed);
    }

    [Fact]
    public void CompleteReconciliation_WhenDifferenceIsNotZero_Should_StillSetStatusToCompleted_AsPerCurrentDomainLogic()
    {
        // Arrange
        // The domain entity currently comments out the check for Difference != 0
        var reconciliation = CreateTestBankReconciliation(statementBalance: 1000m, systemBalance: 950m, status: ReconciliationStatus.InProgress); // Diff = 50
        reconciliation.Difference.Should().NotBe(0);

        // Act
        reconciliation.CompleteReconciliation();

        // Assert
        reconciliation.Status.Should().Be(ReconciliationStatus.Completed);
    }

    [Fact]
    public void CloseReconciliation_WhenCompleted_Should_SetStatusToClosed()
    {
        // Arrange
        var reconciliation = CreateTestBankReconciliation(status: ReconciliationStatus.InProgress);
        reconciliation.CompleteReconciliation(); // Status is now Completed

        // Act
        reconciliation.CloseReconciliation();

        // Assert
        reconciliation.Status.Should().Be(ReconciliationStatus.Closed);
    }

    [Fact]
    public void CloseReconciliation_WhenNotCompleted_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var reconciliation = CreateTestBankReconciliation(status: ReconciliationStatus.InProgress);

        // Act
        Action act = () => reconciliation.CloseReconciliation();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Reconciliation must be completed before it can be closed.");
    }
}
