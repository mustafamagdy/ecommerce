using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.CustomerPayments.Specifications;
using FSH.WebApi.Application.Accounting.CustomerInvoices.Specifications; // For CustomerInvoicesByIdsSpec
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Domain.Operation.Customers; // For Customer entity
// using Microsoft.Extensions.Localization; // If needed

namespace FSH.WebApi.Application.Accounting.Reports;

public class CustomerPaymentHistoryReportHandler : IRequestHandler<CustomerPaymentHistoryReportRequest, CustomerPaymentHistoryReportDto>
{
    private readonly IReadRepository<CustomerPayment> _paymentRepo;
    private readonly IReadRepository<Customer> _customerRepo;
    private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
    private readonly IReadRepository<CustomerInvoice> _invoiceRepo;
    // private readonly IStringLocalizer<CustomerPaymentHistoryReportHandler> _localizer;

    public CustomerPaymentHistoryReportHandler(
        IReadRepository<CustomerPayment> paymentRepo,
        IReadRepository<Customer> customerRepo,
        IReadRepository<PaymentMethod> paymentMethodRepo,
        IReadRepository<CustomerInvoice> invoiceRepo
        /* IStringLocalizer<CustomerPaymentHistoryReportHandler> localizer */)
    {
        _paymentRepo = paymentRepo;
        _customerRepo = customerRepo;
        _paymentMethodRepo = paymentMethodRepo;
        _invoiceRepo = invoiceRepo;
        // _localizer = localizer;
    }

    public async Task<CustomerPaymentHistoryReportDto> Handle(CustomerPaymentHistoryReportRequest request, CancellationToken cancellationToken)
    {
        var reportDto = new CustomerPaymentHistoryReportDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CustomerId = request.CustomerId,
            PaymentMethodId = request.PaymentMethodId,
            GeneratedOn = DateTime.UtcNow.ToString("o"),
            Payments = new List<CustomerPaymentHistoryLineDto>()
        };

        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value, cancellationToken);
            if (customer == null)
                throw new NotFoundException($"Customer with ID {request.CustomerId.Value} not found.");
            reportDto.CustomerName = customer.Name;
        }

        if (request.PaymentMethodId.HasValue)
        {
            var paymentMethod = await _paymentMethodRepo.GetByIdAsync(request.PaymentMethodId.Value, cancellationToken);
            if (paymentMethod == null)
                throw new NotFoundException($"Payment Method with ID {request.PaymentMethodId.Value} not found.");
            reportDto.PaymentMethodName = paymentMethod.Name;
        }

        // 1. Fetch Payments
        var paymentsSpec = new CustomerPaymentsForHistoryReportSpec(request.StartDate, request.EndDate, request.CustomerId, request.PaymentMethodId);
        var payments = await _paymentRepo.ListAsync(paymentsSpec, cancellationToken);

        if (!payments.Any())
        {
            return reportDto; // Return empty report if no payments match filters
        }

        // 2. Prepare Invoice Details (Optimization)
        var allAppliedInvoiceIds = payments
            .SelectMany(p => p.AppliedInvoices) // p.AppliedInvoices is IReadOnlyCollection<CustomerPaymentApplication>
            .Select(app => app.CustomerInvoiceId)
            .Distinct()
            .ToList();

        Dictionary<Guid, CustomerInvoice> relatedInvoicesMap = new Dictionary<Guid, CustomerInvoice>();
        if (allAppliedInvoiceIds.Any())
        {
            var invoicesSpec = new CustomerInvoicesByIdsSpec(allAppliedInvoiceIds); // Assumes this spec exists
            var relatedInvoices = await _invoiceRepo.ListAsync(invoicesSpec, cancellationToken);
            relatedInvoicesMap = relatedInvoices.ToDictionary(inv => inv.Id);
        }

        // 3. Populate Report Lines
        reportDto.TotalPaymentsAmount = 0m;
        reportDto.TotalAppliedAmount = 0m;
        reportDto.TotalUnappliedAmount = 0m;

        foreach (var payment in payments)
        {
            decimal unappliedAmount = payment.GetUnappliedAmount(); // Domain method
            decimal appliedAmount = payment.AmountReceived - unappliedAmount;

            var line = new CustomerPaymentHistoryLineDto
            {
                CustomerPaymentId = payment.Id,
                PaymentDate = payment.PaymentDate,
                CustomerId = payment.CustomerId,
                CustomerName = payment.Customer?.Name ?? "N/A", // Customer should be included by spec
                AmountReceived = payment.AmountReceived,
                PaymentMethodId = payment.PaymentMethodId,
                PaymentMethodName = payment.PaymentMethod?.Name ?? "N/A", // PaymentMethod should be included
                ReferenceNumber = payment.ReferenceNumber,
                Notes = payment.Notes,
                UnappliedAmount = unappliedAmount,
                AppliedAmount = appliedAmount,
                AppliedInvoices = new List<CustomerPaymentAppliedInvoiceDto>()
            };

            foreach (var application in payment.AppliedInvoices) // These are CustomerPaymentApplication entities
            {
                if (relatedInvoicesMap.TryGetValue(application.CustomerInvoiceId, out var appliedInvoice))
                {
                    line.AppliedInvoices.Add(new CustomerPaymentAppliedInvoiceDto
                    {
                        CustomerInvoiceId = application.CustomerInvoiceId,
                        InvoiceNumber = appliedInvoice.InvoiceNumber,
                        InvoiceDate = appliedInvoice.InvoiceDate,
                        InvoiceTotalAmount = appliedInvoice.TotalAmount,
                        AmountAppliedToInvoice = application.AmountApplied
                    });
                }
            }

            reportDto.Payments.Add(line);
            reportDto.TotalPaymentsAmount += payment.AmountReceived;
            reportDto.TotalAppliedAmount += appliedAmount;
            reportDto.TotalUnappliedAmount += unappliedAmount;
        }

        // Sorting is handled by the spec (PaymentDate DESC, CustomerName ASC)
        // reportDto.Payments = reportDto.Payments.OrderByDescending(p => p.PaymentDate).ThenBy(p => p.CustomerName).ToList();

        return reportDto;
    }
}
