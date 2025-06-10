using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using FSH.WebApi.Domain.Operation.Customers; // For Customer Name
using MediatR;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class SearchCreditMemosHandler : IRequestHandler<SearchCreditMemosRequest, PaginationResponse<CreditMemoDto>>
{
    private readonly IReadRepository<CreditMemo> _creditMemoRepository;
    private readonly IReadRepository<Customer> _customerRepository; // To fetch CustomerName
    private readonly IReadRepository<CustomerInvoice> _invoiceRepository; // To fetch OriginalCustomerInvoiceNumber
    private readonly IStringLocalizer<SearchCreditMemosHandler> _localizer;

    public SearchCreditMemosHandler(
        IReadRepository<CreditMemo> creditMemoRepository,
        IReadRepository<Customer> customerRepository,
        IReadRepository<CustomerInvoice> invoiceRepository,
        IStringLocalizer<SearchCreditMemosHandler> localizer)
    {
        _creditMemoRepository = creditMemoRepository;
        _customerRepository = customerRepository;
        _invoiceRepository = invoiceRepository;
        _localizer = localizer;
    }

    public async Task<PaginationResponse<CreditMemoDto>> Handle(SearchCreditMemosRequest request, CancellationToken cancellationToken)
    {
        var spec = new CreditMemosBySearchFilterSpec(request); // Includes Applications with Invoices

        var creditMemos = await _creditMemoRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _creditMemoRepository.CountAsync(spec, cancellationToken);

        var dtos = new List<CreditMemoDto>();
        foreach (var creditMemo in creditMemos)
        {
            var dto = creditMemo.Adapt<CreditMemoDto>();
            dto.Status = creditMemo.Status.ToString();

            var customer = await _customerRepository.GetByIdAsync(creditMemo.CustomerId, cancellationToken);
            dto.CustomerName = customer?.Name;

            if (creditMemo.OriginalCustomerInvoiceId.HasValue)
            {
                var originalInvoice = await _invoiceRepository.GetByIdAsync(creditMemo.OriginalCustomerInvoiceId.Value, cancellationToken);
                dto.OriginalCustomerInvoiceNumber = originalInvoice?.InvoiceNumber;
            }

            foreach (var appDto in dto.Applications)
            {
                var appEntity = creditMemo.Applications.FirstOrDefault(a => a.Id == appDto.Id);
                if (appEntity?.CustomerInvoice != null)
                {
                    appDto.CustomerInvoiceNumber = appEntity.CustomerInvoice.InvoiceNumber;
                }
            }

            dto.AppliedAmount = creditMemo.GetAppliedAmount();
            dto.AvailableBalance = creditMemo.GetAvailableBalance();

            dtos.Add(dto);
        }

        return new PaginationResponse<CreditMemoDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
