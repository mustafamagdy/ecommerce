using Ardalis.Specification;
using FSH.WebApi.Domain.HR; // For Applicant entity

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Specifications;

public class ApplicantByEmailAndJobOpeningSpec : Specification<Applicant>, ISingleResultSpecification
{
    public ApplicantByEmailAndJobOpeningSpec(string email, Guid jobOpeningId)
    {
        Query
            .Where(a => a.Email.ToLower() == email.ToLower() && a.JobOpeningId == jobOpeningId);
    }
}
