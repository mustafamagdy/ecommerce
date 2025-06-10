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
using FSH.WebApi.Application.Common.Interfaces; // For IUnitOfWork or similar

namespace FSH.WebApi.Application.Accounting.CustomerPayments;

public class CreateCustomerPaymentHandler : IRequestHandler<CreateCustomerPaymentRequest, Guid>
{
    private readonly IRepository<CustomerPayment> _paymentRepository;
    private readonly IRepository<CustomerInvoice> _invoiceRepository;
    private readonly IReadRepository<Customer> _customerRepository;
    private readonly IReadRepository<PaymentMethod> _paymentMethodRepository;
    private readonly IStringLocalizer<CreateCustomerPaymentHandler> _localizer;
    private readonly ILogger<CreateCustomerPaymentHandler> _logger;
    // private readonly IUnitOfWork _unitOfWork; // Recommended for transaction management

    public CreateCustomerPaymentHandler(
        IRepository<CustomerPayment> paymentRepository,
        IRepository<CustomerInvoice> invoiceRepository,
        IReadRepository<Customer> customerRepository,
        IReadRepository<PaymentMethod> paymentMethodRepository,
        IStringLocalizer<CreateCustomerPaymentHandler> localizer,
        ILogger<CreateCustomerPaymentHandler> logger
        /* IUnitOfWork unitOfWork */)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _localizer = localizer;
        _logger = logger;
        // _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCustomerPaymentRequest request, CancellationToken cancellationToken)
    {
        // await _unitOfWork.BeginTransactionAsync(cancellationToken); // Example
        try
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null) throw new NotFoundException(_localizer["Customer not found."]);

            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(request.PaymentMethodId, cancellationToken);
            if (paymentMethod == null) throw new NotFoundException(_localizer["Payment Method not found."]);

            decimal totalAmountToApply = request.Applications.Sum(a => a.AmountApplied);
            if (totalAmountToApply > request.AmountReceived)
            {
                throw new ValidationException(
                    _localizer["Total amount to be applied ({0}) cannot exceed the amount received ({1}).", totalAmountToApply, request.AmountReceived]);
            }

            var customerPayment = new CustomerPayment(
                customerId: request.CustomerId,
                paymentDate: request.PaymentDate,
                amountReceived: request.AmountReceived,
                paymentMethodId: request.PaymentMethodId,
                referenceNumber: request.ReferenceNumber,
                notes: request.Notes
            );

            foreach (var appRequest in request.Applications)
            {
                var invoice = await _invoiceRepository.GetByIdAsync(appRequest.CustomerInvoiceId, cancellationToken);
                if (invoice == null)
                    throw new NotFoundException(_localizer["Customer Invoice with ID {0} not found.", appRequest.CustomerInvoiceId]);
                if (invoice.CustomerId != request.CustomerId)
                    throw new ValidationException(_localizer["Invoice {0} does not belong to the specified customer.", invoice.InvoiceNumber]);

                if (invoice.GetBalanceDue() < appRequest.AmountApplied)
                {
                    throw new ConflictException(
                        _localizer["Amount applied ({0}) to invoice {1} exceeds its balance due of {2}.",
                        appRequest.AmountApplied, invoice.InvoiceNumber, invoice.GetBalanceDue()]);
                }

                customerPayment.AddPaymentApplication(appRequest.CustomerInvoiceId, appRequest.AmountApplied);
                invoice.ApplyPayment(appRequest.AmountApplied); // This method should update AmountPaid and Status
                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
            }

            await _paymentRepository.AddAsync(customerPayment, cancellationToken);
            // await _unitOfWork.CommitTransactionAsync(cancellationToken); // Example

            _logger.LogInformation(_localizer["Customer Payment {0} created for Customer {1}."], customerPayment.Id, customer.Name);
            return customerPayment.Id;
        }
        catch (Exception ex)
        {
            // await _unitOfWork.RollbackTransactionAsync(cancellationToken); // Example
            _logger.LogError(ex, _localizer["Error creating customer payment."]);
            throw;
        }
    }
}
