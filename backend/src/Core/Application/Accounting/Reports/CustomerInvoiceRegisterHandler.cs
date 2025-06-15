using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.CustomerInvoices.Specifications;
using FSH.WebApi.Application.Accounting.CustomerPayments.Specifications;
using FSH.WebApi.Application.Accounting.CreditMemos.Specifications;
using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Domain.Operation.Customers; // For Customer entity if _customerRepo used

namespace FSH.WebApi.Application.Accounting.Reports;

public class CustomerInvoiceRegisterHandler : IRequestHandler<CustomerInvoiceRegisterRequest, CustomerInvoiceRegisterDto>
{
    private readonly IReadRepository<CustomerInvoice> _invoiceRepo;
    private readonly IReadRepository<Customer> _customerRepo; // Only needed if CustomerId filter is applied and name is fetched separately
    private readonly IReadRepository<CustomerPaymentApplication> _paymentAppRepo;
    private readonly IReadRepository<CreditMemoApplication> _creditMemoAppRepo;
    // private readonly IStringLocalizer<CustomerInvoiceRegisterHandler> _localizer;

    private const decimal ZeroAmountTolerance = 0.005m;

    public CustomerInvoiceRegisterHandler(
        IReadRepository<CustomerInvoice> invoiceRepo,
        IReadRepository<Customer> customerRepo,
        IReadRepository<CustomerPaymentApplication> paymentAppRepo,
        IReadRepository<CreditMemoApplication> creditMemoAppRepo
        /* IStringLocalizer<CustomerInvoiceRegisterHandler> localizer */)
    {
        _invoiceRepo = invoiceRepo;
        _customerRepo = customerRepo;
        _paymentAppRepo = paymentAppRepo;
        _creditMemoAppRepo = creditMemoAppRepo;
        // _localizer = localizer;
    }

