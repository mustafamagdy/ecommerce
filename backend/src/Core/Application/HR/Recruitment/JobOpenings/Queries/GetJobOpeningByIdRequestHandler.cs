using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For JobOpening entity
using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Dtos; // For JobOpeningDto
using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Specifications; // For JobOpeningByIdSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Queries;

public class GetJobOpeningByIdRequestHandler : IRequestHandler<GetJobOpeningByIdRequest, JobOpeningDto>
{
    private readonly IReadRepository<JobOpening> _repository;
    private readonly IStringLocalizer<GetJobOpeningByIdRequestHandler> _t;

    public GetJobOpeningByIdRequestHandler(IReadRepository<JobOpening> repository, IStringLocalizer<GetJobOpeningByIdRequestHandler> localizer)
    {
        _repository = repository;
        _t = localizer;
    }

    public async Task<JobOpeningDto> Handle(GetJobOpeningByIdRequest request, CancellationToken cancellationToken)
    {
        var spec = new JobOpeningByIdSpec(request.Id);
        var jobOpeningDto = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        _ = jobOpeningDto ?? throw new NotFoundException(_t["JobOpening with ID {0} Not Found.", request.Id]);

        return jobOpeningDto;
    }
}
