using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.AssetCategories;

public class CreateAssetCategoryRequest : IRequest<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(256)]
    public string? Description { get; set; }

    public Guid? DefaultDepreciationMethodId { get; set; }

    [Range(1, 100, ErrorMessage = "Default Useful Life must be between 1 and 100 years if specified.")]
    public int? DefaultUsefulLifeYears { get; set; }

    public bool IsActive { get; set; } = true;
}
