using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using FSH.WebApi.Domain.Accounting; // For ReconciliationStatus

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class UpdateBankReconciliationRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; } // ID of the BankReconciliation to update

    // Fields that might be updated during an "in-progress" reconciliation
    public decimal? ManuallyAssignedUnclearedChecks { get; set; }
    public decimal? ManuallyAssignedDepositsInTransit { get; set; }

    // Typically status changes are specific actions, but a general update might be needed for some cases
    public ReconciliationStatus? Status { get; set; } // E.g. moving to PendingReview, or back to Draft from InProgress

    // ReconciliationDate, BankAccountId, BankStatementId, StatementBalance, SystemBalance are generally not updated directly
    // after creation, but rather through specific processes or re-creation if fundamental aspects change.
}
