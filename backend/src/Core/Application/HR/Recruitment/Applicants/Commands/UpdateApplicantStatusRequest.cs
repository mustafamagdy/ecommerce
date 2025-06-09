using FSH.WebApi.Domain.HR; // For ApplicantStatus enum
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Commands;

public class UpdateApplicantStatusRequest : IRequest<Guid> // Returns Applicant.Id
{
    public Guid Id { get; set; } // Applicant Id
    public ApplicantStatus Status { get; set; }
    public string? Notes { get; set; } // Optional notes for status change
}
