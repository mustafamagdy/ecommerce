using FSH.WebApi.Domain.HR; // For JobOpeningStatus enum
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Commands;

public class UpdateJobOpeningStatusRequest : IRequest<Guid> // Returns JobOpening.Id
{
    public Guid Id { get; set; } // ID of the JobOpening to update
    public JobOpeningStatus Status { get; set; }
}
