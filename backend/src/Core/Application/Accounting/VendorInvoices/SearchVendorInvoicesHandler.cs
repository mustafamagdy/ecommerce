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
using FSH.WebApi.Domain.Catalog; // For Product, if needed for DTO population
using System.Linq; // For FirstOrDefault

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class SearchVendorInvoicesHandler : IRequestHandler<SearchVendorInvoicesRequest, PaginationResponse<VendorInvoiceDto>>
{
    private readonly IReadRepository<VendorInvoice> _invoiceRepository;
    private readonly IReadRepository<Supplier> _supplierRepository; // For populating SupplierName
    private readonly IReadRepository<Product>? _productRepository;   // For populating ProductName in items
    private readonly IStringLocalizer<SearchVendorInvoicesHandler> _localizer;

    public SearchVendorInvoicesHandler(
        IReadRepository<VendorInvoice> invoiceRepository,
        IReadRepository<Supplier> supplierRepository,
        IStringLocalizer<SearchVendorInvoicesHandler> localizer,
        IReadRepository<Product>? productRepository = null)
    {
        _invoiceRepository = invoiceRepository;
        _supplierRepository = supplierRepository;
        _localizer = localizer;
        _productRepository = productRepository;
    }

    public async Task<PaginationResponse<VendorInvoiceDto>> Handle(SearchVendorInvoicesRequest request, CancellationToken cancellationToken)
    {
        var spec = new VendorInvoicesBySearchFilterSpec(request); // Assumes this spec also handles includes for items

        // Modify spec to include items for DTO mapping, if not already handled by default in EntitiesByPaginationFilterSpec or VendorInvoicesBySearchFilterSpec
        // A common way is to have a separate spec for list results that includes necessary children for the DTO.
        // For now, let's assume VendorInvoicesBySearchFilterSpec might need adjustment or we create a new one.
        // Or, we fetch minimal data and then enrich DTOs one by one (less efficient for lists).

        // Let's try to adapt the existing spec to include InvoiceItems
        // This might not be the cleanest way if EntitiesByPaginationFilterSpec doesn't directly support this.
        // A better way is to have a specific constructor or method in VendorInvoicesBySearchFilterSpec
        // or create a new spec like VendorInvoicesWithItemsSearchSpec.
        spec.Query.Include(i => i.InvoiceItems); // Ensure items are loaded for mapping

        var invoices = await _invoiceRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _invoiceRepository.CountAsync(spec, cancellationToken); // Use the same spec for count

        var dtos = new List<VendorInvoiceDto>();
        foreach (var invoice in invoices)
        {
            var dto = invoice.Adapt<VendorInvoiceDto>();
            dto.Status = invoice.Status.ToString();

            // Populate SupplierName
            var supplier = await _supplierRepository.GetByIdAsync(invoice.SupplierId, cancellationToken);
            dto.SupplierName = supplier?.Name;

            // Populate InvoiceItems' ProductName if _productRepository is available
            if (_productRepository != null && invoice.InvoiceItems.Any())
            {
                foreach (var itemDto in dto.InvoiceItems)
                {
                    var itemEntity = invoice.InvoiceItems.FirstOrDefault(i => i.Id == itemDto.Id);
                    if (itemEntity?.ProductId.HasValue == true)
                    {
                        var product = await _productRepository.GetByIdAsync(itemEntity.ProductId.Value, cancellationToken);
                        itemDto.ProductName = product?.Name;
                    }
                }
            }
            dtos.Add(dto);
        }

        return new PaginationResponse<VendorInvoiceDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}

// It's important that VendorInvoicesBySearchFilterSpec is correctly defined.
// It was created in a previous step. It should inherit from EntitiesByPaginationFilterSpec<VendorInvoice, VendorInvoiceDto>.
// The Query.Search for keyword across Supplier.Name might need specific handling in the spec itself,
// e.g., by joining with Suppliers or using a view model that already has SupplierName.
// For now, the spec previously created searches on VendorInvoice.InvoiceNumber.
// Adding .Include(i => i.InvoiceItems) in the handler is a pragmatic way if the spec doesn't do it.```tool_code
create_file_with_block
backend/src/Core/Application/Accounting/VendorInvoices/SearchVendorInvoicesHandler.cs
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
using FSH.WebApi.Domain.Catalog; // For Product
using System.Linq; // For FirstOrDefault, Any
using Ardalis.Specification; // For ISpecification

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class SearchVendorInvoicesHandler : IRequestHandler<SearchVendorInvoicesRequest, PaginationResponse<VendorInvoiceDto>>
{
    private readonly IReadRepository<VendorInvoice> _invoiceRepository;
    private readonly IReadRepository<Supplier> _supplierRepository;
    private readonly IReadRepository<Product>? _productRepository;
    private readonly IStringLocalizer<SearchVendorInvoicesHandler> _localizer;

    public SearchVendorInvoicesHandler(
        IReadRepository<VendorInvoice> invoiceRepository,
        IReadRepository<Supplier> supplierRepository,
        IStringLocalizer<SearchVendorInvoicesHandler> localizer,
        IReadRepository<Product>? productRepository = null)
    {
        _invoiceRepository = invoiceRepository;
        _supplierRepository = supplierRepository;
        _localizer = localizer;
        _productRepository = productRepository;
    }

    public async Task<PaginationResponse<VendorInvoiceDto>> Handle(SearchVendorInvoicesRequest request, CancellationToken cancellationToken)
    {
        // Use the existing spec, but ensure it includes items for DTO mapping.
        // A cleaner way would be a dedicated spec for this search that includes items.
        var spec = new VendorInvoicesBySearchWithItemsFilterSpec(request);

        var invoices = await _invoiceRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _invoiceRepository.CountAsync(spec, cancellationToken);

        var dtos = new List<VendorInvoiceDto>();
        foreach (var invoice in invoices)
        {
            var dto = invoice.Adapt<VendorInvoiceDto>();
            dto.Status = invoice.Status.ToString();

            var supplier = await _supplierRepository.GetByIdAsync(invoice.SupplierId, cancellationToken);
            dto.SupplierName = supplier?.Name;

            if (_productRepository != null && invoice.InvoiceItems.Any())
            {
                foreach (var itemDto in dto.InvoiceItems)
                {
                    var itemEntity = invoice.InvoiceItems.FirstOrDefault(i => i.Id == itemDto.Id);
                    if (itemEntity?.ProductId.HasValue == true)
                    {
                        var product = await _productRepository.GetByIdAsync(itemEntity.ProductId.Value, cancellationToken);
                        itemDto.ProductName = product?.Name;
                    }
                }
            }
            dtos.Add(dto);
        }

        return new PaginationResponse<VendorInvoiceDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}

// Create a new spec that inherits from VendorInvoicesBySearchFilterSpec but also includes items.
// This is cleaner than modifying the query in the handler.
public class VendorInvoicesBySearchWithItemsFilterSpec : EntitiesByPaginationFilterSpec<VendorInvoice, VendorInvoiceDto>
{
    public VendorInvoicesBySearchWithItemsFilterSpec(SearchVendorInvoicesRequest request)
        : base(request) // Call the base constructor that sets up pagination
    {
        // Apply filtering logic from VendorInvoicesBySearchFilterSpec
        // This duplicates logic from VendorInvoicesBySearchFilterSpec. Ideally, reuse it.
        // One way: VendorInvoicesBySearchFilterSpec could have a constructor that takes an IQueryable
        // or have its filtering logic in a protected method. For now, duplicating relevant parts.

        Query.OrderByDescending(vi => vi.InvoiceDate, !request.HasOrderBy());

        if (request.SupplierId.HasValue)
        {
            Query.Where(vi => vi.SupplierId == request.SupplierId.Value);
        }

        if (!string.IsNullOrEmpty(request.InvoiceStatus))
        {
            if (Enum.TryParse<VendorInvoiceStatus>(request.InvoiceStatus, true, out var statusEnum))
            {
                Query.Where(vi => vi.Status == statusEnum);
            }
        }
        if (request.DateFrom.HasValue) Query.Where(vi => vi.InvoiceDate >= request.DateFrom.Value);
        if (request.DateTo.HasValue) Query.Where(vi => vi.InvoiceDate <= request.DateTo.Value.AddDays(1).AddTicks(-1));
        if (request.DueDateFrom.HasValue) Query.Where(vi => vi.DueDate >= request.DueDateFrom.Value);
        if (request.DueDateTo.HasValue) Query.Where(vi => vi.DueDate <= request.DueDateTo.Value.AddDays(1).AddTicks(-1));

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            Query.Search(vi => vi.InvoiceNumber, "%" + request.Keyword + "%");
            // Note: Searching by Supplier.Name in this spec would require a join strategy.
            // The base EntitiesByPaginationFilterSpec might not support joins directly in its Search extension.
        }

        // Crucially, include the items
        Query.Include(i => i.InvoiceItems);
    }
}
