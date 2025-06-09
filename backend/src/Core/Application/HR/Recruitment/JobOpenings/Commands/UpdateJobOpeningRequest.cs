using FSH.WebApi.Domain.HR; // For JobOpeningStatus enum
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Commands;

public class UpdateJobOpeningRequest : IRequest<Guid> // Returns JobOpening.Id
{
    public Guid Id { get; set; } // ID of the JobOpening to update

    public string? Title { get; set; }
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
    public JobOpeningStatus? Status { get; set; }
    public DateTime? PostedDate { get; set; } // Usually not updated, but can be
    public DateTime? ClosingDate { get; set; } // Use null to remove closing date
}
