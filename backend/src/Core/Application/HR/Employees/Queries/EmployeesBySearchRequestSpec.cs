using Ardalis.Specification;
using FSH.WebApi.Application.Common.Specification; // For EntitiesByPaginationFilterSpec
using FSH.WebApi.Domain.HR; // Employee, Department, JobTitle
using FSH.WebApi.Application.Common.Models; // For PaginationFilter which SearchEmployeesRequest inherits

namespace FSH.WebApi.Application.HR.Employees.Queries;

public class EmployeesBySearchRequestSpec : EntitiesByPaginationFilterSpec<Employee, EmployeeDto>
{
    public EmployeesBySearchRequestSpec(SearchEmployeesRequest request)
        : base(request) // This handles pagination and ordering via PaginationFilter properties
    {
        Query
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.Manager);

        if (request.DepartmentId.HasValue)
        {
            Query.Where(e => e.DepartmentId == request.DepartmentId.Value);
        }

        if (request.JobTitleId.HasValue)
        {
            Query.Where(e => e.JobTitleId == request.JobTitleId.Value);
        }

        if (request.ManagerId.HasValue)
        {
            Query.Where(e => e.ManagerId == request.ManagerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) // 'Name' is the keyword from SearchEmployeesRequest
        {
            // Using ToLower() for case-insensitive search, assuming database collation might also handle this.
            // For more robust search, consider full-text search capabilities of the database.
            Query.Search(e => e.FirstName, "%" + request.Name.ToLower() + "%")
                 .Search(e => e.LastName, "%" + request.Name.ToLower() + "%")
                 .Search(e => e.Email, "%" + request.Name.ToLower() + "%"); // Added email to search
        }

        // Projection to EmployeeDto
        Query.Select(e => new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            PhoneNumber = e.PhoneNumber,
            DateOfBirth = e.DateOfBirth,
            DateOfJoining = e.DateOfJoining,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department != null ? e.Department.Name : null,
            JobTitleId = e.JobTitleId,
            JobTitleName = e.JobTitle != null ? e.JobTitle.Title : null,
            ManagerId = e.ManagerId,
            ManagerFirstName = e.Manager != null ? e.Manager.FirstName : null,
            ManagerLastName = e.Manager != null ? e.Manager.LastName : null
        });
    }
}
