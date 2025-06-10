using MediatR;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class TerminateEmployeeBenefitRequest : IRequest<Guid> // Returns EmployeeBenefit.Id
{
    public Guid EmployeeBenefitId { get; set; }
    public DateTime TerminationDate { get; set; }
    public string? Reason { get; set; } // Optional, maps to Notes
}
