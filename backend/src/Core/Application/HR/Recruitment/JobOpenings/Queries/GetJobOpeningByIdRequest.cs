using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Queries;

// Assuming JobOpeningDto is in FSH.WebApi.Application.HR.Recruitment.JobOpenings.Dtos
using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Dtos;

public class GetJobOpeningByIdRequest : IRequest<JobOpeningDto>
{
    public Guid Id { get; set; }

    public GetJobOpeningByIdRequest(Guid id) => Id = id;
}
