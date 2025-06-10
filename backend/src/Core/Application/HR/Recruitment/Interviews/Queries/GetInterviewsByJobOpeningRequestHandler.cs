using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Interview entity
using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos; // For InterviewDto
using FSH.WebApi.Application.HR.Recruitment.Interviews.Specifications; // For InterviewsByJobOpeningSpec
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Queries;

public class GetInterviewsByJobOpeningRequestHandler : IRequestHandler<GetInterviewsByJobOpeningRequest, List<InterviewDto>>
{
    private readonly IReadRepository<Interview> _repository;

    public GetInterviewsByJobOpeningRequestHandler(IReadRepository<Interview> repository)
    {
        _repository = repository;
    }

    public async Task<List<InterviewDto>> Handle(GetInterviewsByJobOpeningRequest request, CancellationToken cancellationToken)
    {
        var spec = new InterviewsByJobOpeningSpec(request.JobOpeningId);
        var interviews = await _repository.ListAsync(spec, cancellationToken);
        return interviews;
    }
}
