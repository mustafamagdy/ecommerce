using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For Applicant, JobOpening entities
using FSH.WebApi.Application.HR.Recruitment.Applicants.Dtos; // For ApplicantDto

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Specifications;

public class ApplicantsByJobOpeningSpec : Specification<Applicant, ApplicantDto>
{
    public ApplicantsByJobOpeningSpec(Guid jobOpeningId)
    {
        Query
            .Where(a => a.JobOpeningId == jobOpeningId)
            .Include(a => a.JobOpening) // For JobOpeningTitle, though it's the same for all in this list
            .OrderByDescending(a => a.ApplicationDate);

        Query.Select(a => new ApplicantDto
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Email = a.Email,
            PhoneNumber = a.PhoneNumber,
            ResumePath = a.ResumePath,
            ApplicationDate = a.ApplicationDate,
            JobOpeningId = a.JobOpeningId,
            JobOpeningTitle = a.JobOpening != null ? a.JobOpening.Title : null,
            Status = a.Status,
            Notes = a.Notes,
            CreatedOn = a.CreatedOn,
            LastModifiedOn = a.LastModifiedOn
        });
    }
}
