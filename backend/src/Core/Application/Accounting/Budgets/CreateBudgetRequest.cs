using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.Budgets;

public class CreateBudgetRequest : IRequest<Guid>
{
    [Required]
    [MaxLength(256)]
    public string BudgetName { get; set; } = default!;

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public DateTime PeriodStartDate { get; set; }

    [Required]
    public DateTime PeriodEndDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
    public decimal Amount { get; set; }

    [MaxLength(1024)]
    public string? Description { get; set; }
}
