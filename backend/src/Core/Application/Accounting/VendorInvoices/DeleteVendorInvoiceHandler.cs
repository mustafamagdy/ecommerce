using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class DeleteVendorInvoiceHandler : IRequestHandler<DeleteVendorInvoiceRequest, Guid>
{
    private readonly IRepository<VendorInvoice> _invoiceRepository;
    private readonly IStringLocalizer<DeleteVendorInvoiceHandler> _localizer;
    private readonly ILogger<DeleteVendorInvoiceHandler> _logger;

    public DeleteVendorInvoiceHandler(
        IRepository<VendorInvoice> invoiceRepository,
        IStringLocalizer<DeleteVendorInvoiceHandler> localizer,
        ILogger<DeleteVendorInvoiceHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(DeleteVendorInvoiceRequest request, CancellationToken cancellationToken)
    {
        // It's good practice to load related entities if business rules depend on them.
        // For simple delete, just fetching the aggregate root might be enough.
        // If we need to check items or other related data, use a spec like VendorInvoiceByIdWithItemsSpec.
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);

        if (invoice == null)
        {
            throw new NotFoundException(_localizer["Vendor Invoice with ID {0} not found.", request.Id]);
        }

        // Business Rule: For example, cannot delete an invoice if it's 'Paid' or 'Approved'.
        // This depends on specific requirements.
        if (invoice.Status == VendorInvoiceStatus.Paid ||
            invoice.Status == VendorInvoiceStatus.Approved ||
            invoice.Status == VendorInvoiceStatus.Submitted) // Or any other non-deletable status
        {
            throw new ConflictException(
                _localizer["Cannot delete invoice with status {0}. Only invoices in 'Draft' or 'Cancelled' status can be deleted.", invoice.Status]);
        }

        // If the invoice has items, they should be deleted as part of the aggregate deletion by EF Core
        // if cascading deletes are configured or if VendorInvoiceItems are owned by VendorInvoice.
        // If not, manual deletion of items might be required if IRepository doesn't handle cascades for aggregates.
        // Most modern ORMs with proper aggregate configuration handle this.

        await _invoiceRepository.DeleteAsync(invoice, cancellationToken);

        _logger.LogInformation(_localizer["Vendor Invoice {0} deleted."], invoice.Id);

        return invoice.Id;
    }
}
