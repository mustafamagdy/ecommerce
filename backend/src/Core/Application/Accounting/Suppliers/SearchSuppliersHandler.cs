using FSH.WebApi.Application.Common.Interfaces;
using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.Suppliers;

public class SearchSuppliersHandler : IRequestHandler<SearchSuppliersRequest, PaginationResponse<SupplierDto>>
{
    private readonly IRepository<Supplier> _repository;
    private readonly IStringLocalizer<SearchSuppliersHandler> _localizer;
    // private readonly IRepository<PaymentTerm> _paymentTermRepository; // If DefaultPaymentTermName is needed

    public SearchSuppliersHandler(IRepository<Supplier> repository, IStringLocalizer<SearchSuppliersHandler> localizer /*, IRepository<PaymentTerm> paymentTermRepository */)
    {
        _repository = repository;
        _localizer = localizer;
        // _paymentTermRepository = paymentTermRepository;
    }

    public async Task<PaginationResponse<SupplierDto>> Handle(SearchSuppliersRequest request, CancellationToken cancellationToken)
    {
        var spec = new SuppliersBySearchFilterSpec(request);
        var suppliers = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var dtos = suppliers.Adapt<List<SupplierDto>>();

        // If DefaultPaymentTermName needs to be populated for each DTO in the list
        // This could be inefficient if many suppliers are returned.
        // Consider optimizing if this becomes a performance bottleneck (e.g., joining in the spec or a more specific DTO).
        // foreach (var dto in dtos)
        // {
        //     if (dto.DefaultPaymentTermId.HasValue)
        //     {
        //         var supplierEntity = suppliers.FirstOrDefault(s => s.Id == dto.Id); // Get corresponding entity
        //         if (supplierEntity?.DefaultPaymentTermId.HasValue ?? false)
        //         {
        //             var paymentTerm = await _paymentTermRepository.GetByIdAsync(supplierEntity.DefaultPaymentTermId.Value, cancellationToken);
        //             dto.DefaultPaymentTermName = paymentTerm?.Name;
        //         }
        //     }
        // }

        return new PaginationResponse<SupplierDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
