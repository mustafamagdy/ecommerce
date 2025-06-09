using Ardalis.Specification;
using FSH.WebApi.Application.Common.Specification; // For EntitiesByPaginationFilterSpec
using FSH.WebApi.Domain.HR; // For JobOpening entity
using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Dtos; // For JobOpeningDto
using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Queries; // For SearchJobOpeningsRequest

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Specifications;

public class JobOpeningsBySearchRequestSpec : EntitiesByPaginationFilterSpec<JobOpening, JobOpeningDto>
{
    public JobOpeningsBySearchRequestSpec(SearchJobOpeningsRequest request)
        : base(request) // Handles pagination
    {
        Query
            .Include(jo => jo.Department); // For DepartmentName

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            Query.Search(jo => jo.Title, "%" + request.Keyword + "%")
                 .Search(jo => jo.Description, "%" + request.Keyword + "%");
        }

        if (request.DepartmentId.HasValue)
        {
            Query.Where(jo => jo.DepartmentId == request.DepartmentId.Value);
        }

        if (request.Status.HasValue)
        {
            Query.Where(jo => jo.Status == request.Status.Value);
        }

        // Default order if not specified in request's OrderBy
        if (string.IsNullOrEmpty(request.OrderBy))
        {
            Query.OrderByDescending(jo => jo.PostedDate);
        }

        // Projection to JobOpeningDto
        Query.Select(jo => new JobOpeningDto
        {
            Id = jo.Id,
            Title = jo.Title,
            Description = jo.Description,
            DepartmentId = jo.DepartmentId,
            DepartmentName = jo.Department != null ? jo.Department.Name : null,
            Status = jo.Status, // StatusDescription is a calculated property in JobOpeningDto
            PostedDate = jo.PostedDate,
            ClosingDate = jo.ClosingDate,
            CreatedOn = jo.CreatedOn,
            LastModifiedOn = jo.LastModifiedOn
        });
    }
}
