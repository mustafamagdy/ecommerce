using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos; // For InterviewDto
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Queries;

public class GetInterviewByIdRequest : IRequest<InterviewDto>
{
    public Guid Id { get; set; }

    public GetInterviewByIdRequest(Guid id) => Id = id;
}
