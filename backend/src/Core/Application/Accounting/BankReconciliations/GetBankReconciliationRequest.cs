using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.BankReconciliations;

public class GetBankReconciliationRequest : IRequest<BankReconciliationDto>
{
    public Guid Id { get; set; }

    public GetBankReconciliationRequest(Guid id)
    {
        Id = id;
    }
}
