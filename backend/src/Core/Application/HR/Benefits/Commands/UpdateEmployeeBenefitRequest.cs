using FSH.WebApi.Domain.HR.Enums; // For EmployeeBenefitStatus
using MediatR;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class UpdateEmployeeBenefitRequest : IRequest<Guid> // Returns EmployeeBenefit.Id
{
    public Guid Id { get; set; } // EmployeeBenefitId

    public DateTime? EnrollmentDate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? TerminationDate { get; set; } // Use null to clear termination date

    public decimal? EmployeeContributionOverride { get; set; } // Use null to revert to plan default
    public decimal? EmployerContributionOverride { get; set; } // Use null to revert to plan default

    public EmployeeBenefitStatus? Status { get; set; }
    public string? Notes { get; set; } // Can be used to append or overwrite notes
}
