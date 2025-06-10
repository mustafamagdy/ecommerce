using FSH.WebApi.Application.Common.Models;
using MediatR;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class SearchPaymentMethodsRequest : PaginationFilter, IRequest<PaginationResponse<PaymentMethodDto>>
{
    public string? NameKeyword { get; set; }
    public bool? IsActive { get; set; }
}
