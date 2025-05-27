using FSH.WebApi.Application.Accounting.Ledgers; // For AccountLedgerDto
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Application.Accounting.Ledgers;

public class GetAccountLedgerRequest : IRequest<AccountLedgerDto>
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    public GetAccountLedgerRequest(Guid accountId, DateTime fromDate, DateTime toDate)
    {
        AccountId = accountId;
        FromDate = fromDate;
        ToDate = toDate;
    }
}
