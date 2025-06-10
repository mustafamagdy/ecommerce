using FSH.WebApi.Application.Common.Interfaces;
using System;

namespace FSH.WebApi.Application.Accounting.PaymentTerms;

public class PaymentTermDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DaysUntilDue { get; set; } // Changed from 'Days' to match existing PaymentTerm.cs
    public bool IsActive { get; set; } // Assuming PaymentTerm.cs has IsActive, if not, will add to entity
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
