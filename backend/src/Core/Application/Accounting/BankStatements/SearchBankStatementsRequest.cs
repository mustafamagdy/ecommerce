using FSH.WebApi.Application.Common.Models;
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.BankStatements;

public class SearchBankStatementsRequest : PaginationFilter, IRequest<PaginationResponse<BankStatementDto>>
{
    public Guid? BankAccountId { get; set; }
    public DateTime? StatementDateFrom { get; set; }
    public DateTime? StatementDateTo { get; set; }
    public string? ReferenceNumberKeyword { get; set; }
    public DateTime? ImportDateFrom { get; set; }
    public DateTime? ImportDateTo { get; set; }
}
