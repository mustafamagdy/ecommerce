using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Operation.Customers; // For Customer
using FSH.WebApi.Domain.Operation.Orders;   // For Order
using FSH.WebApi.Domain.Catalog;           // For Product
using MediatR;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class GetCustomerInvoiceHandler : IRequestHandler<GetCustomerInvoiceRequest, CustomerInvoiceDto>
{
    private readonly IReadRepository<CustomerInvoice> _invoiceRepository;
    private readonly IReadRepository<Customer>? _customerRepository; // Optional, depends on if Customer is in same DB
    private readonly IReadRepository<Order>? _orderRepository;       // Optional
    private readonly IReadRepository<Product>? _productRepository;   // Optional
    private readonly IStringLocalizer<GetCustomerInvoiceHandler> _localizer;

    public GetCustomerInvoiceHandler(
        IReadRepository<CustomerInvoice> invoiceRepository,
        IStringLocalizer<GetCustomerInvoiceHandler> localizer,
        IReadRepository<Customer>? customerRepository = null,
        IReadRepository<Order>? orderRepository = null,
        IReadRepository<Product>? productRepository = null)
    {
        _invoiceRepository = invoiceRepository;
        _localizer = localizer;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<CustomerInvoiceDto> Handle(GetCustomerInvoiceRequest request, CancellationToken cancellationToken)
    {
        var spec = new CustomerInvoiceByIdWithDetailsSpec(request.Id); // Includes InvoiceItems
        var invoice = await _invoiceRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (invoice == null)
            throw new NotFoundException(_localizer["Customer Invoice with ID {0} not found.", request.Id]);

        var dto = invoice.Adapt<CustomerInvoiceDto>();
        dto.Status = invoice.Status.ToString(); // Map enum to string

        // Populate CustomerName if repository is available
        if (_customerRepository != null)
        {
            var customer = await _customerRepository.GetByIdAsync(invoice.CustomerId, cancellationToken);
            dto.CustomerName = customer?.Name; // Assuming Customer has a Name property
        }

        // Populate OrderNumber if OrderId exists and repository is available
        if (invoice.OrderId.HasValue && _orderRepository != null)
        {
            var order = await _orderRepository.GetByIdAsync(invoice.OrderId.Value, cancellationToken);
            dto.OrderNumber = order?.OrderNumber; // Assuming Order has an OrderNumber property
        }

        // Populate ProductName for each item if repository is available
        if (_productRepository != null && dto.InvoiceItems.Any())
        {
            foreach (var itemDto in dto.InvoiceItems)
            {
                var itemEntity = invoice.InvoiceItems.FirstOrDefault(i => i.Id == itemDto.Id);
                if (itemEntity?.ProductId.HasValue == true)
                {
                    var product = await _productRepository.GetByIdAsync(itemEntity.ProductId.Value, cancellationToken);
                    itemDto.ProductName = product?.Name; // Assuming Product has a Name property
                }
            }
        }

        return dto;
    }
}
