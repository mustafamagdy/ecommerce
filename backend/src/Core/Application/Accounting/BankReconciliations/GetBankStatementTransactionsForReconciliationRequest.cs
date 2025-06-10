using FSH.WebApi.Application.Common.Models; // For PaginationFilter, PaginationResponse
using FSH.WebApi.Application.Accounting.BankStatements; // For BankStatementTransactionDto
using FSH.WebApi.Domain.Accounting; // For BankTransactionType
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public enum ReconciliationTransactionFilterStatus
{
    All,
    Matched,
    Unmatched
}

public class GetBankStatementTransactionsForReconciliationRequest : PaginationFilter, IRequest<PaginationResponse<BankStatementTransactionDto>>
{
    [Required]
    public Guid BankReconciliationId { get; set; }

    public ReconciliationTransactionFilterStatus FilterStatus { get; set; } = ReconciliationTransactionFilterStatus.All;
    public BankTransactionType? TransactionType { get; set; } // Debit or Credit
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? DescriptionKeyword { get; set; }
    public decimal? ExactAmount { get; set; }
}
