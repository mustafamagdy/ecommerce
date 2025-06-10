using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class CancelInterviewRequest : IRequest<Guid> // Returns Interview.Id
{
    public Guid InterviewId { get; set; }
    public string? CancellationReason { get; set; }
    // Optional: public bool CancelledByApplicant { get; set; } // To distinguish who cancelled
}
