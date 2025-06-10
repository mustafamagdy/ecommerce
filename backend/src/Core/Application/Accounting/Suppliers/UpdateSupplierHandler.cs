using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.Suppliers;

public class UpdateSupplierHandler : IRequestHandler<UpdateSupplierRequest, Guid>
{
    private readonly IRepository<Supplier> _repository;
    private readonly IStringLocalizer<UpdateSupplierHandler> _localizer;
    private readonly ILogger<UpdateSupplierHandler> _logger;
    // private readonly IRepository<PaymentTerm> _paymentTermRepository; // If validation for DefaultPaymentTermId is needed

    public UpdateSupplierHandler(
        IRepository<Supplier> repository,
        IStringLocalizer<UpdateSupplierHandler> localizer,
        ILogger<UpdateSupplierHandler> logger
        /* IRepository<PaymentTerm> paymentTermRepository */)
    {
        _repository = repository;
        _localizer = localizer;
        _logger = logger;
        // _paymentTermRepository = paymentTermRepository;
    }

    public async Task<Guid> Handle(UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (supplier == null)
        {
            throw new NotFoundException(_localizer["Supplier not found."]);
        }

        // Example: Validate DefaultPaymentTermId if provided and changed
        // if (request.DefaultPaymentTermId.HasValue && supplier.DefaultPaymentTermId != request.DefaultPaymentTermId.Value)
        // {
        //     var paymentTerm = await _paymentTermRepository.GetByIdAsync(request.DefaultPaymentTermId.Value, cancellationToken);
        //     if (paymentTerm == null)
        //     {
        //         throw new NotFoundException(_localizer["Payment term not found."]);
        //     }
        // }
        // else if (!request.DefaultPaymentTermId.HasValue && supplier.DefaultPaymentTermId.HasValue) // Clearing the payment term
        // {
        //    // Allow clearing if business logic permits
        // }


        // Example: Check for duplicate name if it's being changed
        if (request.Name is not null && !supplier.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existingSupplier = await _repository.FirstOrDefaultAsync(new SupplierByNameSpec(request.Name), cancellationToken);
            if (existingSupplier != null && existingSupplier.Id != supplier.Id)
            {
                throw new ConflictException(_localizer["A supplier with this name already exists."]);
            }
        }

        supplier.Update(
            name: request.Name,
            contactInfo: request.ContactInfo,
            address: request.Address,
            taxId: request.TaxId,
            defaultPaymentTermId: request.DefaultPaymentTermId,
            bankDetails: request.BankDetails
        );

        await _repository.UpdateAsync(supplier, cancellationToken);

        _logger.LogInformation(_localizer["Supplier updated: {0}"], supplier.Id);

        return supplier.Id;
    }
}
