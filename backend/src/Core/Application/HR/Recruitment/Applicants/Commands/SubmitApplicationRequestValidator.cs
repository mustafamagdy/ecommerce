using FSH.WebApi.Application.Common.Validation;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For JobOpening
using FSH.WebApi.Domain.HR.Enums; // For JobOpeningStatus
using FluentValidation;
using Microsoft.Extensions.Localization;
using FSH.WebApi.Application.Common.FileStorage; // For FileUploadRequest

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Commands;

// Spec to get JobOpening for validation (could be in JobOpenings/Specifications if more broadly used)
public class JobOpeningForValidationSpec : Specification<JobOpening>, ISingleResultSpecification
{
    public JobOpeningForValidationSpec(Guid id) => Query.Where(jo => jo.Id == id);
}

public class SubmitApplicationRequestValidator : CustomValidator<SubmitApplicationRequest>
{
    public SubmitApplicationRequestValidator(
        IReadRepository<JobOpening> jobOpeningRepository,
        IStringLocalizer<SubmitApplicationRequestValidator> T)
    {
        RuleFor(p => p.FirstName)
            .NotEmpty()
            .MaximumLength(75);

        RuleFor(p => p.LastName)
            .NotEmpty()
            .MaximumLength(75);

        RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress().WithMessage(T["Invalid email format."])
            .MaximumLength(256);

        RuleFor(p => p.PhoneNumber)
            .MaximumLength(20);

        RuleFor(p => p.JobOpeningId)
            .NotEmpty()
            .MustAsync(async (id, ct) =>
            {
                var jobOpening = await jobOpeningRepository.FirstOrDefaultAsync(new JobOpeningForValidationSpec(id), ct);
                return jobOpening is not null && jobOpening.Status == JobOpeningStatus.Open;
            })
            .WithMessage((req, id) => T["Job Opening with ID {0} not found or is not open for applications.", id]);

        RuleFor(p => p.Notes)
            .MaximumLength(2000); // Example max length for notes/cover letter

        // Validation for FileUploadRequest (Resume)
        // This is basic. More specific validation (file types, size limits)
        // would depend on how FileUploadRequest is structured and business rules.
        // TODO: File type and size validation should ideally be configurable (e.g., from application settings) rather than hardcoded.
        When(p => p.Resume != null, () =>
        {
            RuleFor(p => p.Resume!.Name)
                .NotEmpty().WithMessage(T["Resume file name cannot be empty."])
                .MaximumLength(256);
            RuleFor(p => p.Resume!.Extension)
                .NotEmpty().WithMessage(T["Resume file extension cannot be empty."])
                .Must(ext => ext == ".pdf" || ext == ".doc" || ext == ".docx" || ext == ".txt") // Example allowed extensions
                .WithMessage(T["Invalid resume file type. Allowed types: .pdf, .doc, .docx, .txt."]);
            RuleFor(p => p.Resume!.Data)
                .NotEmpty().WithMessage(T["Resume file content cannot be empty."]);
            // Example size validation (e.g., max 5MB)
            // RuleFor(p => p.Resume!.Data.Length).LessThanOrEqualTo(5 * 1024 * 1024)
            // .WithMessage(T["Resume file size cannot exceed 5MB."]);
        });
    }
}
