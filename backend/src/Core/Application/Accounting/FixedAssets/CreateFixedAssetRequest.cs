using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using FSH.WebApi.Domain.Accounting; // For FixedAssetStatus default

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class CreateFixedAssetRequest : IRequest<Guid>
{
    // AssetNumber might be auto-generated or user-provided. Assuming user-provided for now.
    [Required]
    [MaxLength(50)]
    public string AssetNumber { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string AssetName { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid AssetCategoryId { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal PurchaseCost { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal SalvageValue { get; set; }

    [Required]
    [Range(1, 100)] // Useful life in years
    public int UsefulLifeYears { get; set; }

    [Required]
    public Guid DepreciationMethodId { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    // Status will likely default in the domain entity or handler.
    // public FixedAssetStatus Status { get; set; } = FixedAssetStatus.Active;
}
