using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class GetPaymentTermHandler : IRequestHandler<GetPaymentTermRequest, PaymentTermDto>
{
    private readonly IReadRepository<PaymentTerm> _repository; // Use IReadRepository for queries
    private readonly IStringLocalizer<GetPaymentTermHandler> _localizer;

    public GetPaymentTermHandler(IReadRepository<PaymentTerm> repository, IStringLocalizer<GetPaymentTermHandler> localizer)
    {
        _repository = repository;
        _localizer = localizer;
    }

    public async Task<PaymentTermDto> Handle(GetPaymentTermRequest request, CancellationToken cancellationToken)
    {
        var paymentTerm = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (paymentTerm == null)
        {
            throw new NotFoundException(_localizer["Payment Term with ID {0} not found.", request.Id]);
        }

        return paymentTerm.Adapt<PaymentTermDto>();
    }
}
