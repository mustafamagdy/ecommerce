using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Common.Interfaces; // For IUnitOfWork (recommended)

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class ApplyCreditMemoToInvoiceHandler : IRequestHandler<ApplyCreditMemoToInvoiceRequest, Guid>
{
    private readonly IRepository<CreditMemo> _creditMemoRepository;
    private readonly IRepository<CustomerInvoice> _invoiceRepository;
    private readonly IStringLocalizer<ApplyCreditMemoToInvoiceHandler> _localizer;
    private readonly ILogger<ApplyCreditMemoToInvoiceHandler> _logger;
    // private readonly IUnitOfWork _unitOfWork; // Recommended for transaction management

    public ApplyCreditMemoToInvoiceHandler(
        IRepository<CreditMemo> creditMemoRepository,
        IRepository<CustomerInvoice> invoiceRepository,
        IStringLocalizer<ApplyCreditMemoToInvoiceHandler> localizer,
        ILogger<ApplyCreditMemoToInvoiceHandler> logger
        /* IUnitOfWork unitOfWork */)
    {
        _creditMemoRepository = creditMemoRepository;
        _invoiceRepository = invoiceRepository;
        _localizer = localizer;
        _logger = logger;
        // _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(ApplyCreditMemoToInvoiceRequest request, CancellationToken cancellationToken)
    {
        // await _unitOfWork.BeginTransactionAsync(cancellationToken); // Example
        try
        {
            var creditMemo = await _creditMemoRepository.GetByIdAsync(request.CreditMemoId, cancellationToken);
            if (creditMemo == null)
                throw new NotFoundException(_localizer["Credit Memo with ID {0} not found.", request.CreditMemoId]);

            var invoice = await _invoiceRepository.GetByIdAsync(request.CustomerInvoiceId, cancellationToken);
            if (invoice == null)
                throw new NotFoundException(_localizer["Customer Invoice with ID {0} not found.", request.CustomerInvoiceId]);

            // Validate consistency: Credit Memo and Invoice must belong to the same customer.
            if (creditMemo.CustomerId != invoice.CustomerId)
            {
                throw new ValidationException(_localizer["Credit Memo and Invoice do not belong to the same customer."]);
            }

            // Validate status of credit memo (must be Approved or PartiallyApplied)
            if (creditMemo.Status != CreditMemoStatus.Approved && creditMemo.Status != CreditMemoStatus.PartiallyApplied)
            {
                throw new ConflictException(_localizer["Credit Memo {0} is not in a state that allows application (Status: {1}).", creditMemo.CreditMemoNumber, creditMemo.Status]);
            }

            // Validate status of invoice (must not be Paid or Void)
            if (invoice.Status == CustomerInvoiceStatus.Paid || invoice.Status == CustomerInvoiceStatus.Void)
            {
                throw new ConflictException(_localizer["Invoice {0} is already {1} and cannot have credit applied.", invoice.InvoiceNumber, invoice.Status]);
            }

            // Validate AmountToApply
            if (request.AmountToApply > creditMemo.GetAvailableBalance())
            {
                throw new ValidationException(_localizer["Amount to apply ({0}) exceeds available credit balance ({1}).", request.AmountToApply, creditMemo.GetAvailableBalance()]);
            }
            if (request.AmountToApply > invoice.GetBalanceDue())
            {
                throw new ValidationException(_localizer["Amount to apply ({0}) exceeds invoice balance due ({1}).", request.AmountToApply, invoice.GetBalanceDue()]);
            }

            // Perform application
            creditMemo.AddApplication(invoice.Id, request.AmountToApply); // Updates CM status internally
            invoice.ApplyPayment(request.AmountToApply); // This is like a payment for the invoice, updates invoice status internally

            await _creditMemoRepository.UpdateAsync(creditMemo, cancellationToken);
            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

            // await _unitOfWork.CommitTransactionAsync(cancellationToken); // Example

            _logger.LogInformation(_localizer["Applied {0} from Credit Memo {1} to Invoice {2}."], request.AmountToApply, creditMemo.CreditMemoNumber, invoice.InvoiceNumber);

            // Return the ID of the CreditMemoApplication or the CreditMemo itself.
            // For now, returning CreditMemoId as the request doesn't create a new aggregate root directly.
            // The actual CreditMemoApplication entity is created inside creditMemo.AddApplication().
            // If an ID for the application record itself is needed, that logic would be more complex.
            return creditMemo.Id;
        }
        catch(Exception ex)
        {
            // await _unitOfWork.RollbackTransactionAsync(cancellationToken); // Example
            _logger.LogError(ex, _localizer["Error applying credit memo to invoice."]);
            throw;
        }
    }
}
