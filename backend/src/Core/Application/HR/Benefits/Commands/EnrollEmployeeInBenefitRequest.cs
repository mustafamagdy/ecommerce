using MediatR;

namespace FSH.WebApi.Application.HR.Benefits.Commands;

public class EnrollEmployeeInBenefitRequest : IRequest<Guid> // Returns EmployeeBenefit.Id
{
    public Guid EmployeeId { get; set; }
    public Guid BenefitPlanId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public decimal? EmployeeContributionOverride { get; set; }
    public decimal? EmployerContributionOverride { get; set; }
    public string? Notes { get; set; }
}
