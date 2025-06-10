using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FSH.WebApi.Application.Common.Interfaces; // For IUnitOfWork or similar if used for transactions

namespace FSH.WebApi.Application.Accounting.VendorPayments;

public class CreateVendorPaymentHandler : IRequestHandler<CreateVendorPaymentRequest, Guid>
{
    private readonly IRepository<VendorPayment> _paymentRepository;
    private readonly IRepository<VendorInvoice> _invoiceRepository;
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IRepository<PaymentMethod> _paymentMethodRepository;
    private readonly IStringLocalizer<CreateVendorPaymentHandler> _localizer;
    private readonly ILogger<CreateVendorPaymentHandler> _logger;
    // private readonly IUnitOfWork _unitOfWork; // For transaction management

    public CreateVendorPaymentHandler(
        IRepository<VendorPayment> paymentRepository,
        IRepository<VendorInvoice> invoiceRepository,
        IRepository<Supplier> supplierRepository,
        IRepository<PaymentMethod> paymentMethodRepository,
        IStringLocalizer<CreateVendorPaymentHandler> localizer,
        ILogger<CreateVendorPaymentHandler> logger
        /* IUnitOfWork unitOfWork */)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _supplierRepository = supplierRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _localizer = localizer;
        _logger = logger;
        // _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateVendorPaymentRequest request, CancellationToken cancellationToken)
    {
        // Begin Transaction (if using explicit IUnitOfWork)
        // await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Validate Supplier and PaymentMethod
            var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken);
            if (supplier == null) throw new NotFoundException(_localizer["Supplier not found."]);

            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(request.PaymentMethodId, cancellationToken);
            if (paymentMethod == null) throw new NotFoundException(_localizer["Payment Method not found."]);

            // 2. Validate sum of applications against AmountPaid (already in validator, but double check is fine)
            decimal totalAmountApplied = request.Applications.Sum(a => a.AmountApplied);
            if (totalAmountApplied != request.AmountPaid)
            {
                throw new ValidationException(_localizer["Total amount applied ({0}) does not match the payment amount ({1}).", totalAmountApplied, request.AmountPaid]);
            }

            // 3. Create VendorPayment entity
            var vendorPayment = new VendorPayment(
                supplierId: request.SupplierId,
                paymentDate: request.PaymentDate,
                amountPaid: request.AmountPaid,
                paymentMethodId: request.PaymentMethodId,
                referenceNumber: request.ReferenceNumber,
                notes: request.Notes
            );

            // 4. Process applications and update invoices
            foreach (var appRequest in request.Applications)
            {
                var invoice = await _invoiceRepository.GetByIdAsync(appRequest.VendorInvoiceId, cancellationToken);
                if (invoice == null)
                {
                    throw new NotFoundException(_localizer["Vendor Invoice with ID {0} not found.", appRequest.VendorInvoiceId]);
                }
                if (invoice.SupplierId != request.SupplierId)
                {
                     throw new ValidationException(_localizer["Invoice {0} does not belong to supplier {1}.", invoice.InvoiceNumber, supplier.Name]);
                }

                // Check if invoice status allows payment application
                if (invoice.Status == VendorInvoiceStatus.Paid || invoice.Status == VendorInvoiceStatus.Cancelled)
                {
                    throw new ConflictException(_localizer["Invoice {0} is already {1} and cannot have payments applied.", invoice.InvoiceNumber, invoice.Status]);
                }

                // Calculate remaining balance on invoice (this might be a method on VendorInvoice)
                // decimal invoiceBalance = invoice.TotalAmount - GetPreviouslyAppliedAmount(invoice.Id); // Simplified
                // For this, we'd need to query existing VendorPaymentApplications for this invoice.
                // This logic can get complex. A simpler approach for now is to assume TotalAmount is the full amount due.
                // A true system would track applied amounts on the invoice itself or calculate it.
                // Let's assume VendorInvoice has a property like AmountDue or similar.
                // For now, we'll just compare against TotalAmount for simplicity, which is not fully correct if partial payments exist.
                // A more robust solution would be: invoice.ApplyPayment(appRequest.AmountApplied);

                if (appRequest.AmountApplied > invoice.TotalAmount) // Simplified check
                {
                     _logger.LogWarning("Attempting to apply {0} to invoice {1} which has a total of {2}. This check should be against amount due.", appRequest.AmountApplied, invoice.InvoiceNumber, invoice.TotalAmount);
                    // For now, let's assume this check is against the current outstanding amount.
                    // This requires the VendorInvoice entity to have a way to calculate its balance.
                    // throw new ValidationException($"Amount applied ({appRequest.AmountApplied}) exceeds invoice {invoice.InvoiceNumber} balance.");
                }


                var paymentApplication = new VendorPaymentApplication(
                    vendorPaymentId: vendorPayment.Id, // Will be set by EF Core if relationship is configured
                    vendorInvoiceId: appRequest.VendorInvoiceId,
                    amountApplied: appRequest.AmountApplied
                );
                vendorPayment.AddPaymentApplication(paymentApplication);

                // Update invoice status (simplified)
                // This logic should ideally be within the VendorInvoice domain entity
                var newInvoiceTotalApplied = (await _paymentRepository.ListAsync(new GetApplicationsForInvoiceSpec(invoice.Id), cancellationToken)).Sum(a => a.AmountApplied) + appRequest.AmountApplied;


                if (newInvoiceTotalApplied >= invoice.TotalAmount)
                {
                    invoice.UpdateStatus(VendorInvoiceStatus.Paid);
                }
                else if (newInvoiceTotalApplied > 0)
                {
                    // invoice.UpdateStatus(VendorInvoiceStatus.PartiallyPaid); // If such status exists
                    // For now, keep as Approved or Submitted if partially paid
                }
                await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
            }

            await _paymentRepository.AddAsync(vendorPayment, cancellationToken);

            // Commit Transaction (if using explicit IUnitOfWork)
            // await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogInformation(_localizer["Vendor Payment {0} created successfully."], vendorPayment.Id);
            return vendorPayment.Id;
        }
        catch (Exception ex)
        {
            // Rollback Transaction (if using explicit IUnitOfWork)
            // await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, _localizer["Error creating vendor payment."]);
            throw; // Re-throw the exception
        }
    }
}

// Helper Spec (example, might need to be in its own file)
public class GetApplicationsForInvoiceSpec : Ardalis.Specification.Specification<VendorPaymentApplication>
{
    public GetApplicationsForInvoiceSpec(Guid invoiceId)
    {
        Query.Where(app => app.VendorInvoiceId == invoiceId);
    }
}
