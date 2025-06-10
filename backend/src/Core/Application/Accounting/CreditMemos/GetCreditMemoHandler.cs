using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Operation.Customers; // For Customer Name
using MediatR;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class GetCreditMemoHandler : IRequestHandler<GetCreditMemoRequest, CreditMemoDto>
{
    private readonly IReadRepository<CreditMemo> _creditMemoRepository;
    private readonly IReadRepository<Customer> _customerRepository; // To fetch CustomerName
    private readonly IReadRepository<CustomerInvoice> _invoiceRepository; // To fetch OriginalCustomerInvoiceNumber
    private readonly IStringLocalizer<GetCreditMemoHandler> _localizer;

    public GetCreditMemoHandler(
        IReadRepository<CreditMemo> creditMemoRepository,
        IReadRepository<Customer> customerRepository,
        IReadRepository<CustomerInvoice> invoiceRepository,
        IStringLocalizer<GetCreditMemoHandler> localizer)
    {
        _creditMemoRepository = creditMemoRepository;
        _customerRepository = customerRepository;
        _invoiceRepository = invoiceRepository;
        _localizer = localizer;
    }

    public async Task<CreditMemoDto> Handle(GetCreditMemoRequest request, CancellationToken cancellationToken)
    {
        var spec = new CreditMemoByIdWithDetailsSpec(request.Id); // Includes Applications with their Invoices
        var creditMemo = await _creditMemoRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (creditMemo == null)
            throw new NotFoundException(_localizer["Credit Memo with ID {0} not found.", request.Id]);

        var dto = creditMemo.Adapt<CreditMemoDto>();
        dto.Status = creditMemo.Status.ToString(); // Map enum

        // Populate CustomerName
        var customer = await _customerRepository.GetByIdAsync(creditMemo.CustomerId, cancellationToken);
        dto.CustomerName = customer?.Name;

        // Populate OriginalCustomerInvoiceNumber
        if (creditMemo.OriginalCustomerInvoiceId.HasValue)
        {
            var originalInvoice = await _invoiceRepository.GetByIdAsync(creditMemo.OriginalCustomerInvoiceId.Value, cancellationToken);
            dto.OriginalCustomerInvoiceNumber = originalInvoice?.InvoiceNumber;
        }

        // Populate CustomerInvoiceNumber for each application (should be handled by Spec + Mapster if names align)
        // For clarity, explicit mapping if needed:
        foreach (var appDto in dto.Applications)
        {
            var appEntity = creditMemo.Applications.FirstOrDefault(a => a.Id == appDto.Id);
            if (appEntity?.CustomerInvoice != null) // CustomerInvoice should be included by the spec
            {
                appDto.CustomerInvoiceNumber = appEntity.CustomerInvoice.InvoiceNumber;
            }
        }

        // Calculate AppliedAmount and AvailableBalance
        dto.AppliedAmount = creditMemo.GetAppliedAmount();
        dto.AvailableBalance = creditMemo.GetAvailableBalance();

        return dto;
    }
}
