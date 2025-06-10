using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.DepreciationMethods;

public class CreateDepreciationMethodRequest : IRequest<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(256)]
    public string? Description { get; set; }
}
