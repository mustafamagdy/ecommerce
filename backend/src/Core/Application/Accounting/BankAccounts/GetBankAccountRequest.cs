using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.BankAccounts;

public class GetBankAccountRequest : IRequest<BankAccountDto>
{
    public Guid Id { get; set; }

    public GetBankAccountRequest(Guid id)
    {
        Id = id;
    }
}
