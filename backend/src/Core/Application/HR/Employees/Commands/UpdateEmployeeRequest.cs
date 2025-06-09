using MediatR;

namespace FSH.WebApi.Application.HR.Employees.Commands;

public class UpdateEmployeeRequest : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; } // Nullable if we allow partial updates
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; } // Allow clearing the phone number
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfJoining { get; set; } // Usually not updated, but included for completeness
    public Guid? DepartmentId { get; set; }
    public Guid? JobTitleId { get; set; }
    public Guid? ManagerId { get; set; } // Allow changing manager
}
