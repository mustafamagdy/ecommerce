using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class CreateBankReconciliationRequest : IRequest<Guid>
{
    [Required]
    public Guid BankAccountId { get; set; }

    [Required]
    public DateTime ReconciliationDate { get; set; } // End date of period

    [Required]
    public Guid BankStatementId { get; set; } // ID of the bank statement to reconcile

    // StatementBalance will be taken from the BankStatement entity via BankStatementId.
    // SystemBalance (GL) will be calculated by the handler as of ReconciliationDate.
    // Initial status will be Draft or InProgress.
}
