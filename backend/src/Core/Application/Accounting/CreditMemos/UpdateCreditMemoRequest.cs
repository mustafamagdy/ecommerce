using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using FSH.WebApi.Domain.Accounting; // For CreditMemoStatus, if status update is allowed here

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class UpdateCreditMemoRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; }

    // CustomerId is typically not changed.
    // public Guid? CustomerId { get; set; }

    public DateTime? Date { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    // TotalAmount might be restricted from update if already partially/fully applied.
    [Range(0.01, double.MaxValue, ErrorMessage = "Total Amount must be greater than zero.")]
    public decimal? TotalAmount { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public Guid? OriginalCustomerInvoiceId { get; set; } // Allow updating this link if necessary

    // Status updates should ideally be via specific actions (Approve, Void, etc.)
    // public CreditMemoStatus? Status { get; set; }
}
