using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.VendorPayments;

public class GetVendorPaymentHandler : IRequestHandler<GetVendorPaymentRequest, VendorPaymentDto>
{
    private readonly IReadRepository<VendorPayment> _paymentRepository;
    private readonly IStringLocalizer<GetVendorPaymentHandler> _localizer;

    public GetVendorPaymentHandler(
        IReadRepository<VendorPayment> paymentRepository,
        IStringLocalizer<GetVendorPaymentHandler> localizer)
    {
        _paymentRepository = paymentRepository;
        _localizer = localizer;
    }

    public async Task<VendorPaymentDto> Handle(GetVendorPaymentRequest request, CancellationToken cancellationToken)
    {
        var spec = new VendorPaymentByIdWithDetailsSpec(request.Id);
        var vendorPayment = await _paymentRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (vendorPayment == null)
        {
            throw new NotFoundException(_localizer["Vendor Payment with ID {0} not found.", request.Id]);
        }

        // Mapster should handle the base mapping. We then populate related names.
        var dto = vendorPayment.Adapt<VendorPaymentDto>();

        // SupplierName and PaymentMethodName should be populated by Mapster if navigation properties are correctly loaded by the spec
        // and if Mapster is configured for such projections or if DTO properties match navigation property names (e.g. Supplier.Name -> SupplierName)
        // For clarity, explicitly set them if not automatically mapped:
        if (vendorPayment.Supplier != null)
        {
            dto.SupplierName = vendorPayment.Supplier.Name;
        }
        if (vendorPayment.PaymentMethod != null)
        {
            dto.PaymentMethodName = vendorPayment.PaymentMethod.Name;
        }

        // Populate VendorInvoiceNumber for each application
        foreach (var appDto in dto.AppliedInvoices)
        {
            var appEntity = vendorPayment.AppliedInvoices.FirstOrDefault(a => a.Id == appDto.Id);
            if (appEntity?.VendorInvoice != null)
            {
                appDto.VendorInvoiceNumber = appEntity.VendorInvoice.InvoiceNumber;
            }
        }
        // CreatedOn should be mapped by Adapt from AuditableEntity
        // dto.CreatedOn = vendorPayment.CreatedOn;


        return dto;
    }
}
