using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Applicant entity
using FSH.WebApi.Application.HR.Recruitment.Applicants.Dtos; // For ApplicantDto
using FSH.WebApi.Application.HR.Recruitment.Applicants.Specifications; // For ApplicantsByJobOpeningSpec
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Queries;

public class GetApplicantsByJobOpeningRequestHandler : IRequestHandler<GetApplicantsByJobOpeningRequest, List<ApplicantDto>>
{
    private readonly IReadRepository<Applicant> _repository;

    public GetApplicantsByJobOpeningRequestHandler(IReadRepository<Applicant> repository)
    {
        _repository = repository;
    }

    public async Task<List<ApplicantDto>> Handle(GetApplicantsByJobOpeningRequest request, CancellationToken cancellationToken)
    {
        var spec = new ApplicantsByJobOpeningSpec(request.JobOpeningId);
        var applicants = await _repository.ListAsync(spec, cancellationToken);
        return applicants;
    }
}
