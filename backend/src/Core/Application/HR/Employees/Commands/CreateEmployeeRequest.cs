using MediatR; // Required for IRequest

namespace FSH.WebApi.Application.HR.Employees.Commands;

public class CreateEmployeeRequest : IRequest<Guid> // Assuming it returns the Id of the created employee
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfJoining { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid JobTitleId { get; set; }
    public Guid? ManagerId { get; set; }
}
