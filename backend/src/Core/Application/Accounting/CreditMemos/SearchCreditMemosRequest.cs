using FSH.WebApi.Application.Common.Models;
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class SearchCreditMemosRequest : PaginationFilter, IRequest<PaginationResponse<CreditMemoDto>>
{
    public Guid? CustomerId { get; set; }
    public string? CreditMemoNumberKeyword { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Status { get; set; } // Parsed to CreditMemoStatus enum in handler/spec
    public decimal? MinimumTotalAmount { get; set; }
    public decimal? MaximumTotalAmount { get; set; }
    public Guid? OriginalCustomerInvoiceId { get; set; }
    public bool? HasAvailableBalance { get; set; } // Filter for credit memos with available balance
}
