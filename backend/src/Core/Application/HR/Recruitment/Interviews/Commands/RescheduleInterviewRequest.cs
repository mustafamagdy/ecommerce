using FSH.WebApi.Domain.HR.Enums; // For InterviewType (if needed here, likely not)
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class RescheduleInterviewRequest : IRequest<Guid> // Returns Interview.Id
{
    public Guid InterviewId { get; set; }
    public DateTime NewScheduledTime { get; set; }
    public string? NewLocation { get; set; }
    public Guid? NewInterviewerId { get; set; } // Optional: if interviewer changes
    public string? RescheduleReason { get; set; } // Could be mandatory or optional based on policy
    public InterviewType? NewInterviewType { get; set; } // Optional: if type also changes
}
