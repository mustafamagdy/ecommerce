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
using FSH.WebApi.Domain.Operation.Customers; // For Customer entity if needed by IReadRepository<Customer>

namespace FSH.WebApi.Application.Accounting.Reports;

public class ArAgingReportHandler : IRequestHandler<ArAgingReportRequest, ArAgingReportDto>
{
    private readonly IReadRepository<CustomerInvoice> _invoiceRepo;
    private readonly IReadRepository<Customer> _customerRepo;
    private readonly IReadRepository<CustomerPaymentApplication> _paymentAppRepo;
    private readonly IReadRepository<CreditMemoApplication> _creditMemoAppRepo;
    // private readonly IStringLocalizer<ArAgingReportHandler> _localizer;

    private const decimal ZeroAmountTolerance = 0.005m;

    public ArAgingReportHandler(
        IReadRepository<CustomerInvoice> invoiceRepo,
        IReadRepository<Customer> customerRepo,
        IReadRepository<CustomerPaymentApplication> paymentAppRepo,
        IReadRepository<CreditMemoApplication> creditMemoAppRepo
        /* IStringLocalizer<ArAgingReportHandler> localizer */)
    {
        _invoiceRepo = invoiceRepo;
        _customerRepo = customerRepo;
        _paymentAppRepo = paymentAppRepo;
        _creditMemoAppRepo = creditMemoAppRepo;
        // _localizer = localizer;
    }

    public async Task<ArAgingReportDto> Handle(ArAgingReportRequest request, CancellationToken cancellationToken)
    {
        var reportDto = new ArAgingReportDto
        {
            AsOfDate = request.AsOfDate,
            CustomerId = request.CustomerId,
            GeneratedOn = DateTime.UtcNow.ToString("o"),
            Totals = new ArAgingReportTotalsDto()
        };

        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepo.GetByIdAsync(request.CustomerId.Value, cancellationToken);
            if (customer == null)
                throw new NotFoundException($"Customer with ID {request.CustomerId.Value} not found.");
            reportDto.CustomerName = customer.Name;
        }

        // 1. Fetch Invoices
        var invoicesSpec = new CustomerInvoicesForAgingSpec(request.AsOfDate, request.CustomerId);
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
            .ToDictionary(g => g.Key, g => g.Sum(app => app.AmountApplied));

        // 3. Fetch Relevant Credit Memo Applications
        var creditAppsSpec = new CreditApplicationsForCustomerInvoicesUpToDateSpec(invoiceIds, request.AsOfDate);
        var allCreditApps = await _creditMemoAppRepo.ListAsync(creditAppsSpec, cancellationToken);
        var creditsByInvoiceId = allCreditApps
            .GroupBy(app => app.CustomerInvoiceId)
            .ToDictionary(g => g.Key, g => g.Sum(app => app.AmountApplied));

        // 4. Process Invoices and Populate Lines
        foreach (var invoice in candidateInvoices)
        {
            decimal sumOfPayments = paymentsByInvoiceId.TryGetValue(invoice.Id, out var pVal) ? pVal : 0m;
            decimal sumOfCredits = creditsByInvoiceId.TryGetValue(invoice.Id, out var cVal) ? cVal : 0m;
            decimal totalAmountPaidOrCredited = sumOfPayments + sumOfCredits;
            decimal amountDue = invoice.TotalAmount - totalAmountPaidOrCredited;

            if (amountDue <= ZeroAmountTolerance) // Skip fully paid or overpaid invoices
            {
                continue;
            }

            var line = new ArAgingReportLineDto
            {
                CustomerId = invoice.CustomerId,
                CustomerName = invoice.Customer?.Name ?? reportDto.CustomerName ?? "N/A", // Customer should be included by spec
                CustomerInvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                TotalInvoiceAmount = invoice.TotalAmount,
                AmountPaid = totalAmountPaidOrCredited,
                AmountDue = amountDue
            };

            line.DaysDueOrOverdue = (invoice.DueDate.Date - request.AsOfDate.Date).Days;
            int daysOverduePositive = -line.DaysDueOrOverdue;

            if (line.DaysDueOrOverdue >= 0) // Current or not yet due
            {
                line.AgingBucket = "Current";
                line.CurrentAmount = amountDue;
                reportDto.Totals.TotalCurrentAmount += amountDue;
            }
            else // Overdue
            {
                if (daysOverduePositive <= 30)
                {
                    line.AgingBucket = "1-30 Days";
                    line.Overdue1To30DaysAmount = amountDue;
                    reportDto.Totals.TotalOverdue1To30DaysAmount += amountDue;
                }
                else if (daysOverduePositive <= 60)
                {
                    line.AgingBucket = "31-60 Days";
                    line.Overdue31To60DaysAmount = amountDue;
                    reportDto.Totals.TotalOverdue31To60DaysAmount += amountDue;
                }
                else if (daysOverduePositive <= 90)
                {
                    line.AgingBucket = "61-90 Days";
                    line.Overdue61To90DaysAmount = amountDue;
                    reportDto.Totals.TotalOverdue61To90DaysAmount += amountDue;
                }
                else
                {
                    line.AgingBucket = "91+ Days";
                    line.Overdue91PlusDaysAmount = amountDue;
                    reportDto.Totals.TotalOverdue91PlusDaysAmount += amountDue;
                }
            }
            reportDto.Lines.Add(line);
        }

        // 5. Calculate Grand Totals
        reportDto.Totals.GrandTotalAmountDue = reportDto.Lines.Sum(l => l.AmountDue);
        reportDto.Totals.TotalInvoices = reportDto.Lines.Count;
        reportDto.Totals.TotalCustomers = reportDto.Lines.Select(l => l.CustomerId).Distinct().Count();

        // Sort lines
        reportDto.Lines = reportDto.Lines.OrderBy(l => l.CustomerName).ThenBy(l => l.DueDate).ToList();

        return reportDto;
    }
}
