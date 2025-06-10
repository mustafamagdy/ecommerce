using FSH.WebApi.Domain.Common.Contracts; // For DomainEvent

namespace FSH.WebApi.Domain.HR.Recruitment.Events;

public class ApplicantHiredEvent : DomainEvent
{
    public Guid ApplicantId { get; }
    public Guid JobOpeningId { get; }
    public DateTime HiredDate { get; }

    public ApplicantHiredEvent(Guid applicantId, Guid jobOpeningId, DateTime hiredDate)
    {
        ApplicantId = applicantId;
        JobOpeningId = jobOpeningId;
        HiredDate = hiredDate;
    }
}
