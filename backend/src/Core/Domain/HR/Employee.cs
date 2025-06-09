namespace FSH.WebApi.Domain.HR;

public class Employee : AuditableEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfJoining { get; set; }

    public Guid DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    public Guid JobTitleId { get; set; }
    public virtual JobTitle? JobTitle { get; set; }

    public Guid? ManagerId { get; set; }
    public virtual Employee? Manager { get; set; }
}
