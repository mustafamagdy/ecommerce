using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For Interview, Applicant, Employee, JobOpening entities
using FSH.WebApi.Application.HR.Recruitment.Interviews.Dtos; // For InterviewDto
// Assuming Enums are globally available or via FSH.WebApi.Domain.HR.Enums;

namespace FSH.WebApi.Application.HR.Recruitment.Interviews.Specifications;

public class InterviewByIdSpec : Specification<Interview, InterviewDto>, ISingleResultSpecification
{
    public InterviewByIdSpec(Guid interviewId)
    {
        Query
            .Where(i => i.Id == interviewId)
            .Include(i => i.Applicant)
                .ThenInclude(a => a!.JobOpening) // Include JobOpening through Applicant
            .Include(i => i.Interviewer);       // Employee who is the interviewer

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
            Type = i.Type, // TypeDescription is on DTO
            Location = i.Location,
            Notes = i.Notes,
            Feedback = i.Feedback,
            Status = i.Status, // StatusDescription is on DTO
            CreatedOn = i.CreatedOn,
            LastModifiedOn = i.LastModifiedOn
        });
    }
}
