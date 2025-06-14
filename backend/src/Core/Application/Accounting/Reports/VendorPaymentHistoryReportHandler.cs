using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Accounting.VendorPayments.Specifications; // For VendorPaymentsForHistoryReportSpec
using FSH.WebApi.Application.Accounting.VendorInvoices.Specifications;  // For VendorInvoicesByIdsSpec
using FSH.WebApi.Application.Common.Exceptions; // For NotFoundException
// using Microsoft.Extensions.Localization; // If needed

namespace FSH.WebApi.Application.Accounting.Reports;

public class VendorPaymentHistoryReportHandler : IRequestHandler<VendorPaymentHistoryReportRequest, VendorPaymentHistoryReportDto>
{
    private readonly IReadRepository<VendorPayment> _paymentRepo;
    private readonly IReadRepository<Supplier> _supplierRepo;
    private readonly IReadRepository<PaymentMethod> _paymentMethodRepo;
    private readonly IReadRepository<VendorInvoice> _invoiceRepo;
    // private readonly IStringLocalizer<VendorPaymentHistoryReportHandler> _localizer;

    public VendorPaymentHistoryReportHandler(
        IReadRepository<VendorPayment> paymentRepo,
        IReadRepository<Supplier> supplierRepo,
        IReadRepository<PaymentMethod> paymentMethodRepo,
        IReadRepository<VendorInvoice> invoiceRepo
        /* IStringLocalizer<VendorPaymentHistoryReportHandler> localizer */)
    {
        _paymentRepo = paymentRepo;
        _supplierRepo = supplierRepo;
        _paymentMethodRepo = paymentMethodRepo;
        _invoiceRepo = invoiceRepo;
        // _localizer = localizer;
    }

    public async Task<VendorPaymentHistoryReportDto> Handle(VendorPaymentHistoryReportRequest request, CancellationToken cancellationToken)
    {
        var reportDto = new VendorPaymentHistoryReportDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            SupplierId = request.SupplierId,
            PaymentMethodId = request.PaymentMethodId,
            GeneratedOn = DateTime.UtcNow.ToString("o"), // Set directly
            Payments = new List<VendorPaymentHistoryLineDto>()
        };

        if (request.SupplierId.HasValue)
        {
            var supplier = await _supplierRepo.GetByIdAsync(request.SupplierId.Value, cancellationToken);
            if (supplier == null)
                throw new NotFoundException($"Supplier with ID {request.SupplierId.Value} not found.");
            reportDto.SupplierName = supplier.Name;
        }

        if (request.PaymentMethodId.HasValue)
        {
            var paymentMethod = await _paymentMethodRepo.GetByIdAsync(request.PaymentMethodId.Value, cancellationToken);
            if (paymentMethod == null)
                throw new NotFoundException($"Payment Method with ID {request.PaymentMethodId.Value} not found.");
            reportDto.PaymentMethodName = paymentMethod.Name;
        }

        // 1. Fetch Payments
        var paymentsSpec = new VendorPaymentsForHistoryReportSpec(request.StartDate, request.EndDate, request.SupplierId, request.PaymentMethodId);
        var payments = await _paymentRepo.ListAsync(paymentsSpec, cancellationToken);

        if (!payments.Any())
        {
            return reportDto; // Return empty report if no payments match filters
        }

        // 2. Prepare Invoice Details (Optimization)
        var allAppliedInvoiceIds = payments
            .SelectMany(p => p.AppliedInvoices)
            .Select(app => app.VendorInvoiceId)
            .Distinct()
            .ToList();

        Dictionary<Guid, VendorInvoice> relatedInvoicesMap = new Dictionary<Guid, VendorInvoice>();
        if (allAppliedInvoiceIds.Any())
        {
            var invoicesSpec = new VendorInvoicesByIdsSpec(allAppliedInvoiceIds);
            var relatedInvoices = await _invoiceRepo.ListAsync(invoicesSpec, cancellationToken);
            relatedInvoicesMap = relatedInvoices.ToDictionary(inv => inv.Id);
        }

        // 3. Populate Report Lines
        reportDto.TotalPaymentsAmount = 0m;

        foreach (var payment in payments)
        {
            var line = new VendorPaymentHistoryLineDto
            {
                VendorPaymentId = payment.Id,
                PaymentDate = payment.PaymentDate,
                SupplierId = payment.SupplierId,
                SupplierName = payment.Supplier?.Name ?? "N/A", // Supplier should be included by spec
                AmountPaid = payment.AmountPaid,
                PaymentMethodId = payment.PaymentMethodId,
                PaymentMethodName = payment.PaymentMethod?.Name ?? "N/A", // PaymentMethod should be included
                ReferenceNumber = payment.ReferenceNumber,
                Notes = payment.Notes,
                AppliedInvoices = new List<VendorPaymentAppliedInvoiceDto>()
            };

            foreach (var application in payment.AppliedInvoices)
            {
                if (relatedInvoicesMap.TryGetValue(application.VendorInvoiceId, out var appliedInvoice))
                {
                    line.AppliedInvoices.Add(new VendorPaymentAppliedInvoiceDto
                    {
                        VendorInvoiceId = application.VendorInvoiceId,
                        InvoiceNumber = appliedInvoice.InvoiceNumber,
                        InvoiceDate = appliedInvoice.InvoiceDate,
                        InvoiceTotalAmount = appliedInvoice.TotalAmount,
                        AmountAppliedToInvoice = application.AmountApplied
                    });
                }
                // Else: Invoice details not found for an application, might indicate data integrity issue or log a warning.
            }

            reportDto.Payments.Add(line);
            reportDto.TotalPaymentsAmount += payment.AmountPaid;
        }

        // Sorting already handled by spec, but if additional client-side sort needed:
        // reportDto.Payments = reportDto.Payments.OrderByDescending(p => p.PaymentDate).ThenBy(p => p.SupplierName).ToList();

        return reportDto;
    }
}
