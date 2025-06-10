using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Mapster; // For ProjectToType for efficient querying if IRepository supports IQueryable
using System.Collections.Generic; // For List

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class SearchPaymentTermsHandler : IRequestHandler<SearchPaymentTermsRequest, PaginationResponse<PaymentTermDto>>
{
    private readonly IReadRepository<PaymentTerm> _repository;

    public SearchPaymentTermsHandler(IReadRepository<PaymentTerm> repository)
    {
        _repository = repository;
    }

    public async Task<PaginationResponse<PaymentTermDto>> Handle(SearchPaymentTermsRequest request, CancellationToken cancellationToken)
    {
        var spec = new PaymentTermsBySearchFilterSpec(request);

        // Using .ListAsync and then .Adapt is common if no complex mappings or projections are needed from DB
        var paymentTerms = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);
        var dtos = paymentTerms.Adapt<List<PaymentTermDto>>();

        return new PaginationResponse<PaymentTermDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
