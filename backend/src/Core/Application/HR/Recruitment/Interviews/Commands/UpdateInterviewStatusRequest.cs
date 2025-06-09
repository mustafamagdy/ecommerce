using FSH.WebApi.Domain.HR; // For InterviewStatus enum
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class UpdateInterviewStatusRequest : IRequest<Guid> // Returns Interview.Id
{
    public Guid Id { get; set; } // Interview Id
    public InterviewStatus Status { get; set; }
    public string? Notes { get; set; } // Optional notes for this status change
}
