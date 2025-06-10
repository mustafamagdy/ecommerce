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

namespace FSH.WebApi.Application.Accounting.CustomerPayments;

public class GetCustomerPaymentHandler : IRequestHandler<GetCustomerPaymentRequest, CustomerPaymentDto>
{
    private readonly IReadRepository<CustomerPayment> _paymentRepository;
    private readonly IReadRepository<Customer> _customerRepository; // To fetch CustomerName
    private readonly IStringLocalizer<GetCustomerPaymentHandler> _localizer;

    public GetCustomerPaymentHandler(
        IReadRepository<CustomerPayment> paymentRepository,
        IReadRepository<Customer> customerRepository,
        IStringLocalizer<GetCustomerPaymentHandler> localizer)
    {
        _paymentRepository = paymentRepository;
        _customerRepository = customerRepository;
        _localizer = localizer;
    }

    public async Task<CustomerPaymentDto> Handle(GetCustomerPaymentRequest request, CancellationToken cancellationToken)
    {
        var spec = new CustomerPaymentByIdWithDetailsSpec(request.Id); // Includes PaymentMethod and Applications with Invoices
        var customerPayment = await _paymentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (customerPayment == null)
        {
            throw new NotFoundException(_localizer["Customer Payment with ID {0} not found.", request.Id]);
        }

        var dto = customerPayment.Adapt<CustomerPaymentDto>();

        // Populate CustomerName
        var customer = await _customerRepository.GetByIdAsync(customerPayment.CustomerId, cancellationToken);
        dto.CustomerName = customer?.Name; // Assuming Customer has a Name property

        // PaymentMethodName should be populated by Mapster due to Include in spec
        if (customerPayment.PaymentMethod != null) // Safety check
        {
            dto.PaymentMethodName = customerPayment.PaymentMethod.Name;
        }

        // Populate CustomerInvoiceNumber for each application
        // This should also be largely handled by Mapster if DTO property names align with navigation path (e.g. CustomerInvoice.InvoiceNumber)
        foreach (var appDto in dto.AppliedInvoices)
        {
            var appEntity = customerPayment.AppliedInvoices.FirstOrDefault(a => a.Id == appDto.Id);
            if (appEntity?.CustomerInvoice != null) // Safety check
            {
                appDto.CustomerInvoiceNumber = appEntity.CustomerInvoice.InvoiceNumber;
            }
        }

        // Calculate UnappliedAmount
        dto.UnappliedAmount = customerPayment.GetUnappliedAmount();


        return dto;
    }
}
