using FSH.WebApi.Application.Common.Models;
using MediatR;
using System; // Required for Guid

namespace FSH.WebApi.Application.Accounting.VendorInvoices;

public class SearchVendorInvoicesRequest : PaginationFilter, IRequest<PaginationResponse<VendorInvoiceDto>>
{
    public string? Keyword { get; set; } // For searching in InvoiceNumber, SupplierName (requires join)
    public Guid? SupplierId { get; set; }
    public string? InvoiceStatus { get; set; } // To filter by VendorInvoiceStatus enum (parsed in handler/spec)
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
}
