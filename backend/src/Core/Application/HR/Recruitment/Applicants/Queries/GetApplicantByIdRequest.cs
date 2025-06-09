using FSH.WebApi.Application.HR.Recruitment.Applicants.Dtos; // For ApplicantDto
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Queries;

public class GetApplicantByIdRequest : IRequest<ApplicantDto>
{
    public Guid Id { get; set; }

    public GetApplicantByIdRequest(Guid id) => Id = id;
}
