using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Threading.Tasks;
using Mapster; // Required for Adapt

namespace FSH.WebApi.Application.Accounting.Suppliers;

public class GetSupplierHandler : IRequestHandler<GetSupplierRequest, SupplierDto>
{
    private readonly IRepository<Supplier> _repository;
    private readonly IStringLocalizer<GetSupplierHandler> _localizer;
    // private readonly IRepository<PaymentTerm> _paymentTermRepository; // If DefaultPaymentTermName is needed in DTO

    public GetSupplierHandler(IRepository<Supplier> repository, IStringLocalizer<GetSupplierHandler> localizer /*, IRepository<PaymentTerm> paymentTermRepository */)
    {
        _repository = repository;
        _localizer = localizer;
        // _paymentTermRepository = paymentTermRepository;
    }

    public async Task<SupplierDto> Handle(GetSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (supplier == null)
        {
            throw new NotFoundException(_localizer["Supplier not found."]);
        }

        var dto = supplier.Adapt<SupplierDto>();

        // If we need to populate DefaultPaymentTermName in the DTO:
        // if (supplier.DefaultPaymentTermId.HasValue)
        // {
        //     var paymentTerm = await _paymentTermRepository.GetByIdAsync(supplier.DefaultPaymentTermId.Value, cancellationToken);
        //     dto.DefaultPaymentTermName = paymentTerm?.Name; // Handle null paymentTerm if it can be deleted independently
        // }

        return dto;
    }
}
