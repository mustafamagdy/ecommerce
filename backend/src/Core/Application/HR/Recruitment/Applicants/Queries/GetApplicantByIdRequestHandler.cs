using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Applicant entity
using FSH.WebApi.Application.HR.Recruitment.Applicants.Dtos; // For ApplicantDto
using FSH.WebApi.Application.HR.Recruitment.Applicants.Specifications; // For ApplicantByIdSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Queries;

public class GetApplicantByIdRequestHandler : IRequestHandler<GetApplicantByIdRequest, ApplicantDto>
{
    private readonly IReadRepository<Applicant> _repository;
    private readonly IStringLocalizer<GetApplicantByIdRequestHandler> _t;

    public GetApplicantByIdRequestHandler(IReadRepository<Applicant> repository, IStringLocalizer<GetApplicantByIdRequestHandler> localizer)
    {
        _repository = repository;
        _t = localizer;
    }

    public async Task<ApplicantDto> Handle(GetApplicantByIdRequest request, CancellationToken cancellationToken)
    {
        var spec = new ApplicantByIdSpec(request.Id);
        var applicantDto = await _repository.FirstOrDefaultAsync(spec, cancellationToken);

        _ = applicantDto ?? throw new NotFoundException(_t["Applicant with ID {0} Not Found.", request.Id]);

        return applicantDto;
    }
}
