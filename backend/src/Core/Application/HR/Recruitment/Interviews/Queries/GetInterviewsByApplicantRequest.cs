using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos; // For InterviewDto
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Queries;

public class GetInterviewsByApplicantRequest : IRequest<List<InterviewDto>>
{
    public Guid ApplicantId { get; set; }

    public GetInterviewsByApplicantRequest(Guid applicantId) => ApplicantId = applicantId;
}
