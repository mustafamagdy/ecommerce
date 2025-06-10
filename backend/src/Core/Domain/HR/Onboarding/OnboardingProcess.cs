using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.HR.Enums; // For OnboardingProcessStatus
using FSH.WebApi.Domain.HR.Recruitment; // For Applicant, JobOpening (if needed for direct nav from here)

namespace FSH.WebApi.Domain.HR.Onboarding;

public class OnboardingProcess : AuditableEntity
{
    public Guid ApplicantId { get; set; } // From the hired applicant
    public virtual Applicant? Applicant { get; set; }

    public Guid? EmployeeId { get; set; } // To be filled when the actual Employee record is created
    public virtual Employee? Employee { get; set; } // Employee who was the applicant

    public Guid JobOpeningId { get; set; } // From the job opening the applicant was hired for
    public virtual JobOpening? JobOpening { get; set; }

    public DateTime HiredDate { get; set; }
    public DateTime? OnboardingStartDate { get; set; }
    public OnboardingProcessStatus Status { get; set; } = OnboardingProcessStatus.Pending;
    public string? Notes { get; set; }

    public List<OnboardingTask> Tasks { get; private set; } = new();

    public OnboardingProcess(Guid applicantId, Guid jobOpeningId, DateTime hiredDate)
    {
        ApplicantId = applicantId;
        JobOpeningId = jobOpeningId;
        HiredDate = hiredDate;
    }

    // Parameterless constructor for EF Core
    private OnboardingProcess() { }
}
