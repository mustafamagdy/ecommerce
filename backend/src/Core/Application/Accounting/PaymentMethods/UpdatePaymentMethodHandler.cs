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

public class UpdatePaymentMethodHandler : IRequestHandler<UpdatePaymentMethodRequest, Guid>
{
    private readonly IRepository<PaymentMethod> _repository;
    private readonly IStringLocalizer<UpdatePaymentMethodHandler> _localizer;
    private readonly ILogger<UpdatePaymentMethodHandler> _logger;

    public UpdatePaymentMethodHandler(
        IRepository<PaymentMethod> repository,
        IStringLocalizer<UpdatePaymentMethodHandler> localizer,
        ILogger<UpdatePaymentMethodHandler> logger)
    {
        _repository = repository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(UpdatePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var paymentMethod = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (paymentMethod == null)
        {
            throw new NotFoundException(_localizer["Payment Method with ID {0} not found.", request.Id]);
        }

        if (request.Name is not null && !paymentMethod.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existingPaymentMethod = await _repository.FirstOrDefaultAsync(new PaymentMethodByNameSpec(request.Name), cancellationToken);
            if (existingPaymentMethod != null && existingPaymentMethod.Id != paymentMethod.Id)
            {
                throw new ConflictException(_localizer["A payment method with this name already exists."]);
            }
        }

        paymentMethod.Update(
            name: request.Name,
            description: request.Description,
            isActive: request.IsActive
        );

        await _repository.UpdateAsync(paymentMethod, cancellationToken);
        _logger.LogInformation(_localizer["Payment Method '{0}' (ID: {1}) updated."], paymentMethod.Name, paymentMethod.Id);
        return paymentMethod.Id;
    }
}
