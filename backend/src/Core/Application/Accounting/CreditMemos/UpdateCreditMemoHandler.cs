using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Operation.Customers; // For validating customer if OriginalCustomerInvoiceId changes
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class UpdateCreditMemoHandler : IRequestHandler<UpdateCreditMemoRequest, Guid>
{
    private readonly IRepository<CreditMemo> _creditMemoRepository;
    private readonly IReadRepository<CustomerInvoice>? _invoiceRepository; // For validating OriginalCustomerInvoiceId
    private readonly IReadRepository<Customer>? _customerRepository; // For validating customer on original invoice
    private readonly IStringLocalizer<UpdateCreditMemoHandler> _localizer;
    private readonly ILogger<UpdateCreditMemoHandler> _logger;

    public UpdateCreditMemoHandler(
        IRepository<CreditMemo> creditMemoRepository,
        IStringLocalizer<UpdateCreditMemoHandler> localizer,
        ILogger<UpdateCreditMemoHandler> logger,
        IReadRepository<CustomerInvoice>? invoiceRepository = null,
        IReadRepository<Customer>? customerRepository = null)
    {
        _creditMemoRepository = creditMemoRepository;
        _localizer = localizer;
        _logger = logger;
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
    }

    public async Task<Guid> Handle(UpdateCreditMemoRequest request, CancellationToken cancellationToken)
    {
        var creditMemo = await _creditMemoRepository.GetByIdAsync(request.Id, cancellationToken);
        if (creditMemo == null)
            throw new NotFoundException(_localizer["Credit Memo with ID {0} not found.", request.Id]);

        // Business Rule: Prevent major updates if already applied or void.
        if (creditMemo.Status == CreditMemoStatus.Applied ||
            creditMemo.Status == CreditMemoStatus.PartiallyApplied ||
            creditMemo.Status == CreditMemoStatus.Void)
        {
            // Allow only notes update for example
            if (request.Notes != creditMemo.Notes &&
                (request.Date.HasValue || request.Reason is not null || request.TotalAmount.HasValue || request.Currency is not null || request.OriginalCustomerInvoiceId.HasValue)) {
                 throw new ConflictException(_localizer["Cannot update details of a credit memo that is {0}, except for its notes.", creditMemo.Status]);
            }
        }

        if (request.OriginalCustomerInvoiceId.HasValue &&
            request.OriginalCustomerInvoiceId != creditMemo.OriginalCustomerInvoiceId &&
            _invoiceRepository != null && _customerRepository != null)
        {
            var originalInvoice = await _invoiceRepository.GetByIdAsync(request.OriginalCustomerInvoiceId.Value, cancellationToken);
            if (originalInvoice == null)
                throw new NotFoundException(_localizer["Original Customer Invoice with ID {0} not found.", request.OriginalCustomerInvoiceId.Value]);

            // Ensure original invoice belongs to the same customer as the credit memo
            if (originalInvoice.CustomerId != creditMemo.CustomerId)
            {
                 var customer = await _customerRepository.GetByIdAsync(creditMemo.CustomerId, cancellationToken);
                 throw new ValidationException(_localizer["Original Invoice {0} does not belong to Customer {1} of the Credit Memo.", originalInvoice.InvoiceNumber, customer?.Name ?? creditMemo.CustomerId.ToString()]);
            }
        }


        creditMemo.Update(
            date: request.Date,
            reason: request.Reason,
            totalAmount: request.TotalAmount,
            currency: request.Currency,
            notes: request.Notes,
            originalCustomerInvoiceId: request.OriginalCustomerInvoiceId
        );

        await _creditMemoRepository.UpdateAsync(creditMemo, cancellationToken);
        _logger.LogInformation(_localizer["Credit Memo {0} updated."], creditMemo.CreditMemoNumber);
        return creditMemo.Id;
    }
}
