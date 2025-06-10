using Ardalis.Specification;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Specifications;

public class InterviewsByInterviewerSpec : Specification<Interview, InterviewDto>
{
    public InterviewsByInterviewerSpec(Guid interviewerId)
    {
        Query
            .Where(i => i.InterviewerId == interviewerId)
            .Include(i => i.Applicant)
                .ThenInclude(a => a!.JobOpening)
            .Include(i => i.Interviewer) // Though InterviewerId is filtered, including Interviewer might be useful if InterviewerName needs to be consistent across list
            .OrderByDescending(i => i.ScheduledTime);

        Query.Select(i => new InterviewDto
        {
            Id = i.Id,
            ApplicantId = i.ApplicantId,
            ApplicantName = i.Applicant != null ? $"{i.Applicant.FirstName} {i.Applicant.LastName}" : null,
            JobOpeningId = i.Applicant != null ? i.Applicant.JobOpeningId : (Guid?)null,
            JobOpeningTitle = i.Applicant != null && i.Applicant.JobOpening != null ? i.Applicant.JobOpening.Title : null,
            InterviewerId = i.InterviewerId,
            InterviewerName = i.Interviewer != null ? $"{i.Interviewer.FirstName} {i.Interviewer.LastName}" : null,
            ScheduledTime = i.ScheduledTime,
            Type = i.Type,
            Location = i.Location,
            Notes = i.Notes,
            Feedback = i.Feedback,
            Status = i.Status,
            CreatedOn = i.CreatedOn,
            LastModifiedOn = i.LastModifiedOn
        });
    }
}
