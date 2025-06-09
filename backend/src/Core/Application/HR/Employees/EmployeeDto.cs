namespace FSH.WebApi.Application.HR.Employees;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfJoining { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; } // From related Department entity
    public Guid JobTitleId { get; set; }
    public string? JobTitleName { get; set; } // From related JobTitle entity
    public Guid? ManagerId { get; set; }
    public string? ManagerFirstName { get; set; } // From related Employee entity (Manager)
    public string? ManagerLastName { get; set; } // From related Employee entity (Manager)
}
