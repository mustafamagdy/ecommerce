using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using FSH.WebApi.Domain.Accounting; // For FixedAssetStatus

namespace FSH.WebApi.Application.Accounting.FixedAssets;

public class UpdateFixedAssetRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; }

    // AssetNumber is typically not updatable.
    // public string? AssetNumber { get; set; }

    [MaxLength(100)]
    public string? AssetName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? AssetCategoryId { get; set; }
    public DateTime? PurchaseDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PurchaseCost { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? SalvageValue { get; set; }

    [Range(1, 100)]
    public int? UsefulLifeYears { get; set; }

    public Guid? DepreciationMethodId { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public FixedAssetStatus? Status { get; set; }
}
