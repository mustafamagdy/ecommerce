using FSH.WebApi.Application.Common.Interfaces;
using System;

namespace FSH.WebApi.Application.Accounting.Suppliers;

public class SupplierDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? ContactInfo { get; set; }
    public string? Address { get; set; }
    public string? TaxId { get; set; }
    public Guid? DefaultPaymentTermId { get; set; }
    // public string? DefaultPaymentTermName { get; set; } // Consider adding if useful for display
    public string? BankDetails { get; set; }
}
