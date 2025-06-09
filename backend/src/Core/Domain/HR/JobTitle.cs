namespace FSH.WebApi.Domain.HR;

public class JobTitle : AuditableEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
