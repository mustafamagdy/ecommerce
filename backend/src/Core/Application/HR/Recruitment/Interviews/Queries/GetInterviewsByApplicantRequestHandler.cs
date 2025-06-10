using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Interview entity
using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos; // For InterviewDto
using FSH.WebApi.Application.HR.Recruitment.Interviews.Specifications; // For InterviewsByApplicantSpec
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Queries;

public class GetInterviewsByApplicantRequestHandler : IRequestHandler<GetInterviewsByApplicantRequest, List<InterviewDto>>
{
    private readonly IReadRepository<Interview> _repository;

    public GetInterviewsByApplicantRequestHandler(IReadRepository<Interview> repository)
    {
        _repository = repository;
    }

    public async Task<List<InterviewDto>> Handle(GetInterviewsByApplicantRequest request, CancellationToken cancellationToken)
    {
        var spec = new InterviewsByApplicantSpec(request.ApplicantId);
        var interviews = await _repository.ListAsync(spec, cancellationToken);
        return interviews;
    }
}
