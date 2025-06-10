using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos; // For InterviewDto
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Queries;

public class GetInterviewsByInterviewerRequest : IRequest<List<InterviewDto>>
{
    public Guid InterviewerId { get; set; } // Employee Id

    public GetInterviewsByInterviewerRequest(Guid interviewerId) => InterviewerId = interviewerId;
}
