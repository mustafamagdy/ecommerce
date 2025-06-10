using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class DisposeFixedAssetRequest : IRequest<Guid> // Returns the ID of the disposed asset
{
    [Required]
    public Guid FixedAssetId { get; set; }

    [Required]
    public DateTime DisposalDate { get; set; }

    [MaxLength(500)]
    public string? DisposalReason { get; set; } // E.g., "Sold", "Scrapped", "Damaged"

    [Range(0, double.MaxValue)]
    public decimal? DisposalAmount { get; set; } // Proceeds from disposal, if any
}
