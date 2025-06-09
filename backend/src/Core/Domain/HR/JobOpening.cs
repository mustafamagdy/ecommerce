using FSH.WebApi.Domain.Common.Contracts; // For AuditableEntity
// No specific using needed if Department is in the same FSH.WebApi.Domain.HR namespace
using FSH.WebApi.Domain.HR.Enums; // Added for JobOpeningStatus

namespace FSH.WebApi.Domain.HR;

public class JobOpening : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }
    public virtual Department? Department { get; set; } // Assuming Department entity exists, adjust namespace if needed

    public JobOpeningStatus Status { get; set; } = JobOpeningStatus.Open;
    public DateTime PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; } // Nullable if it can be open-ended

    public JobOpening()
    {
        PostedDate = DateTime.UtcNow;
    }
}
