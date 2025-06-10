using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification; // Required for Specification

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

// Specification to check if any supplier uses this payment term
public class SuppliersByPaymentTermSpec : Specification<Supplier>, ISingleResultSpecification // ISingleResultSpecification for ExistsAsync
{
    public SuppliersByPaymentTermSpec(Guid paymentTermId) =>
        Query.Where(s => s.DefaultPaymentTermId == paymentTermId);
}

public class DeletePaymentTermHandler : IRequestHandler<DeletePaymentTermRequest, Guid>
{
    private readonly IRepository<PaymentTerm> _paymentTermRepository;
    private readonly IReadRepository<Supplier> _supplierRepository; // Using IReadRepository as we only check for existence
    private readonly IStringLocalizer<DeletePaymentTermHandler> _localizer;
    private readonly ILogger<DeletePaymentTermHandler> _logger;

    public DeletePaymentTermHandler(
        IRepository<PaymentTerm> paymentTermRepository,
        IReadRepository<Supplier> supplierRepository,
        IStringLocalizer<DeletePaymentTermHandler> localizer,
        ILogger<DeletePaymentTermHandler> logger)
    {
        _paymentTermRepository = paymentTermRepository;
        _supplierRepository = supplierRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(DeletePaymentTermRequest request, CancellationToken cancellationToken)
    {
        var paymentTerm = await _paymentTermRepository.GetByIdAsync(request.Id, cancellationToken);
        if (paymentTerm == null)
        {
            throw new NotFoundException(_localizer["Payment Term with ID {0} not found.", request.Id]);
        }

        // Check if any supplier is using this payment term
        var spec = new SuppliersByPaymentTermSpec(request.Id);
        bool paymentTermInUse = await _supplierRepository.AnyAsync(spec, cancellationToken);
        if (paymentTermInUse)
        {
            throw new ConflictException(_localizer["Payment Term '{0}' is in use by one or more suppliers and cannot be deleted.", paymentTerm.Name]);
        }

        // Optional: Business rule to prevent deletion of "active" payment terms
        // if (paymentTerm.IsActive)
        // {
        //    throw new ConflictException(_localizer["Active Payment Term '{0}' cannot be deleted. Please deactivate it first.", paymentTerm.Name]);
        // }


        await _paymentTermRepository.DeleteAsync(paymentTerm, cancellationToken);
        _logger.LogInformation(_localizer["Payment Term '{0}' (ID: {1}) deleted."], paymentTerm.Name, paymentTerm.Id);
        return paymentTerm.Id;
    }
}
