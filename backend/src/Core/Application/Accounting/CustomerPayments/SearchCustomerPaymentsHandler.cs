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

namespace FSH.WebApi.Application.Accounting.CustomerPayments;

public class SearchCustomerPaymentsHandler : IRequestHandler<SearchCustomerPaymentsRequest, PaginationResponse<CustomerPaymentDto>>
{
    private readonly IReadRepository<CustomerPayment> _paymentRepository;
    private readonly IReadRepository<Customer> _customerRepository; // To fetch CustomerName
    private readonly IStringLocalizer<SearchCustomerPaymentsHandler> _localizer;

    public SearchCustomerPaymentsHandler(
        IReadRepository<CustomerPayment> paymentRepository,
        IReadRepository<Customer> customerRepository,
        IStringLocalizer<SearchCustomerPaymentsHandler> localizer)
    {
        _paymentRepository = paymentRepository;
        _customerRepository = customerRepository;
        _localizer = localizer;
    }

    public async Task<PaginationResponse<CustomerPaymentDto>> Handle(SearchCustomerPaymentsRequest request, CancellationToken cancellationToken)
    {
        var spec = new CustomerPaymentsBySearchFilterSpec(request); // Includes related entities

        var payments = await _paymentRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _paymentRepository.CountAsync(spec, cancellationToken);

        var dtos = new List<CustomerPaymentDto>();
        foreach (var payment in payments)
        {
            var dto = payment.Adapt<CustomerPaymentDto>();

            // Populate CustomerName
            var customer = await _customerRepository.GetByIdAsync(payment.CustomerId, cancellationToken);
            dto.CustomerName = customer?.Name;

            // PaymentMethodName and InvoiceNumbers in applications should be handled by Mapster
            // due to Includes in the specification, if DTO property names align.
            // Explicit mapping for clarity if needed:
            if (payment.PaymentMethod != null)
            {
                dto.PaymentMethodName = payment.PaymentMethod.Name;
            }
            foreach (var appDto in dto.AppliedInvoices)
            {
                var appEntity = payment.AppliedInvoices.FirstOrDefault(a => a.Id == appDto.Id);
                if (appEntity?.CustomerInvoice != null)
                {
                    appDto.CustomerInvoiceNumber = appEntity.CustomerInvoice.InvoiceNumber;
                }
            }

            // Calculate UnappliedAmount for each DTO
            dto.UnappliedAmount = payment.GetUnappliedAmount();

            dtos.Add(dto);
        }

        return new PaginationResponse<CustomerPaymentDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
