using Ardalis.Specification;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Dtos;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Specifications;

public class JobOpeningByIdSpec : Specification<JobOpening, JobOpeningDto>, ISingleResultSpecification
{
    public JobOpeningByIdSpec(Guid jobOpeningId)
    {
        Query
            .Where(jo => jo.Id == jobOpeningId)
            .Include(jo => jo.Department); // To get DepartmentName

        Query.Select(jo => new JobOpeningDto
        {
            Id = jo.Id,
            Title = jo.Title,
            Description = jo.Description,
            DepartmentId = jo.DepartmentId,
            DepartmentName = jo.Department != null ? jo.Department.Name : null,
            Status = jo.Status,
            PostedDate = jo.PostedDate,
            ClosingDate = jo.ClosingDate,
            CreatedOn = jo.CreatedOn,
            LastModifiedOn = jo.LastModifiedOn
        });
    }
}
