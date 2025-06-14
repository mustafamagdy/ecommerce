using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.VendorInvoices.Specifications; // For VendorInvoicesForAgingSpec
using FSH.WebApi.Application.Accounting.VendorPayments.Specifications; // For PaymentApplicationsForInvoicesUpToDateSpec
using FSH.WebApi.Application.Common.Exceptions; // For NotFoundException
// using Microsoft.Extensions.Localization; // If needed

namespace FSH.WebApi.Application.Accounting.Reports;

public class ApAgingReportHandler : IRequestHandler<ApAgingReportRequest, ApAgingReportDto>
{
    private readonly IReadRepository<VendorInvoice> _invoiceRepo;
    private readonly IReadRepository<Supplier> _supplierRepo;
    private readonly IReadRepository<VendorPaymentApplication> _paymentAppRepo;
    // private readonly IStringLocalizer<ApAgingReportHandler> _localizer;

    public ApAgingReportHandler(
        IReadRepository<VendorInvoice> invoiceRepo,
        IReadRepository<Supplier> supplierRepo,
        IReadRepository<VendorPaymentApplication> paymentAppRepo
        /* IStringLocalizer<ApAgingReportHandler> localizer */)
    {
        _invoiceRepo = invoiceRepo;
        _supplierRepo = supplierRepo;
        _paymentAppRepo = paymentAppRepo;
        // _localizer = localizer;
    }

    public async Task<ApAgingReportDto> Handle(ApAgingReportRequest request, CancellationToken cancellationToken)
    {
        var reportDto = new ApAgingReportDto
        {
            AsOfDate = request.AsOfDate,
            GeneratedOn = DateTime.UtcNow.ToString("o"), // Set directly as per DTO design
            Totals = new ApAgingReportTotalsDto() // Initialize totals
        };

        if (request.SupplierId.HasValue)
        {
            var supplier = await _supplierRepo.GetByIdAsync(request.SupplierId.Value, cancellationToken);
            if (supplier == null)
                throw new NotFoundException($"Supplier with ID {request.SupplierId.Value} not found.");
            reportDto.FilterSupplierId = supplier.Id;
            reportDto.FilterSupplierName = supplier.Name;
        }

        // 1. Fetch Invoices
        var invoicesSpec = new VendorInvoicesForAgingSpec(request.AsOfDate, request.SupplierId);
        var invoices = await _invoiceRepo.ListAsync(invoicesSpec, cancellationToken);

        if (!invoices.Any())
        {
            return reportDto; // Return empty report if no relevant invoices
        }

        // 2. Get all relevant payment applications in one go for efficiency
        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var paymentAppsSpec = new PaymentApplicationsForInvoicesUpToDateSpec(invoiceIds, request.AsOfDate);
        var allRelevantPaymentApps = await _paymentAppRepo.ListAsync(paymentAppsSpec, cancellationToken);

        var paymentAppsByInvoiceId = allRelevantPaymentApps.GroupBy(app => app.VendorInvoiceId)
                                                          .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var invoice in invoices)
        {
            // Calculate Amount Paid for this invoice as of AsOfDate
            decimal totalAmountPaidAsOfDate = 0m;
            if (paymentAppsByInvoiceId.TryGetValue(invoice.Id, out var appsForThisInvoice))
            {
                totalAmountPaidAsOfDate = appsForThisInvoice.Sum(app => app.AmountApplied);
            }

            decimal amountDue = invoice.TotalAmount - totalAmountPaidAsOfDate;

            if (amountDue <= 0.009m) // Using a small tolerance for effectively zero or negative (overpaid)
            {
                continue; // Skip fully paid or overpaid invoices
            }

            var line = new ApAgingReportLineDto
            {
                SupplierId = invoice.SupplierId,
                SupplierName = invoice.Supplier?.Name ?? "N/A", // Supplier should be included by spec
                VendorInvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                TotalInvoiceAmount = invoice.TotalAmount,
                AmountPaid = totalAmountPaidAsOfDate,
                AmountDue = amountDue
            };

            line.DaysDue = (invoice.DueDate.Date - request.AsOfDate.Date).Days;
            int daysOverdue = -line.DaysDue; // Overdue days are positive

            if (line.DaysDue >= 0) // Current or not yet due
            {
                line.AgingBucket = "Current";
                line.CurrentAmount = amountDue;
                reportDto.Totals.TotalCurrentAmount += amountDue;
            }
            else // Overdue
            {
                if (daysOverdue <= 30)
                {
                    line.AgingBucket = "1-30 Days";
                    line.Overdue1To30DaysAmount = amountDue;
                    reportDto.Totals.TotalOverdue1To30DaysAmount += amountDue;
                }
                else if (daysOverdue <= 60)
                {
                    line.AgingBucket = "31-60 Days";
                    line.Overdue31To60DaysAmount = amountDue;
                    reportDto.Totals.TotalOverdue31To60DaysAmount += amountDue;
                }
                else if (daysOverdue <= 90)
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

        // 4. Calculate Grand Totals
        reportDto.Totals.GrandTotalAmountDue = reportDto.Lines.Sum(l => l.AmountDue);
        reportDto.Totals.TotalInvoices = reportDto.Lines.Count;
        reportDto.Totals.TotalSuppliers = reportDto.Lines.Select(l => l.SupplierId).Distinct().Count();

        // Sort lines
        reportDto.Lines = reportDto.Lines.OrderBy(l => l.SupplierName).ThenBy(l => l.DueDate).ToList();

        return reportDto;
    }
}
