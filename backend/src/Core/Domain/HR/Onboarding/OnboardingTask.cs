using FSH.WebApi.Domain.Common.Contracts;
using FSH.WebApi.Domain.HR.Enums; // For OnboardingTaskStatus

namespace FSH.WebApi.Domain.HR.Onboarding;

public class OnboardingTask : AuditableEntity
{
    public Guid OnboardingProcessId { get; set; }
    public virtual OnboardingProcess? OnboardingProcess { get; set; }

    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AssignedTo { get; set; } // Could be a role name, user ID, or specific instructions
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public OnboardingTaskStatus Status { get; set; } = OnboardingTaskStatus.ToDo;

    public OnboardingTask(Guid onboardingProcessId, string taskName)
    {
        OnboardingProcessId = onboardingProcessId;
        TaskName = taskName;
    }

    // Parameterless constructor for EF Core
    private OnboardingTask() { }
}
