using FSH.WebApi.Application.Common.Models; // For PaginationFilter, PaginationResponse
using MediatR;

namespace FSH.WebApi.Application.HR.Employees.Queries;

public class SearchEmployeesRequest : PaginationFilter, IRequest<PaginationResponse<EmployeeDto>>
{
    // Specific search parameters for Employees
    public string? Name { get; set; } // Search by FirstName or LastName
    public Guid? DepartmentId { get; set; }
    public Guid? JobTitleId { get; set; }
    public Guid? ManagerId { get; set; }
}
