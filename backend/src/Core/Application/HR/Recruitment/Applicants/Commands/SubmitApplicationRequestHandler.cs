using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Common.FileStorage; // Added for IFileStorageService
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR; // For Applicant, JobOpening entities
using FSH.WebApi.Domain.HR.Enums; // For ApplicantStatus
using FSH.WebApi.Application.HR.Recruitment.Applicants.Specifications; // For ApplicantByEmailAndJobOpeningSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Recruitment.Applicants.Commands;

public class SubmitApplicationRequestHandler : IRequestHandler<SubmitApplicationRequest, Guid>
{
    private readonly IRepositoryWithEvents<Applicant> _applicantRepo;
    private readonly IReadRepository<JobOpening> _jobOpeningRepo;
    private readonly IFileStorageService _fileStorage; // Injected for resume upload
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public SubmitApplicationRequestHandler(
        IRepositoryWithEvents<Applicant> applicantRepo,
        IReadRepository<JobOpening> jobOpeningRepo,
        IFileStorageService fileStorage, // Added
        IApplicationUnitOfWork uow,
        IStringLocalizer<SubmitApplicationRequestHandler> localizer)
    {
        _applicantRepo = applicantRepo;
        _jobOpeningRepo = jobOpeningRepo;
        _fileStorage = fileStorage; // Added
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(SubmitApplicationRequest request, CancellationToken cancellationToken)
    {
        // Validate JobOpeningId and its status (must be Open)
        // Using a simple GetByIdAsync for now, a dedicated spec could be more robust
        var jobOpening = await _jobOpeningRepo.GetByIdAsync(request.JobOpeningId, cancellationToken);
        _ = jobOpening ?? throw new NotFoundException(_t["Job Opening with ID {0} not found.", request.JobOpeningId]);
        if (jobOpening.Status != JobOpeningStatus.Open)
        {
            throw new ConflictException(_t["This job opening is not currently open for applications. Its status is {0}.", jobOpening.Status]);
        }

        // Check for duplicate applications
        var duplicateSpec = new ApplicantByEmailAndJobOpeningSpec(request.Email, request.JobOpeningId);
        var existingApplication = await _applicantRepo.FirstOrDefaultAsync(duplicateSpec, cancellationToken);
        if (existingApplication is not null)
        {
            throw new ConflictException(_t["You have already applied for this job opening with this email address."]);
        }

        string resumePath = string.Empty;
        if (request.Resume is not null)
        {
            // Ensure FileUploadRequest is correctly populated (Name, Extension, Data) by the caller (e.g., controller)
            if (string.IsNullOrEmpty(request.Resume.Name) || string.IsNullOrEmpty(request.Resume.Extension) || request.Resume.Data == null)
            {
                // This basic validation should ideally be part of FluentValidation for the request DTO.
                throw new ValidationException(_t["Resume file information is incomplete. Name, Extension, and Data are required."]);
            }

            // TODO: Implement more robust error handling for file upload (e.g., catch specific exceptions from IFileStorageService).
            // TODO: Consider transactional consistency: What if Applicant record creation succeeds but file upload fails?
            //       (e.g., could use a two-phase commit, a saga pattern, or mark applicant as 'PendingResumeUpload' and have a cleanup job).
            // TODO: Implement retry logic for transient file storage errors if applicable for the chosen IFileStorageService.
            resumePath = await _fileStorage.UploadAsync<Applicant>(request.Resume, FileType.Document, cancellationToken);
            // Consider what happens if UploadAsync throws an exception. The current try-catch in middleware would handle it,
            // but specific handling might be needed here if, for example, a specific applicant status should be set.
        }

        var applicant = new Applicant
        {
            JobOpeningId = request.JobOpeningId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ResumePath = resumePath, // Path from file upload
            Notes = request.Notes,
            ApplicationDate = DateTime.UtcNow,
            Status = ApplicantStatus.Applied // Initial status
        };

        applicant.AddDomainEvent(EntityCreatedEvent.WithEntity(applicant));
        await _applicantRepo.AddAsync(applicant, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return applicant.Id;
    }
}
