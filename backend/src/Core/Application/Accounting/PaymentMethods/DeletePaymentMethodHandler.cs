using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;

namespace FSH.WebApi.Application.Accounting.PaymentMethods;

// Specification to check if any vendor payment uses this payment method
public class VendorPaymentsByPaymentMethodSpec : Specification<VendorPayment>, ISingleResultSpecification
{
    public VendorPaymentsByPaymentMethodSpec(Guid paymentMethodId) =>
        Query.Where(vp => vp.PaymentMethodId == paymentMethodId);
}

public class DeletePaymentMethodHandler : IRequestHandler<DeletePaymentMethodRequest, Guid>
{
    private readonly IRepository<PaymentMethod> _paymentMethodRepository;
    private readonly IReadRepository<VendorPayment> _vendorPaymentRepository;
    private readonly IStringLocalizer<DeletePaymentMethodHandler> _localizer;
    private readonly ILogger<DeletePaymentMethodHandler> _logger;

    public DeletePaymentMethodHandler(
        IRepository<PaymentMethod> paymentMethodRepository,
        IReadRepository<VendorPayment> vendorPaymentRepository,
        IStringLocalizer<DeletePaymentMethodHandler> localizer,
        ILogger<DeletePaymentMethodHandler> logger)
    {
        _paymentMethodRepository = paymentMethodRepository;
        _vendorPaymentRepository = vendorPaymentRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(DeletePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(request.Id, cancellationToken);
        if (paymentMethod == null)
        {
            throw new NotFoundException(_localizer["Payment Method with ID {0} not found.", request.Id]);
        }

        var spec = new VendorPaymentsByPaymentMethodSpec(request.Id);
        bool paymentMethodInUse = await _vendorPaymentRepository.AnyAsync(spec, cancellationToken);
        if (paymentMethodInUse)
        {
            throw new ConflictException(_localizer["Payment Method '{0}' is in use by one or more vendor payments and cannot be deleted.", paymentMethod.Name]);
        }

        await _paymentMethodRepository.DeleteAsync(paymentMethod, cancellationToken);
        _logger.LogInformation(_localizer["Payment Method '{0}' (ID: {1}) deleted."], paymentMethod.Name, paymentMethod.Id);
        return paymentMethod.Id;
    }
}
