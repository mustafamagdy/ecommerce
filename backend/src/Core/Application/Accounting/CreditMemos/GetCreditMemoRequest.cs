using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.CreditMemos;

public class GetCreditMemoRequest : IRequest<CreditMemoDto>
{
    public Guid Id { get; set; }

    public GetCreditMemoRequest(Guid id)
    {
        Id = id;
    }
}
