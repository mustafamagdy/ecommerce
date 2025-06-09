using FSH.WebApi.Domain.Common.Contracts; // For AuditableEntity

using FSH.WebApi.Domain.HR.Enums; // Added for InterviewType and InterviewStatus

namespace FSH.WebApi.Domain.HR;

public class Interview : AuditableEntity
{
    public Guid ApplicantId { get; set; }
    public virtual Applicant? Applicant { get; set; }

    public Guid InterviewerId { get; set; } // FK to Employee entity
    public virtual Employee? Interviewer { get; set; } // Navigation to Employee

    public DateTime ScheduledTime { get; set; }
    public InterviewType Type { get; set; }
    public string? Location { get; set; } // Can be a meeting link or physical address
    public string? Notes { get; set; } // General notes, e.g. meeting link, location, or prep material
    public string? Feedback { get; set; }
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;
}
