using FSH.WebApi.Domain.HR.Enums; // For InterviewStatus, ApplicantStatus enums
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Commands;

public class RecordInterviewFeedbackRequest : IRequest<Guid> // Returns Interview.Id
{
    public Guid Id { get; set; } // Interview Id
    public string Feedback { get; set; } = string.Empty;
    public InterviewStatus InterviewNewStatus { get; set; } = InterviewStatus.Completed; // Default to Completed
    public ApplicantStatus? NextRecommendedApplicantStep { get; set; } // Optional: e.g., Shortlisted, OfferExtended, Rejected
}
