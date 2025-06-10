// Using domain enums via global using FSH.WebApi.Domain.HR.Enums;
// No direct using needed if global using for Enums is active in Domain and transitively available.
// However, explicit using is clearer if needed.
using FSH.WebApi.Domain.HR.Enums;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos;

public class InterviewDto
{
    public Guid Id { get; set; }

    public Guid ApplicantId { get; set; }
    public string? ApplicantName { get; set; } // To be populated (Applicant.FirstName + Applicant.LastName)

    public Guid? JobOpeningId { get; set; } // Via Applicant.JobOpeningId
    public string? JobOpeningTitle { get; set; } // Via Applicant.JobOpening.Title

    public Guid InterviewerId { get; set; }
    public string? InterviewerName { get; set; } // To be populated (Employee.FirstName + Employee.LastName)

    public DateTime ScheduledTime { get; set; }
    public InterviewType Type { get; set; }
    public string InterviewTypeDescription => Type.ToString();

    public string? Location { get; set; } // From Interview entity
    public string? Notes { get; set; } // From Interview entity (internal scheduling notes)
    public string? Feedback { get; set; } // From Interview entity
    public InterviewStatus Status { get; set; }
    public string StatusDescription => Status.ToString();

    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
