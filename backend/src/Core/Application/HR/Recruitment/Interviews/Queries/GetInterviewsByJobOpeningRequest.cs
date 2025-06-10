using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos; // For InterviewDto
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Queries;

public class GetInterviewsByJobOpeningRequest : IRequest<List<InterviewDto>>
{
    public Guid JobOpeningId { get; set; }

    public GetInterviewsByJobOpeningRequest(Guid jobOpeningId) => JobOpeningId = jobOpeningId;
}
