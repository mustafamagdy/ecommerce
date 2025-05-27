using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.Budgets;

public class DeleteBudgetRequest : IRequest<Guid>
{
    [Required]
    public Guid Id { get; set; }

    public DeleteBudgetRequest(Guid id)
    {
        Id = id;
    }
}
