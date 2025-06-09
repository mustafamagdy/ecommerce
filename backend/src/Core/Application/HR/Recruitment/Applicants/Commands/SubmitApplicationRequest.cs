using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Commands;

public class SubmitApplicationRequest : IRequest<Guid> // Returns Applicant.Id
{
    public Guid JobOpeningId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public string? Notes { get; set; } // Applicant's cover letter or general notes

    // Use FileUploadRequest as per FSH boilerplate for file uploads
    public FSH.WebApi.Application.Common.FileStorage.FileUploadRequest? Resume { get; set; }
}
