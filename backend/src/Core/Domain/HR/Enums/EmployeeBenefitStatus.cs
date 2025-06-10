namespace FSH.WebApi.Domain.HR.Enums;

public enum EmployeeBenefitStatus
{
    PendingEnrollment,  // Employee has elected but not yet effective or confirmed
    Active,             // Benefit is currently active for the employee
    Waived,             // Employee has explicitly declined this benefit
    Terminated,         // Benefit enrollment has ended
    PendingTermination  // Benefit is set to terminate on a future date
}
