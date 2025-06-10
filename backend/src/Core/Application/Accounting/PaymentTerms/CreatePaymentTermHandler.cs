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

public class CreatePaymentTermHandler : IRequestHandler<CreatePaymentTermRequest, Guid>
{
    private readonly IRepository<PaymentTerm> _repository;
    private readonly IStringLocalizer<CreatePaymentTermHandler> _localizer;
    private readonly ILogger<CreatePaymentTermHandler> _logger;

    public CreatePaymentTermHandler(
        IRepository<PaymentTerm> repository,
        IStringLocalizer<CreatePaymentTermHandler> localizer,
        ILogger<CreatePaymentTermHandler> logger)
    {
        _repository = repository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreatePaymentTermRequest request, CancellationToken cancellationToken)
    {
        // Check for duplicate name
        var existingPaymentTerm = await _repository.FirstOrDefaultAsync(new PaymentTermByNameSpec(request.Name), cancellationToken);
        if (existingPaymentTerm != null)
        {
            throw new ConflictException(_localizer["A payment term with this name already exists."]);
        }

        var paymentTerm = new PaymentTerm(
            name: request.Name,
            daysUntilDue: request.DaysUntilDue,
            description: request.Description,
            isActive: request.IsActive
        );

        await _repository.AddAsync(paymentTerm, cancellationToken);
        _logger.LogInformation(_localizer["Payment Term '{0}' created."], paymentTerm.Name);
        return paymentTerm.Id;
    }
}
