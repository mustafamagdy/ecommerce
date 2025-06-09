using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // Employee, Department, JobTitle

namespace FSH.WebApi.Application.HR.Employees.Queries;

public class EmployeeByIdSpec : Specification<Employee, EmployeeDto>, ISingleResultSpecification
{
    public EmployeeByIdSpec(Guid employeeId)
    {
        Query
            .Where(e => e.Id == employeeId)
            .Include(e => e.Department) // Ensure Department is included for DepartmentName
            .Include(e => e.JobTitle)   // Ensure JobTitle is included for JobTitleName
            .Include(e => e.Manager);   // Ensure Manager is included for Manager's name

        // The Ardalis.Specification library automatically maps to the DTO
        // if the property names match or if a Select clause is used for custom mapping.
        // For simple cases like navigation property names, it might work directly.
        // For more complex mappings (like Manager.FirstName + Manager.LastName), a Select clause is better.

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
            // Note: CreatedBy, CreatedOn etc. are not in EmployeeDto as per previous definition.
            // If they were, they would be mapped from AuditableEntity properties.
        });
    }
}
