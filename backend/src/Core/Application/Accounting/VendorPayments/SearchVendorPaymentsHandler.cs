using FSH.WebApi.Application.Common.Models;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Accounting;
using MediatR;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;

namespace FSH.WebApi.Application.Accounting.VendorPayments;

public class SearchVendorPaymentsHandler : IRequestHandler<SearchVendorPaymentsRequest, PaginationResponse<VendorPaymentDto>>
{
    private readonly IReadRepository<VendorPayment> _paymentRepository;
    private readonly IStringLocalizer<SearchVendorPaymentsHandler> _localizer;

    public SearchVendorPaymentsHandler(
        IReadRepository<VendorPayment> paymentRepository,
        IStringLocalizer<SearchVendorPaymentsHandler> localizer)
    {
        _paymentRepository = paymentRepository;
        _localizer = localizer;
    }

    public async Task<PaginationResponse<VendorPaymentDto>> Handle(SearchVendorPaymentsRequest request, CancellationToken cancellationToken)
    {
        var spec = new VendorPaymentsBySearchFilterSpec(request);

        var payments = await _paymentRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _paymentRepository.CountAsync(spec, cancellationToken);

        var dtos = new List<VendorPaymentDto>();
        foreach (var payment in payments)
        {
            var dto = payment.Adapt<VendorPaymentDto>();

            // Populate related names - these should be loaded by the spec's Includes
            if (payment.Supplier != null)
            {
                dto.SupplierName = payment.Supplier.Name;
            }
            if (payment.PaymentMethod != null)
            {
                dto.PaymentMethodName = payment.PaymentMethod.Name;
            }

            foreach (var appDto in dto.AppliedInvoices)
            {
                var appEntity = payment.AppliedInvoices.FirstOrDefault(a => a.Id == appDto.Id);
                if (appEntity?.VendorInvoice != null)
                {
                    appDto.VendorInvoiceNumber = appEntity.VendorInvoice.InvoiceNumber;
                }
            }
            // dto.CreatedOn = payment.CreatedOn; // Should be mapped by Adapt

            dtos.Add(dto);
        }

        return new PaginationResponse<VendorPaymentDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
