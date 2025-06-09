namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Dtos;

public class ApplicantDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}"; // Calculated property
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ResumePath { get; set; } // Could be a URL if served, or just path reference

    public DateTime ApplicationDate { get; set; }

    public Guid JobOpeningId { get; set; }
    public string? JobOpeningTitle { get; set; } // To be populated

    public FSH.WebApi.Domain.HR.Enums.ApplicantStatus Status { get; set; } // Use the enum from Domain
    public string StatusDescription => Status.ToString(); // String representation
    public string? Notes { get; set; } // From Applicant entity

    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
