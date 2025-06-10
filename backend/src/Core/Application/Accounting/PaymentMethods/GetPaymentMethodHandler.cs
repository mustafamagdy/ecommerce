using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class GetPaymentMethodHandler : IRequestHandler<GetPaymentMethodRequest, PaymentMethodDto>
{
    private readonly IReadRepository<PaymentMethod> _repository;
    private readonly IStringLocalizer<GetPaymentMethodHandler> _localizer;

    public GetPaymentMethodHandler(IReadRepository<PaymentMethod> repository, IStringLocalizer<GetPaymentMethodHandler> localizer)
    {
        _repository = repository;
        _localizer = localizer;
    }

    public async Task<PaymentMethodDto> Handle(GetPaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var paymentMethod = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (paymentMethod == null)
        {
            throw new NotFoundException(_localizer["Payment Method with ID {0} not found.", request.Id]);
        }

        return paymentMethod.Adapt<PaymentMethodDto>();
    }
}
