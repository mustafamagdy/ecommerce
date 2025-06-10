using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Interview entity
using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos; // For InterviewDto
using FSH.WebApi.Application.HR.Recruitment.Interviews.Specifications; // For InterviewsByInterviewerSpec
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Queries;

public class GetInterviewsByInterviewerRequestHandler : IRequestHandler<GetInterviewsByInterviewerRequest, List<InterviewDto>>
{
    private readonly IReadRepository<Interview> _repository;

    public GetInterviewsByInterviewerRequestHandler(IReadRepository<Interview> repository)
    {
        _repository = repository;
    }

    public async Task<List<InterviewDto>> Handle(GetInterviewsByInterviewerRequest request, CancellationToken cancellationToken)
    {
        var spec = new InterviewsByInterviewerSpec(request.InterviewerId);
        var interviews = await _repository.ListAsync(spec, cancellationToken);
        return interviews;
    }
}
