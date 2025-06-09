using FSH.WebApi.Domain.Common.Contracts; // For AuditableEntity

using FSH.WebApi.Domain.HR.Enums; // Added for ApplicantStatus

namespace FSH.WebApi.Domain.HR;

public class Applicant : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ResumePath { get; set; } // Path to the stored resume file

    public DateTime ApplicationDate { get; set; }
    public ApplicantStatus Status { get; set; } = ApplicantStatus.Applied; // Default status
    public string? Notes { get; set; } // General notes, can be used for status change reasons etc.

    public Guid JobOpeningId { get; set; }
    public virtual JobOpening? JobOpening { get; set; }

    public Applicant()
    {
        ApplicationDate = DateTime.UtcNow;
    }
}
