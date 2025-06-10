using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

public class CreatePaymentMethodHandler : IRequestHandler<CreatePaymentMethodRequest, Guid>
{
    private readonly IRepository<PaymentMethod> _repository;
    private readonly IStringLocalizer<CreatePaymentMethodHandler> _localizer;
    private readonly ILogger<CreatePaymentMethodHandler> _logger;

    public CreatePaymentMethodHandler(
        IRepository<PaymentMethod> repository,
        IStringLocalizer<CreatePaymentMethodHandler> localizer,
        ILogger<CreatePaymentMethodHandler> logger)
    {
        _repository = repository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreatePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var existingPaymentMethod = await _repository.FirstOrDefaultAsync(new PaymentMethodByNameSpec(request.Name), cancellationToken);
        if (existingPaymentMethod != null)
        {
            throw new ConflictException(_localizer["A payment method with this name already exists."]);
        }

        var paymentMethod = new PaymentMethod(
            name: request.Name,
            description: request.Description,
            isActive: request.IsActive
        );

        await _repository.AddAsync(paymentMethod, cancellationToken);
        _logger.LogInformation(_localizer["Payment Method '{0}' created."], paymentMethod.Name);
        return paymentMethod.Id;
    }
}
