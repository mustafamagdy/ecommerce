using FSH.WebApi.Application.Common.Models;
using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.CustomerInvoices;

public class SearchCustomerInvoicesRequest : PaginationFilter, IRequest<PaginationResponse<CustomerInvoiceDto>>
{
    public Guid? CustomerId { get; set; }
    public Guid? OrderId { get; set; }
    public string? InvoiceNumberKeyword { get; set; } // Search within InvoiceNumber
    public DateTime? InvoiceDateFrom { get; set; }
    public DateTime? InvoiceDateTo { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public string? Status { get; set; } // To filter by CustomerInvoiceStatus enum (parsed in handler/spec)
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
}
