using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Operation.Customers; // For Customer
using FSH.WebApi.Domain.Catalog;           // For Product
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using Microsoft.Extensions.Localization; // Added for _localizer

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class SearchCustomerInvoicesHandler : IRequestHandler<SearchCustomerInvoicesRequest, PaginationResponse<CustomerInvoiceDto>>
{
    private readonly IReadRepository<CustomerInvoice> _invoiceRepository;
    private readonly IReadRepository<Customer>? _customerRepository; // Optional
    private readonly IReadRepository<Product>? _productRepository;   // Optional
    private readonly IStringLocalizer<SearchCustomerInvoicesHandler> _localizer; // Added

    public SearchCustomerInvoicesHandler(
        IReadRepository<CustomerInvoice> invoiceRepository,
        IStringLocalizer<SearchCustomerInvoicesHandler> localizer, // Added
        IReadRepository<Customer>? customerRepository = null,
        IReadRepository<Product>? productRepository = null)
    {
        _invoiceRepository = invoiceRepository;
        _localizer = localizer; // Added
        _customerRepository = customerRepository;
        _productRepository = productRepository;
    }

    public async Task<PaginationResponse<CustomerInvoiceDto>> Handle(SearchCustomerInvoicesRequest request, CancellationToken cancellationToken)
    {
        var spec = new CustomerInvoicesBySearchFilterSpec(request); // This spec includes InvoiceItems

        var invoices = await _invoiceRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _invoiceRepository.CountAsync(spec, cancellationToken);

        var dtos = new List<CustomerInvoiceDto>();
        foreach (var invoice in invoices)
        {
            var dto = invoice.Adapt<CustomerInvoiceDto>();
            dto.Status = invoice.Status.ToString();

            if (_customerRepository != null)
            {
                var customer = await _customerRepository.GetByIdAsync(invoice.CustomerId, cancellationToken);
                dto.CustomerName = customer?.Name;
            }

            // OrderNumber is not directly on CustomerInvoice, if OrderId is present, it could be fetched
            // but SearchCustomerInvoicesRequest doesn't filter by OrderNumber directly, only OrderId.
            // For list view, OrderNumber might be less critical or fetched on demand if OrderId is shown.
            // If an Order repository was available, OrderNumber could be populated if OrderId exists.

            if (_productRepository != null && dto.InvoiceItems.Any())
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

        return new PaginationResponse<CustomerInvoiceDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
