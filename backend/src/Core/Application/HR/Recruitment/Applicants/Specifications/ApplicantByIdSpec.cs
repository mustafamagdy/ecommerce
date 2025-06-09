using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For Applicant, JobOpening entities
using FSH.WebApi.Application.HR.Recruitment.Applicants.Dtos; // For ApplicantDto
// Assuming ApplicantStatus enum is globally available via Domain.GlobalUsings or direct using
// using FSH.WebApi.Domain.HR.Enums;

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Specifications;

public class ApplicantByIdSpec : Specification<Applicant, ApplicantDto>, ISingleResultSpecification
{
    public ApplicantByIdSpec(Guid applicantId)
    {
        Query
            .Where(a => a.Id == applicantId)
            .Include(a => a.JobOpening); // To get JobOpening.Title

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
            Status = a.Status, // StatusDescription is handled by DTO's calculated property
            Notes = a.Notes,
            CreatedOn = a.CreatedOn,
            LastModifiedOn = a.LastModifiedOn
        });
    }
}
