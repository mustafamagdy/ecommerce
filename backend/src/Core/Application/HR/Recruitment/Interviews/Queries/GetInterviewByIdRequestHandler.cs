using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Interview entity
using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos; // For InterviewDto
using FSH.WebApi.Application.HR.Recruitment.Interviews.Specifications; // For InterviewByIdSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Queries;

public class GetInterviewByIdRequestHandler : IRequestHandler<GetInterviewByIdRequest, InterviewDto>
{
    private readonly IReadRepository<Interview> _repository;
    private readonly IStringLocalizer<GetInterviewByIdRequestHandler> _t;

    public GetInterviewByIdRequestHandler(IReadRepository<Interview> repository, IStringLocalizer<GetInterviewByIdRequestHandler> localizer)
    {
        _repository = repository;
        _t = localizer;
    }

    public async Task<InterviewDto> Handle(GetInterviewByIdRequest request, CancellationToken cancellationToken)
    {
        var spec = new InterviewByIdSpec(request.Id);
        var interviewDto = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        _ = interviewDto ?? throw new NotFoundException(_t["Interview with ID {0} Not Found.", request.Id]);

        return interviewDto;
    }
}
