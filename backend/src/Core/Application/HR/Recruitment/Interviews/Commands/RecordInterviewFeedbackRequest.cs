using FSH.WebApi.Domain.HR; // For InterviewStatus enum
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class RecordInterviewFeedbackRequest : IRequest<Guid> // Returns Interview.Id
{
    public Guid Id { get; set; } // Interview Id
    public string Feedback { get; set; } = string.Empty;
    public InterviewStatus NewStatus { get; set; } = InterviewStatus.Completed; // Default to Completed when feedback is given
    // Could also include a rating, e.g. public int? Rating { get; set; }
}
