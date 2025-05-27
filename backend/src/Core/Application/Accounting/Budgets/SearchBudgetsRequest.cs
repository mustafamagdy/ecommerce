using FSH.WebApi.Application.Common.Models;
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.Budgets;

public class SearchBudgetsRequest : PaginationFilter, IRequest<PaginationResponse<BudgetDto>>
{
    // Keyword is already in PaginationFilter for BudgetName search
    public Guid? AccountId { get; set; }
    public DateTime? FromDate { get; set; } // To filter budgets whose periods overlap with this range
    public DateTime? ToDate { get; set; }   // To filter budgets whose periods overlap with this range
}
