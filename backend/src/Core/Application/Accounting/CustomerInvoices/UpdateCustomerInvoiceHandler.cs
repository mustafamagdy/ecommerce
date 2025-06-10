using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Catalog; // For Product
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class UpdateCustomerInvoiceHandler : IRequestHandler<UpdateCustomerInvoiceRequest, Guid>
{
    private readonly IRepository<CustomerInvoice> _invoiceRepository;
    private readonly IReadRepository<Product>? _productRepository; // Optional, for validating products in items
    private readonly IStringLocalizer<UpdateCustomerInvoiceHandler> _localizer;
    private readonly ILogger<UpdateCustomerInvoiceHandler> _logger;

    public UpdateCustomerInvoiceHandler(
        IRepository<CustomerInvoice> invoiceRepository,
        IStringLocalizer<UpdateCustomerInvoiceHandler> localizer,
        ILogger<UpdateCustomerInvoiceHandler> logger,
        IReadRepository<Product>? productRepository = null)
    {
        _invoiceRepository = invoiceRepository;
        _localizer = localizer;
        _logger = logger;
        _productRepository = productRepository;
    }

    public async Task<Guid> Handle(UpdateCustomerInvoiceRequest request, CancellationToken cancellationToken)
    {
        var spec = new CustomerInvoiceByIdWithDetailsSpec(request.Id); // To include items
        var invoice = await _invoiceRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (invoice == null)
            throw new NotFoundException(_localizer["Customer Invoice with ID {0} not found.", request.Id]);

        // Business rule: Prevent updates if invoice is in a "final" state (e.g., Paid or Void)
        if (invoice.Status == CustomerInvoiceStatus.Paid || invoice.Status == CustomerInvoiceStatus.Void)
        {
            throw new ConflictException(_localizer["Cannot update an invoice that is already {0}.", invoice.Status]);
        }

        // Update scalar properties from request
        invoice.Update(
            invoiceDate: request.InvoiceDate,
            dueDate: request.DueDate,
            currency: request.Currency,
            notes: request.Notes,
            status: null // Status updates should be via specific actions, or carefully considered here
        );

        // Handle Invoice Items if provided in the request
        if (request.InvoiceItems != null)
        {
            // Remove items not present in the request's item list
            var itemsToRemove = invoice.InvoiceItems
                .Where(ei => !request.InvoiceItems.Any(ri => ri.Id.HasValue && ri.Id.Value == ei.Id))
                .ToList(); // Materialize to avoid issues with collection modification

            foreach (var itemToRemove in itemsToRemove)
            {
                invoice.RemoveInvoiceItem(itemToRemove.Id);
            }

            // Update existing items and add new ones
            foreach (var itemRequest in request.InvoiceItems)
            {
                if (itemRequest.Id.HasValue && itemRequest.Id.Value != Guid.Empty) // Existing item
                {
                    var existingItem = invoice.InvoiceItems.FirstOrDefault(i => i.Id == itemRequest.Id.Value);
                    if (existingItem == null)
                        throw new NotFoundException(_localizer["Invoice item with ID {0} not found on invoice {1}.", itemRequest.Id.Value, invoice.Id]);

                    if (itemRequest.ProductId.HasValue && itemRequest.ProductId != existingItem.ProductId && _productRepository != null)
                    {
                        var product = await _productRepository.GetByIdAsync(itemRequest.ProductId.Value, cancellationToken);
                        if (product == null) throw new NotFoundException(_localizer["Product with ID {0} not found.", itemRequest.ProductId.Value]);
                    }

                    existingItem.Update(
                        description: itemRequest.Description,
                        quantity: itemRequest.Quantity,
                        unitPrice: itemRequest.UnitPrice,
                        taxAmount: itemRequest.TaxAmount,
                        productId: itemRequest.ProductId
                    );
                }
                else // New item
                {
                    if (string.IsNullOrWhiteSpace(itemRequest.Description) || !itemRequest.Quantity.HasValue || !itemRequest.UnitPrice.HasValue)
                        throw new ValidationException(_localizer["For new invoice items, Description, Quantity, and UnitPrice are required."]);

                    if (itemRequest.ProductId.HasValue && _productRepository != null)
                    {
                        var product = await _productRepository.GetByIdAsync(itemRequest.ProductId.Value, cancellationToken);
                        if (product == null) throw new NotFoundException(_localizer["Product with ID {0} not found.", itemRequest.ProductId.Value]);
                    }

                    invoice.AddInvoiceItem(
                        description: itemRequest.Description!,
                        quantity: itemRequest.Quantity!.Value,
                        unitPrice: itemRequest.UnitPrice!.Value,
                        taxAmount: itemRequest.TaxAmount ?? 0,
                        productId: itemRequest.ProductId
                    );
                }
            }
            // RecalculateTotalAmount is called by AddInvoiceItem/RemoveInvoiceItem
        }

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        _logger.LogInformation(_localizer["Customer Invoice {0} updated."], invoice.InvoiceNumber);
        return invoice.Id;
    }
}
