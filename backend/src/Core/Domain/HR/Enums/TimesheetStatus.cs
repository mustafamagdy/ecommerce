namespace FSH.WebApi.Domain.HR.Enums;

public enum TimesheetStatus
{
    Open,       // Timesheet is currently being filled, not yet submitted
    Submitted,  // Submitted by employee for approval
    Approved,   // Approved by manager
    Rejected,   // Rejected by manager (employee may need to resubmit)
    Processed   // Processed for payroll
}
