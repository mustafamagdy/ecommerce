using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using System.Collections.Generic;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class SearchPaymentMethodsHandler : IRequestHandler<SearchPaymentMethodsRequest, PaginationResponse<PaymentMethodDto>>
{
    private readonly IReadRepository<PaymentMethod> _repository;

    public SearchPaymentMethodsHandler(IReadRepository<PaymentMethod> repository)
    {
        _repository = repository;
    }

    public async Task<PaginationResponse<PaymentMethodDto>> Handle(SearchPaymentMethodsRequest request, CancellationToken cancellationToken)
    {
        var spec = new PaymentMethodsBySearchFilterSpec(request);

        var paymentMethods = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);
        var dtos = paymentMethods.Adapt<List<PaymentMethodDto>>();

        return new PaginationResponse<PaymentMethodDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
