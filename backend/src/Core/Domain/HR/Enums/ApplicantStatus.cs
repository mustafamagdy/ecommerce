namespace FSH.WebApi.Domain.HR.Enums;

public enum ApplicantStatus
{
    Applied,
    Screening,
    Shortlisted,
    InterviewScheduled, // As per new requirement
    InterviewCompleted, // As per new requirement
    OfferExtended,
    Hired,
    Rejected,
    Withdrawn
}
