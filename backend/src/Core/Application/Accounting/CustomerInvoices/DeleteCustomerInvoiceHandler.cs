using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class DeleteCustomerInvoiceHandler : IRequestHandler<DeleteCustomerInvoiceRequest, Guid>
{
    private readonly IRepository<CustomerInvoice> _invoiceRepository;
    private readonly IStringLocalizer<DeleteCustomerInvoiceHandler> _localizer;
    private readonly ILogger<DeleteCustomerInvoiceHandler> _logger;

    public DeleteCustomerInvoiceHandler(
        IRepository<CustomerInvoice> invoiceRepository,
        IStringLocalizer<DeleteCustomerInvoiceHandler> localizer,
        ILogger<DeleteCustomerInvoiceHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Guid> Handle(DeleteCustomerInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (invoice == null)
            throw new NotFoundException(_localizer["Customer Invoice with ID {0} not found.", request.Id]);

        // Business Rule: Prevent deletion of invoices that have payments or are in a non-deletable state.
        // A more robust check would involve querying actual payment applications for this invoice.
        // For this example, we'll check the status and AmountPaid property.
        if (invoice.Status == CustomerInvoiceStatus.Paid ||
            invoice.Status == CustomerInvoiceStatus.PartiallyPaid ||
            invoice.AmountPaid > 0)
        {
            throw new ConflictException(_localizer["Cannot delete invoice {0} as it has payments applied or is in a state that prevents deletion. Consider voiding the invoice instead.", invoice.InvoiceNumber]);
        }

        // Alternative: Instead of deleting, an invoice could be marked as 'Void' or 'Cancelled'.
        // This is often preferred for accounting records.
        // if (invoice.Status != CustomerInvoiceStatus.Draft && invoice.Status != CustomerInvoiceStatus.Sent) // Example
        // {
        //     invoice.UpdateStatus(CustomerInvoiceStatus.Void);
        //     await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        //     _logger.LogInformation(_localizer["Customer Invoice {0} voided."], invoice.InvoiceNumber);
        // }
        // else // True deletion only for drafts, perhaps
        // {
        //     await _invoiceRepository.DeleteAsync(invoice, cancellationToken);
        //     _logger.LogInformation(_localizer["Customer Invoice {0} deleted."], invoice.InvoiceNumber);
        // }


        // For now, implementing hard delete as per "DeleteCustomerInvoiceHandler" name,
        // but with the status/payment check.
        await _invoiceRepository.DeleteAsync(invoice, cancellationToken);
        _logger.LogInformation(_localizer["Customer Invoice {0} (ID: {1}) deleted."], invoice.InvoiceNumber, invoice.Id);

        return invoice.Id;
    }
}
