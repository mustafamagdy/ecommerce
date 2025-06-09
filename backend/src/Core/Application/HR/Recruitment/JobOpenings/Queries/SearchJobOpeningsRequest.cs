using FSH.WebApi.Application.Common.Models; // For PaginationFilter, PaginationResponse
using FSH.WebApi.Domain.HR.Enums; // For JobOpeningStatus
using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Dtos; // For JobOpeningDto
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Queries;

public class SearchJobOpeningsRequest : PaginationFilter, IRequest<PaginationResponse<JobOpeningDto>>
{
    public string? Keyword { get; set; }
    public Guid? DepartmentId { get; set; }
    public JobOpeningStatus? Status { get; set; }
}
