using Ardalis.Specification;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Specifications;

public class InterviewsByApplicantSpec : Specification<Interview, InterviewDto>
{
    public InterviewsByApplicantSpec(Guid applicantId)
    {
        Query
            .Where(i => i.ApplicantId == applicantId)
            .Include(i => i.Applicant)
                .ThenInclude(a => a!.JobOpening)
            .Include(i => i.Interviewer)
            .OrderByDescending(i => i.ScheduledTime); // Order by most recent first

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
