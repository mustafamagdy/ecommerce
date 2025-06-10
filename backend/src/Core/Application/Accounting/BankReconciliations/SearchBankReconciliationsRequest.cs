using FSH.WebApi.Application.Common.Models;
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class SearchBankReconciliationsRequest : PaginationFilter, IRequest<PaginationResponse<BankReconciliationDto>>
{
    public Guid? BankAccountId { get; set; }
    public DateTime? ReconciliationDateFrom { get; set; }
    public DateTime? ReconciliationDateTo { get; set; }
    public string? Status { get; set; } // Parsed to ReconciliationStatus enum
    public Guid? BankStatementId { get; set; }
}
