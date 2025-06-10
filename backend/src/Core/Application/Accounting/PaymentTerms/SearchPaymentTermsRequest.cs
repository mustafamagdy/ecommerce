using FSH.WebApi.Application.Common.Models;
using MediatR;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class SearchPaymentTermsRequest : PaginationFilter, IRequest<PaginationResponse<PaymentTermDto>>
{
    public string? NameKeyword { get; set; } // Search by keyword in Name and Description
    public bool? IsActive { get; set; }
    public int? ExactDaysUntilDue { get; set; } // Example of specific filter
}
