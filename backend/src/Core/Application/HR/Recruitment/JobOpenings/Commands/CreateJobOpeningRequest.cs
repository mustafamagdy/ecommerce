using FSH.WebApi.Domain.HR; // For JobOpeningStatus enum
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Commands;

public class CreateJobOpeningRequest : IRequest<Guid> // Returns JobOpening.Id
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public JobOpeningStatus Status { get; set; } = JobOpeningStatus.Open; // Default to Open
    public DateTime PostedDate { get; set; } = DateTime.UtcNow; // Default to now
    public DateTime? ClosingDate { get; set; }
}
