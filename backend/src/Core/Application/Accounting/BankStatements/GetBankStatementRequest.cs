using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.BankStatements;

public class GetBankStatementRequest : IRequest<BankStatementDto>
{
    public Guid Id { get; set; }

    public GetBankStatementRequest(Guid id)
    {
        Id = id;
    }
}
