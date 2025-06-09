// Using the domain enum, or could define a DTO-specific enum
using FSH.WebApi.Domain.HR; // For JobOpeningStatus

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Dtos;

public class JobOpeningDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; } // To be populated

    public JobOpeningStatus Status { get; set; }
    public string StatusDescription => Status.ToString(); // String representation of enum

    public DateTime PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
