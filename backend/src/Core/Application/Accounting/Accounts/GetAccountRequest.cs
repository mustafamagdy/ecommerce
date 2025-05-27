using MediatR;
using System;
using System.ComponentModel.DataAnnotations; // Added for RequiredAttribute

namespace FSH.WebApi.Application.Accounting.Accounts;

public class GetAccountRequest : IRequest<AccountDto>
{
    [Required]
    public Guid Id { get; set; }

    public GetAccountRequest(Guid id)
    {
        Id = id;
    }
}
