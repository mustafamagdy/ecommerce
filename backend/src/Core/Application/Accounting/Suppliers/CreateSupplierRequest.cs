using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.Suppliers;

public class CreateSupplierRequest : IRequest<Guid>
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = default!;

    [MaxLength(256)]
    public string? ContactInfo { get; set; }

    [MaxLength(1024)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? TaxId { get; set; }

    public Guid? DefaultPaymentTermId { get; set; }

    [MaxLength(1024)]
    public string? BankDetails { get; set; }
}
