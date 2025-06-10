using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class UpdatePaymentTermHandler : IRequestHandler<UpdatePaymentTermRequest, Guid>
{
    private readonly IRepository<PaymentTerm> _repository;
    private readonly IStringLocalizer<UpdatePaymentTermHandler> _localizer;
    private readonly ILogger<UpdatePaymentTermHandler> _logger;

    public UpdatePaymentTermHandler(
        IRepository<PaymentTerm> repository,
        IStringLocalizer<UpdatePaymentTermHandler> localizer,
        ILogger<UpdatePaymentTermHandler> logger)
    {
        _repository = repository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(UpdatePaymentTermRequest request, CancellationToken cancellationToken)
    {
        var paymentTerm = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (paymentTerm == null)
        {
            throw new NotFoundException(_localizer["Payment Term with ID {0} not found.", request.Id]);
        }

        // Check for duplicate name if name is being changed
        if (request.Name is not null && !paymentTerm.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existingPaymentTerm = await _repository.FirstOrDefaultAsync(new PaymentTermByNameSpec(request.Name), cancellationToken);
            if (existingPaymentTerm != null && existingPaymentTerm.Id != paymentTerm.Id)
            {
                throw new ConflictException(_localizer["A payment term with this name already exists."]);
            }
        }

        paymentTerm.Update(
            name: request.Name,
            daysUntilDue: request.DaysUntilDue,
            description: request.Description,
            isActive: request.IsActive
        );

        await _repository.UpdateAsync(paymentTerm, cancellationToken);
        _logger.LogInformation(_localizer["Payment Term '{0}' (ID: {1}) updated."], paymentTerm.Name, paymentTerm.Id);
        return paymentTerm.Id;
    }
}
