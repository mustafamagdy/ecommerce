using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Operation.Customers; // Assuming Customer entity path
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class CreateCreditMemoHandler : IRequestHandler<CreateCreditMemoRequest, Guid>
{
    private readonly IRepository<CreditMemo> _creditMemoRepository;
    private readonly IReadRepository<Customer> _customerRepository;
    private readonly IReadRepository<CustomerInvoice>? _invoiceRepository; // For validating OriginalCustomerInvoiceId
    private readonly IStringLocalizer<CreateCreditMemoHandler> _localizer;
    private readonly ILogger<CreateCreditMemoHandler> _logger;

    public CreateCreditMemoHandler(
        IRepository<CreditMemo> creditMemoRepository,
        IReadRepository<Customer> customerRepository,
        IStringLocalizer<CreateCreditMemoHandler> localizer,
        ILogger<CreateCreditMemoHandler> logger,
        IReadRepository<CustomerInvoice>? invoiceRepository = null)
    {
        _creditMemoRepository = creditMemoRepository;
        _customerRepository = customerRepository;
        _localizer = localizer;
        _logger = logger;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Guid> Handle(CreateCreditMemoRequest request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
            throw new NotFoundException(_localizer["Customer with ID {0} not found.", request.CustomerId]);

        if (request.OriginalCustomerInvoiceId.HasValue && _invoiceRepository != null)
        {
            var originalInvoice = await _invoiceRepository.GetByIdAsync(request.OriginalCustomerInvoiceId.Value, cancellationToken);
            if (originalInvoice == null)
                throw new NotFoundException(_localizer["Original Customer Invoice with ID {0} not found.", request.OriginalCustomerInvoiceId.Value]);
            if (originalInvoice.CustomerId != request.CustomerId)
                throw new ValidationException(_localizer["Original Invoice {0} does not belong to Customer {1}.", originalInvoice.InvoiceNumber, customer.Name]);
        }

        string creditMemoNumber = await GenerateNextCreditMemoNumberAsync(cancellationToken);

        var creditMemo = new CreditMemo(
            customerId: request.CustomerId,
            creditMemoNumber: creditMemoNumber,
            date: request.Date,
            reason: request.Reason,
            totalAmount: request.TotalAmount,
            currency: request.Currency,
            notes: request.Notes,
            originalCustomerInvoiceId: request.OriginalCustomerInvoiceId,
            status: CreditMemoStatus.Approved // Or Draft, depending on workflow. Let's use Approved if no items.
        );

        await _creditMemoRepository.AddAsync(creditMemo, cancellationToken);
        _logger.LogInformation(_localizer["Credit Memo {0} created for Customer {1}."], creditMemo.CreditMemoNumber, customer.Name);
        return creditMemo.Id;
    }

    private async Task<string> GenerateNextCreditMemoNumberAsync(CancellationToken cancellationToken)
    {
        // Basic example: CR-YYYYMMDD-XXXX. Not concurrency-safe.
        var today = DateTime.UtcNow;
        string prefix = $"CR-{today:yyyyMMdd}-";

        var lastCreditMemoTodaySpec = new CreditMemoByNumberPrefixSpec(prefix);
        var lastCreditMemo = (await _creditMemoRepository.ListAsync(lastCreditMemoTodaySpec, cancellationToken))
                                .OrderByDescending(cm => cm.CreditMemoNumber)
                                .FirstOrDefault();
        int nextSequence = 1;
        if (lastCreditMemo != null)
        {
            string lastSeqStr = lastCreditMemo.CreditMemoNumber.Substring(prefix.Length);
            if (int.TryParse(lastSeqStr, out int lastSeq))
            {
                nextSequence = lastSeq + 1;
            }
        }
        return $"{prefix}{nextSequence:D4}";
    }
}

// Specification to find credit memos by prefix for number generation
public class CreditMemoByNumberPrefixSpec : Specification<CreditMemo>
{
    public CreditMemoByNumberPrefixSpec(string prefix)
    {
        Query.Where(cm => cm.CreditMemoNumber.StartsWith(prefix));
    }
}
