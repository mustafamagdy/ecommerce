using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.VendorInvoices.Specifications;
using FSH.WebApi.Application.Accounting.VendorPayments.Specifications; // For PaymentApplicationsForInvoicesUpToDateSpec
using FSH.WebApi.Application.Common.Exceptions;
// using Microsoft.Extensions.Localization; // If needed

namespace FSH.WebApi.Application.Accounting.Reports;

public class VendorInvoiceRegisterHandler : IRequestHandler<VendorInvoiceRegisterRequest, VendorInvoiceRegisterDto>
{
    private readonly IReadRepository<VendorInvoice> _invoiceRepo;
    private readonly IReadRepository<Supplier> _supplierRepo;
    private readonly IReadRepository<VendorPaymentApplication> _paymentAppRepo;
    // private readonly IStringLocalizer<VendorInvoiceRegisterHandler> _localizer;

    private const decimal ZeroAmountTolerance = 0.005m; // Tolerance for considering an amount as zero

    public VendorInvoiceRegisterHandler(
        IReadRepository<VendorInvoice> invoiceRepo,
        IReadRepository<Supplier> supplierRepo,
        IReadRepository<VendorPaymentApplication> paymentAppRepo
        /* IStringLocalizer<VendorInvoiceRegisterHandler> localizer */)
    {
        _invoiceRepo = invoiceRepo;
        _supplierRepo = supplierRepo;
        _paymentAppRepo = paymentAppRepo;
        // _localizer = localizer;
    }

    public async Task<VendorInvoiceRegisterDto> Handle(VendorInvoiceRegisterRequest request, CancellationToken cancellationToken)
    {
        var reportDto = new VendorInvoiceRegisterDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            SupplierId = request.SupplierId,
            StatusFilter = request.StatusFilter.ToString(),
            AsOfDate = request.AsOfDate,
            GeneratedOn = DateTime.UtcNow.ToString("o"),
            Invoices = new List<VendorInvoiceRegisterLineDto>()
        };

        if (request.SupplierId.HasValue)
        {
            var supplier = await _supplierRepo.GetByIdAsync(request.SupplierId.Value, cancellationToken);
            if (supplier == null)
                throw new NotFoundException($"Supplier with ID {request.SupplierId.Value} not found.");
            reportDto.SupplierName = supplier.Name;
        }

        // 1. Fetch Invoices based on primary filters (date range, supplier)
        var invoicesSpec = new VendorInvoicesForRegisterSpec(request.StartDate, request.EndDate, request.SupplierId);
        var candidateInvoices = await _invoiceRepo.ListAsync(invoicesSpec, cancellationToken);

        if (!candidateInvoices.Any())
        {
            return reportDto; // Return empty report
        }

        // 2. Fetch Relevant Payment Applications for all candidate invoices up to AsOfDate
        var invoiceIds = candidateInvoices.Select(i => i.Id).ToList();
        var paymentAppsSpec = new PaymentApplicationsForInvoicesUpToDateSpec(invoiceIds, request.AsOfDate);
        var allRelevantPaymentApps = await _paymentAppRepo.ListAsync(paymentAppsSpec, cancellationToken);
        var applicationsByInvoiceId = allRelevantPaymentApps
            .GroupBy(app => app.VendorInvoiceId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 3. Process Invoices and Populate Lines
        foreach (var invoice in candidateInvoices)
        {
            decimal amountPaidAsOfDate = 0m;
            DateTime? lastPaymentDate = null;

            if (applicationsByInvoiceId.TryGetValue(invoice.Id, out var appsForThisInvoice))
            {
                amountPaidAsOfDate = appsForThisInvoice.Sum(app => app.AmountApplied);
                if (appsForThisInvoice.Any())
                {
                    lastPaymentDate = appsForThisInvoice.Max(app => app.VendorPayment.PaymentDate);
                }
            }

            decimal amountDue = invoice.TotalAmount - amountPaidAsOfDate;
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
                calculatedStatus = amountPaidAsOfDate > ZeroAmountTolerance ? "Partially Paid" : "Unpaid";
            }


            // Apply Status Filter
            bool includeInReport = false;
            switch (request.StatusFilter)
            {
                case VendorInvoiceRegisterStatusFilter.All:
                    includeInReport = true;
                    break;
                case VendorInvoiceRegisterStatusFilter.Open: // Not fully paid
                    if (!isEffectivelyPaid) includeInReport = true;
                    break;
                case VendorInvoiceRegisterStatusFilter.Paid:
                    if (isEffectivelyPaid) includeInReport = true;
                    break;
                case VendorInvoiceRegisterStatusFilter.Overdue:
                    if (isOverdue) includeInReport = true;
                    break;
                case VendorInvoiceRegisterStatusFilter.Unpaid: // TotalAmount > 0 and AmountPaid effectively zero
                    if (invoice.TotalAmount > ZeroAmountTolerance && amountPaidAsOfDate <= ZeroAmountTolerance) includeInReport = true;
                    break;
            }

            if (!includeInReport)
            {
                continue;
            }

            var line = new VendorInvoiceRegisterLineDto
            {
                VendorInvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                SupplierId = invoice.SupplierId,
                SupplierName = invoice.Supplier?.Name ?? "N/A", // Supplier should be included by spec
                SupplierContactInfo = invoice.Supplier?.ContactInfo, // Assuming ContactInfo exists
                TotalAmount = invoice.TotalAmount,
                AmountPaid = amountPaidAsOfDate,
                AmountDue = amountDue,
                Status = calculatedStatus,
                LastPaymentDate = lastPaymentDate,
                DaysOverdue = isOverdue ? (request.AsOfDate.Date - invoice.DueDate.Date).Days : 0
            };
            reportDto.Invoices.Add(line);

            reportDto.GrandTotalAmount += line.TotalAmount;
            reportDto.GrandTotalPaidAmount += line.AmountPaid;
            reportDto.GrandTotalAmountDue += line.AmountDue;
        }

        // Sorting already handled by spec, but if further client-side sort needed:
        // reportDto.Invoices = reportDto.Invoices.OrderBy(l => l.SupplierName).ThenBy(l => l.InvoiceDate).ToList();

        return reportDto;
    }
}
