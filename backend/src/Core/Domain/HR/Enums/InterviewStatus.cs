namespace FSH.WebApi.Domain.HR.Enums;

public enum InterviewStatus
{
    Scheduled,
    Completed,
    CancelledByInterviewer, // Updated from Cancelled
    CancelledByApplicant,   // New
    Rescheduled
    // NoShow was removed based on new list, can be added if needed
}
