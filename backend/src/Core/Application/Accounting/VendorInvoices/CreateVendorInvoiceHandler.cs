using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Catalog; // For Product, if validating ProductId
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class CreateVendorInvoiceHandler : IRequestHandler<CreateVendorInvoiceRequest, Guid>
{
    private readonly IRepository<VendorInvoice> _invoiceRepository;
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IRepository<Product>? _productRepository; // Optional: if product validation is strict
    private readonly IStringLocalizer<CreateVendorInvoiceHandler> _localizer;
    private readonly ILogger<CreateVendorInvoiceHandler> _logger;

    public CreateVendorInvoiceHandler(
        IRepository<VendorInvoice> invoiceRepository,
        IRepository<Supplier> supplierRepository,
        IStringLocalizer<CreateVendorInvoiceHandler> localizer,
        ILogger<CreateVendorInvoiceHandler> logger,
        IRepository<Product>? productRepository = null) // Product repo is optional
    {
        _invoiceRepository = invoiceRepository;
        _supplierRepository = supplierRepository;
        _localizer = localizer;
        _logger = logger;
        _productRepository = productRepository;
    }

    public async Task<Guid> Handle(CreateVendorInvoiceRequest request, CancellationToken cancellationToken)
    {
        // 1. Validate Supplier
        var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken);
        if (supplier == null)
        {
            throw new NotFoundException(_localizer["Supplier not found."]);
        }

        // 2. Validate Invoice Date and Due Date (basic validation already in validator, can add more here)
        if (request.DueDate < request.InvoiceDate)
        {
            // This should be caught by FluentValidation, but as an example of domain/handler validation:
            throw new ValidationException(_localizer["Due date cannot be before invoice date."]);
        }

        // 3. Create VendorInvoice entity
        var invoice = new VendorInvoice(
            supplierId: request.SupplierId,
            invoiceNumber: request.InvoiceNumber,
            invoiceDate: request.InvoiceDate,
            dueDate: request.DueDate,
            totalAmount: request.TotalAmount, // This should match sum of items
            currency: request.Currency,
            status: VendorInvoiceStatus.Draft, // Initial status
            notes: request.Notes
        );

        // 4. Process and add VendorInvoiceItems
        decimal calculatedTotalAmount = 0;
        foreach (var itemRequest in request.InvoiceItems)
        {
            // Optional: Validate ProductId if productRepository is available
            if (itemRequest.ProductId.HasValue && _productRepository != null)
            {
                var product = await _productRepository.GetByIdAsync(itemRequest.ProductId.Value, cancellationToken);
                if (product == null)
                {
                    throw new NotFoundException(_localizer["Product with ID {0} not found.", itemRequest.ProductId.Value]);
                }
            }

            var itemTotal = itemRequest.Quantity * itemRequest.UnitPrice;
            if (itemTotal != itemRequest.TotalAmount)
            {
                 throw new ValidationException(_localizer["Item total for '{0}' does not match Quantity * UnitPrice.", itemRequest.Description]);
            }

            var invoiceItem = new VendorInvoiceItem(
                vendorInvoiceId: invoice.Id, // Will be set by EF Core if relationship is configured
                description: itemRequest.Description,
                quantity: itemRequest.Quantity,
                unitPrice: itemRequest.UnitPrice,
                taxAmount: itemRequest.TaxAmount,
                productId: itemRequest.ProductId
            );
            invoice.AddInvoiceItem(invoiceItem); // Assuming AddInvoiceItem correctly adds to the internal list
            calculatedTotalAmount += itemTotal;
        }

        // 5. Validate TotalAmount against sum of items
        if (Math.Abs(calculatedTotalAmount - request.TotalAmount) > 0.001m) // Using a small tolerance for decimal comparison
        {
            throw new ValidationException(_localizer["Invoice TotalAmount does not match the sum of its items."]);
        }
        // If invoice.TotalAmount should be authoritative, set it here after calculation
        // invoice.Update(totalAmount: calculatedTotalAmount, ... other fields null ...);


        // 6. Add to repository
        await _invoiceRepository.AddAsync(invoice, cancellationToken);

        _logger.LogInformation(_localizer["Vendor Invoice created: {0}"], invoice.Id);

        return invoice.Id;
    }
}
