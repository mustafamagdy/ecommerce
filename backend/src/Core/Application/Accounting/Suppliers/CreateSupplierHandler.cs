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
using Ardalis.Specification; // Required for ISpecification and potentially Specification

namespace FSH.WebApi.Application.Accounting.Suppliers;

// Specification to check for existing supplier by name (example for uniqueness check)
public class SupplierByNameSpec : Specification<Supplier>, ISingleResultSpecification
{
    public SupplierByNameSpec(string name) =>
        Query.Where(s => s.Name == name);
}

public class CreateSupplierHandler : IRequestHandler<CreateSupplierRequest, Guid>
{
    private readonly IRepository<Supplier> _repository;
    private readonly IStringLocalizer<CreateSupplierHandler> _localizer;
    private readonly ILogger<CreateSupplierHandler> _logger;
    // private readonly IRepository<PaymentTerm> _paymentTermRepository; // If validation for DefaultPaymentTermId is needed

    public CreateSupplierHandler(
        IRepository<Supplier> repository,
        IStringLocalizer<CreateSupplierHandler> localizer,
        ILogger<CreateSupplierHandler> logger
        /* IRepository<PaymentTerm> paymentTermRepository */)
    {
        _repository = repository;
        _localizer = localizer;
        _logger = logger;
        // _paymentTermRepository = paymentTermRepository;
    }

    public async Task<Guid> Handle(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        // Example: Check if a supplier with the same name already exists
        var existingSupplier = await _repository.FirstOrDefaultAsync(new SupplierByNameSpec(request.Name), cancellationToken);
        if (existingSupplier != null)
        {
            throw new ConflictException(_localizer["A supplier with this name already exists."]);
        }

        // Example: Validate DefaultPaymentTermId if provided
        // if (request.DefaultPaymentTermId.HasValue)
        // {
        //     var paymentTerm = await _paymentTermRepository.GetByIdAsync(request.DefaultPaymentTermId.Value, cancellationToken);
        //     if (paymentTerm == null)
        //     {
        //         throw new NotFoundException(_localizer["Payment term not found."]);
        //     }
        // }

        var supplier = new Supplier(
            name: request.Name,
            contactInfo: request.ContactInfo,
            address: request.Address,
            taxId: request.TaxId,
            defaultPaymentTermId: request.DefaultPaymentTermId,
            bankDetails: request.BankDetails
        );

        await _repository.AddAsync(supplier, cancellationToken);

        _logger.LogInformation(_localizer["Supplier created: {0}"], supplier.Id);

        return supplier.Id;
    }
}
