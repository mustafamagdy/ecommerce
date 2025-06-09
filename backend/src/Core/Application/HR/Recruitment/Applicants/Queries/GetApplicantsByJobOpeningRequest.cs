using FSH.WebApi.Application.HR.Recruitment.Applicants.Dtos; // For ApplicantDto
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Queries;

public class GetApplicantsByJobOpeningRequest : IRequest<List<ApplicantDto>>
{
    public Guid JobOpeningId { get; set; }

    public GetApplicantsByJobOpeningRequest(Guid jobOpeningId) => JobOpeningId = jobOpeningId;
}
