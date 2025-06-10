using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

// Returns the ID of the BankStatementTransaction that was matched/unmatched
public class MatchBankTransactionRequest : IRequest<Guid>
{
    [Required]
    public Guid BankReconciliationId { get; set; } // Context for this matching operation

    [Required]
    public Guid BankStatementTransactionId { get; set; }

    // If un-matching, these can be null/empty
    public Guid? SystemTransactionId { get; set; }

    [MaxLength(50)] // E.g., "VendorPayment", "CustomerPayment", "JournalEntry"
    public string? SystemTransactionType { get; set; }

    [Required]
    public bool IsMatched { get; set; } // true to match, false to unmatch
}