    public async Task<CustomerInvoiceRegisterDto> Handle(CustomerInvoiceRegisterRequest request, CancellationToken cancellationToken)
    {
        var reportDto = new CustomerInvoiceRegisterDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CustomerId = request.CustomerId,
            StatusFilter = request.StatusFilter.ToString(),
            AsOfDate = request.AsOfDate,
            GeneratedOn = DateTime.UtcNow.ToString("o"),
            Invoices = new List<CustomerInvoiceRegisterLineDto>()
        };

        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value, cancellationToken);
            if (customer == null)
                throw new NotFoundException($"Customer with ID {request.CustomerId.Value} not found.");
            reportDto.CustomerName = customer.Name;
        }

        // 1. Fetch Invoices
        var invoicesSpec = new CustomerInvoicesForRegisterSpec(request.StartDate, request.EndDate, request.CustomerId);
        var candidateInvoices = await _invoiceRepo.ListAsync(invoicesSpec, cancellationToken);

        if (!candidateInvoices.Any())
        {
            return reportDto; // Return empty report
        }

        var invoiceIds = candidateInvoices.Select(i => i.Id).ToList();

        // 2. Fetch Relevant Payment Applications
        var paymentAppsSpec = new PaymentApplicationsForCustomerInvoicesUpToDateSpec(invoiceIds, request.AsOfDate);
        var allPaymentApps = await _paymentAppRepo.ListAsync(paymentAppsSpec, cancellationToken);
        var paymentsByInvoiceId = allPaymentApps
            .GroupBy(app => app.CustomerInvoiceId)
            .ToDictionary(
                g => g.Key,
                g => new { Amount = g.Sum(app => app.AmountApplied), MaxDate = g.Max(app => app.CustomerPayment.PaymentDate) }
            );

        // 3. Fetch Relevant Credit Memo Applications
        var creditAppsSpec = new CreditApplicationsForCustomerInvoicesUpToDateSpec(invoiceIds, request.AsOfDate);
        var allCreditApps = await _creditMemoAppRepo.ListAsync(creditAppsSpec, cancellationToken);
        var creditsByInvoiceId = allCreditApps
            .GroupBy(app => app.CustomerInvoiceId)
            .ToDictionary(
                g => g.Key,
                g => new { Amount = g.Sum(app => app.AmountApplied), MaxDate = g.Max(app => app.CreditMemo.Date) }
            );

        // 4. Process Invoices and Populate Lines
        foreach (var invoice in candidateInvoices)
        {
            decimal sumOfPayments = paymentsByInvoiceId.TryGetValue(invoice.Id, out var pVal) ? pVal.Amount : 0m;
            DateTime? maxPaymentDate = paymentsByInvoiceId.TryGetValue(invoice.Id, out var pDateVal) ? pDateVal.MaxDate : (DateTime?)null;

            decimal sumOfCredits = creditsByInvoiceId.TryGetValue(invoice.Id, out var cVal) ? cVal.Amount : 0m;
            DateTime? maxCreditDate = creditsByInvoiceId.TryGetValue(invoice.Id, out var cDateVal) ? cDateVal.MaxDate : (DateTime?)null;

            decimal amountPaidOrCredited = sumOfPayments + sumOfCredits;
            decimal amountDue = invoice.TotalAmount - amountPaidOrCredited;

            bool isEffectivelyPaid = amountDue <= ZeroAmountTolerance;
            bool isOverdue = !isEffectivelyPaid && invoice.DueDate.Date < request.AsOfDate.Date;

            string calculatedStatus;
            if (isEffectivelyPaid)
            {
                calculatedStatus = "Paid";
            }
            else if (isOverdue)
            {
                calculatedStatus = "Overdue";
            }
            else // Not fully paid and not overdue
            {
                calculatedStatus = amountPaidOrCredited > ZeroAmountTolerance ? "Partially Paid" : "Unpaid";
            }

            // Apply Status Filter
            bool includeInReport = false;
            switch (request.StatusFilter)
            {
                case CustomerInvoiceRegisterStatusFilter.All:
                    includeInReport = true;
                    break;
                case CustomerInvoiceRegisterStatusFilter.Open: // Not fully paid/credited
                    if (!isEffectivelyPaid) includeInReport = true;
                    break;
                case CustomerInvoiceRegisterStatusFilter.Paid:
                    if (isEffectivelyPaid) includeInReport = true;
                    break;
                case CustomerInvoiceRegisterStatusFilter.Overdue:
                    if (isOverdue) includeInReport = true;
                    break;
                case CustomerInvoiceRegisterStatusFilter.Unpaid: // No payments or credits applied yet
                    if (amountPaidOrCredited <= ZeroAmountTolerance && invoice.TotalAmount > ZeroAmountTolerance) includeInReport = true;
                    break;
            }

            if (!includeInReport)
            {
                continue;
            }

            DateTime? lastActivityDate = null;
            if (maxPaymentDate.HasValue && maxCreditDate.HasValue)
                lastActivityDate = maxPaymentDate > maxCreditDate ? maxPaymentDate : maxCreditDate;
            else if (maxPaymentDate.HasValue)
                lastActivityDate = maxPaymentDate;
            else if (maxCreditDate.HasValue)
                lastActivityDate = maxCreditDate;

            var line = new CustomerInvoiceRegisterLineDto
            {
                CustomerInvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                CustomerId = invoice.CustomerId,
                CustomerName = invoice.Customer?.Name ?? "N/A", // Customer included by spec
                CustomerContactInfo = invoice.Customer?.Email ?? invoice.Customer?.PhoneNumber,
                TotalAmount = invoice.TotalAmount,
                AmountPaidOrCredited = amountPaidOrCredited,
                AmountDue = amountDue,
                Status = calculatedStatus,
                LastPaymentOrCreditDate = lastActivityDate,
                DaysOverdue = isOverdue ? (request.AsOfDate.Date - invoice.DueDate.Date).Days : 0
            };
            reportDto.Invoices.Add(line);

            reportDto.GrandTotalAmount += line.TotalAmount;
            reportDto.GrandTotalPaidOrCreditedAmount += line.AmountPaidOrCredited;
            reportDto.GrandTotalAmountDue += line.AmountDue;
        }

        // Sorting is handled by the spec primarily (CustomerName, then InvoiceDate)
        // If additional sorting on calculated fields is needed, it can be done here.
        // reportDto.Invoices = reportDto.Invoices.OrderBy(l => l.CustomerName).ThenBy(l => l.InvoiceDate).ToList();

        return reportDto;
    }
}
