using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Catalog; // For Product, if needed for item validation/linking
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class UpdateVendorInvoiceHandler : IRequestHandler<UpdateVendorInvoiceRequest, Guid>
{
    private readonly IRepository<VendorInvoice> _invoiceRepository;
    private readonly IRepository<Supplier> _supplierRepository; // To validate supplier if changed
    private readonly IRepository<Product>? _productRepository;   // To validate products if provided for items
    private readonly IStringLocalizer<UpdateVendorInvoiceHandler> _localizer;
    private readonly ILogger<UpdateVendorInvoiceHandler> _logger;

    public UpdateVendorInvoiceHandler(
        IRepository<VendorInvoice> invoiceRepository,
        IRepository<Supplier> supplierRepository,
        IStringLocalizer<UpdateVendorInvoiceHandler> localizer,
        ILogger<UpdateVendorInvoiceHandler> logger,
        IRepository<Product>? productRepository = null)
    {
        _invoiceRepository = invoiceRepository;
        _supplierRepository = supplierRepository;
        _localizer = localizer;
        _logger = logger;
        _productRepository = productRepository;
    }

    public async Task<Guid> Handle(UpdateVendorInvoiceRequest request, CancellationToken cancellationToken)
    _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);
        var invoice = await _invoiceRepository.FirstOrDefaultAsync(new VendorInvoiceByIdWithItemsSpec(request.Id), cancellationToken);

        if (invoice == null)
        {
            throw new NotFoundException(_localizer["Vendor Invoice not found."]);
        }

        // Potentially check invoice status before allowing updates (e.g., cannot update a 'Paid' invoice)
        if (invoice.Status == VendorInvoiceStatus.Paid || invoice.Status == VendorInvoiceStatus.Cancelled)
        {
            throw new ConflictException(_localizer["Cannot update an invoice that is already {0}.", invoice.Status]);
        }

        // Update scalar properties
        // Note: The VendorInvoice.Update method should handle nulls correctly (only update if value is provided)
        invoice.Update(
            invoiceNumber: request.InvoiceNumber,
            invoiceDate: request.InvoiceDate,
            dueDate: request.DueDate,
            totalAmount: null, // TotalAmount will be recalculated or validated based on items
            currency: request.Currency,
            status: null, // Status updates should ideally be specific actions/requests e.g. SubmitInvoiceRequest
            notes: request.Notes
        );

        // Update SupplierId if provided
        if (request.SupplierId.HasValue && request.SupplierId.Value != invoice.SupplierId)
        {
            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId.Value, cancellationToken);
            if (supplier == null) throw new NotFoundException(_localizer["Supplier not found."]);
            // How to update SupplierId? VendorInvoice entity might not have a public setter or method.
            // This implies SupplierId might not be updatable after creation or needs a specific method.
            // For now, assuming it can be updated if the VendorInvoice domain entity allows it (e.g. through a specific method).
            // If it's not directly updatable on the entity, this line would cause an issue or be ignored.
            // invoice.SetSupplier(request.SupplierId.Value); // Example if such method existed
            _logger.LogWarning("Attempting to change SupplierId on VendorInvoice {0}. Ensure entity supports this.", invoice.Id);
        }


        // Handle Invoice Items
        if (request.InvoiceItems != null)
        {
            await UpdateInvoiceItems(invoice, request.InvoiceItems, cancellationToken);
        }

        // Recalculate TotalAmount from items
        // This ensures data integrity. The request.TotalAmount can be used as a cross-check.
        decimal calculatedTotalFromItems = invoice.InvoiceItems.Sum(item => item.Quantity * item.UnitPrice);
        if (request.TotalAmount.HasValue && Math.Abs(calculatedTotalFromItems - request.TotalAmount.Value) > 0.001m)
        {
            throw new ValidationException(
                _localizer["Provided TotalAmount ({0}) does not match the sum of its items ({1}).", request.TotalAmount.Value, calculatedTotalFromItems]);
        }
        // Update the invoice's total amount based on the sum of its items
        invoice.Update(null,null,null, calculatedTotalFromItems, null,null,null);


        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        _logger.LogInformation(_localizer["Vendor Invoice {0} updated."], invoice.Id);
        return invoice.Id;
    }

    private async Task UpdateInvoiceItems(VendorInvoice invoice, List<UpdateVendorInvoiceItemRequest> itemRequests, CancellationToken cancellationToken)
    {
        var existingItems = invoice.InvoiceItems.ToList(); // Work with a copy

        // Remove items that are not in the request
        var itemsToRemove = existingItems.Where(ei => !itemRequests.Any(ri => ri.Id.HasValue && ri.Id.Value == ei.Id)).ToList();
        foreach (var itemToRemove in itemsToRemove)
        {
            invoice.RemoveInvoiceItem(itemToRemove.Id);
        }

        foreach (var itemRequest in itemRequests)
        {
            if (itemRequest.Id.HasValue && itemRequest.Id.Value != Guid.Empty) // Existing item
            {
                var existingItem = existingItems.FirstOrDefault(i => i.Id == itemRequest.Id.Value);
                if (existingItem == null)
                {
                    throw new NotFoundException(_localizer["Invoice item with ID {0} not found on invoice {1}.", itemRequest.Id.Value, invoice.Id]);
                }

                // Validate product if ProductId is provided and changed
                Guid? newProductId = itemRequest.ProductId;
                if (newProductId.HasValue && newProductId != existingItem.ProductId && _productRepository != null)
                {
                    var product = await _productRepository.GetByIdAsync(newProductId.Value, cancellationToken);
                    if (product == null) throw new NotFoundException(_localizer["Product with ID {0} not found.", newProductId.Value]);
                } else if (itemRequest.ProductId == Guid.Empty) // Convention to clear ProductId
                {
                    newProductId = null;
                }


                existingItem.Update(
                    description: itemRequest.Description,
                    quantity: itemRequest.Quantity,
                    unitPrice: itemRequest.UnitPrice,
                    taxAmount: itemRequest.TaxAmount,
                    productId: newProductId
                );
            }
            else // New item
            {
                if (string.IsNullOrWhiteSpace(itemRequest.Description) || !itemRequest.Quantity.HasValue || !itemRequest.UnitPrice.HasValue)
                {
                    throw new ValidationException(_localizer["For new invoice items, Description, Quantity, and UnitPrice are required."]);
                }

                // Validate product if ProductId is provided
                if (itemRequest.ProductId.HasValue && _productRepository != null)
                {
                    var product = await _productRepository.GetByIdAsync(itemRequest.ProductId.Value, cancellationToken);
                    if (product == null) throw new NotFoundException(_localizer["Product with ID {0} not found.", itemRequest.ProductId.Value]);
                }

                var newItem = new VendorInvoiceItem(
                    vendorInvoiceId: invoice.Id,
                    description: itemRequest.Description!, // Already checked for null/whitespace
                    quantity: itemRequest.Quantity!.Value, // Already checked for null
                    unitPrice: itemRequest.UnitPrice!.Value, // Already checked for null
                    taxAmount: itemRequest.TaxAmount ?? 0,
                    productId: itemRequest.ProductId
                );
                invoice.AddInvoiceItem(newItem);
            }
        }
    }
}

// Ensure VendorInvoiceByIdWithItemsSpec is accessible or defined.
// It was created in GetVendorInvoiceHandler.cs in a previous step.
// public class VendorInvoiceByIdWithItemsSpec : Specification<VendorInvoice, VendorInvoiceDto>, ISingleResultSpecification
// {
//    public VendorInvoiceByIdWithItemsSpec(Guid invoiceId)
//    {
//        Query
//            .Where(vi => vi.Id == invoiceId)
//            .Include(vi => vi.InvoiceItems);
//    }
// }
