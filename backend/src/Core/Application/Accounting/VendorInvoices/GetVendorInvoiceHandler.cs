using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Catalog; // For Product
using MediatR;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class GetVendorInvoiceHandler : IRequestHandler<GetVendorInvoiceRequest, VendorInvoiceDto>
{
    private readonly IReadRepository<VendorInvoice> _invoiceRepository; // Use IReadRepository for queries
    private readonly IReadRepository<Supplier> _supplierRepository;
    private readonly IReadRepository<Product>? _productRepository;
    private readonly IStringLocalizer<GetVendorInvoiceHandler> _localizer;

    public GetVendorInvoiceHandler(
        IReadRepository<VendorInvoice> invoiceRepository,
        IReadRepository<Supplier> supplierRepository,
        IStringLocalizer<GetVendorInvoiceHandler> localizer,
        IReadRepository<Product>? productRepository = null)
    {
        _invoiceRepository = invoiceRepository;
        _supplierRepository = supplierRepository;
        _localizer = localizer;
        _productRepository = productRepository;
    }

    public async Task<VendorInvoiceDto> Handle(GetVendorInvoiceRequest request, CancellationToken cancellationToken)
    {
        // Define a specification to include InvoiceItems
        var spec = new VendorInvoiceByIdWithItemsSpec(request.Id);
        var invoice = await _invoiceRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (invoice == null)
        {
            throw new NotFoundException(_localizer["Vendor Invoice not found."]);
        }

        var dto = invoice.Adapt<VendorInvoiceDto>();
        dto.Status = invoice.Status.ToString(); // Map enum to string

        // Populate SupplierName
        var supplier = await _supplierRepository.GetByIdAsync(invoice.SupplierId, cancellationToken);
        dto.SupplierName = supplier?.Name;

        // Populate InvoiceItems, including ProductName if possible
        dto.InvoiceItems = invoice.InvoiceItems.Adapt<List<VendorInvoiceItemDto>>();
        if (_productRepository != null)
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
        return dto;
    }
}

// Specification to ensure InvoiceItems are loaded with the VendorInvoice
public class VendorInvoiceByIdWithItemsSpec : Specification<VendorInvoice, VendorInvoiceDto>, ISingleResultSpecification
{
    public VendorInvoiceByIdWithItemsSpec(Guid invoiceId)
    {
        Query
            .Where(vi => vi.Id == invoiceId)
            .Include(vi => vi.InvoiceItems); // Ensure InvoiceItems are included
            // .Include(vi => vi.Supplier); // Optionally include Supplier if needed directly by spec
    }
}
