using Ardalis.Specification;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Domain.HR.Enums;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Specifications;

public class InterviewsByInterviewerAtTimeSpec : Specification<Interview>, ISingleResultSpecification
{
    /// <summary>
    /// Checks for interviews for a specific interviewer that overlap with the given time slot.
    /// Considers interviews that are not Cancelled.
    /// </summary>
    /// <param name="interviewerId">The ID of the interviewer.</param>
    /// <param name="proposedStartTime">The proposed start time of the new interview.</param>
    /// <param name="proposedEndTime">The proposed end time of the new interview.</param>
    /// <param name="interviewIdToExclude">Optional: An existing interview ID to exclude from the check (for rescheduling).</param>
    public InterviewsByInterviewerAtTimeSpec(Guid interviewerId, DateTime proposedStartTime, DateTime proposedEndTime, Guid? interviewIdToExclude = null)
    {
        Query
            .Where(i => i.InterviewerId == interviewerId)
            .Where(i => i.Status != InterviewStatus.CancelledByApplicant &&
                         i.Status != InterviewStatus.CancelledByInterviewer &&
                         i.Status != InterviewStatus.Completed) // Only check against active or scheduled interviews
            .Where(i => (i.ScheduledTime < proposedEndTime && proposedStartTime < i.ScheduledTime.AddHours(1))); // Assuming 1hr default duration for existing interviews for overlap check
                                                                                                                  // A more precise check would need existing interview's end time.
                                                                                                                  // For simplicity, using ScheduledTime < proposedEndTime and proposedStartTime < (ScheduledTime + durationOfExisting)

        if (interviewIdToExclude.HasValue)
        {
            Query.Where(i => i.Id != interviewIdToExclude.Value);
        }
    }
}
