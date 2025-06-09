// Using domain enums, or could define DTO-specific enums
using FSH.WebApi.Domain.HR; // For InterviewType, InterviewStatus

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos;

public class InterviewDto
{
    public Guid Id { get; set; }

    public Guid ApplicantId { get; set; }
    public string? ApplicantName { get; set; } // To be populated

    public Guid InterviewerId { get; set; }
    public string? InterviewerName { get; set; } // To be populated (Employee's name)

    public DateTime ScheduledTime { get; set; }
    public InterviewType Type { get; set; }
    public string? Feedback { get; set; }
    public InterviewStatus Status { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
