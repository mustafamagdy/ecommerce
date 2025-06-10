using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class UpdateAssetCategoryRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(256)]
    public string? Description { get; set; }

    public Guid? DefaultDepreciationMethodId { get; set; } // Allow unsetting

    [Range(1, 100, ErrorMessage = "Default Useful Life must be between 1 and 100 years if specified.")]
    public int? DefaultUsefulLifeYears { get; set; } // Allow unsetting

    public bool? IsActive { get; set; }
}
