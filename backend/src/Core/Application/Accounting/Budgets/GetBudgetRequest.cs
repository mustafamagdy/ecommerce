using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.Budgets;

public class GetBudgetRequest : IRequest<BudgetDto>
{
    [Required]
    public Guid Id { get; set; }

    public GetBudgetRequest(Guid id)
    {
        Id = id;
    }
}
