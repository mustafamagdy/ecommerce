using MediatR;
using System;

namespace FSH.WebApi.Application.Accounting.Reports;

public enum CreditMemoRegisterStatusFilter
{
    All,
    Draft,
    Approved,         // Approved but not yet applied at all or only partially
    PartiallyApplied, // Some amount applied, but not fully
    Applied,          // Fully applied (AvailableBalance is zero or negligible)
    Void
}

public class CreditMemoRegisterRequest : IRequest<CreditMemoRegisterDto>
{
    public DateTime? StartDate { get; set; } // Filter by CreditMemo.Date
    public DateTime? EndDate { get; set; }   // Filter by CreditMemo.Date
    public Guid? CustomerId { get; set; }
    public CreditMemoRegisterStatusFilter StatusFilter { get; set; } = CreditMemoRegisterStatusFilter.All;
    // AsOfDate is not strictly needed if we consider the latest state of credit memos,
    // but could be added if point-in-time application status was required.
    // For a register, current status is usually sufficient.
}
