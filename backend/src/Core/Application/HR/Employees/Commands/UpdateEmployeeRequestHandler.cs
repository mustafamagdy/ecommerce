using FSH.WebApi.Application.Common.Exceptions; // For NotFoundException, ValidationException
using FSH.WebApi.Application.Common.Persistence; // For IRepositoryWithEvents, IApplicationUnitOfWork
using FSH.WebApi.Domain.HR; // For Employee entity
using MediatR; // For IRequestHandler
using Microsoft.Extensions.Localization; // For IStringLocalizer
using FluentValidation.Results; // For ValidationFailure
using System.Collections.Generic; // For List

namespace FSH.WebApi.Application.HR.Employees.Commands;

public class UpdateEmployeeRequestHandler : IRequestHandler<UpdateEmployeeRequest, Guid>
{
    private readonly IRepositoryWithEvents<Employee> _employeeRepository;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public UpdateEmployeeRequestHandler(IRepositoryWithEvents<Employee> employeeRepository, IApplicationUnitOfWork uow, IStringLocalizer<UpdateEmployeeRequestHandler> localizer)
    {
        _employeeRepository = employeeRepository;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);

        _ = employee ?? throw new NotFoundException(_t["Employee {0} Not Found.", request.Id]);

        // DateOfJoining validation against DateOfBirth (if DateOfJoining is being updated)
        if (request.DateOfJoining.HasValue)
        {
            // Use employee's actual DateOfBirth, or if request.DateOfBirth is also being updated, use that one.
            DateTime dobForValidation = request.DateOfBirth.HasValue ? request.DateOfBirth.Value : employee.DateOfBirth;
            if (request.DateOfJoining.Value < dobForValidation.AddYears(16))
            {
                var failures = new List<ValidationFailure>
                {
                    new ValidationFailure(nameof(request.DateOfJoining), _t["Date of joining must be at least 16 years after Date of Birth."])
                };
                throw new ValidationException(failures);
            }
        }

        // Update employee properties. Consider moving this logic to an Update method in the Employee entity later.
        // If a field is present in the request, update it. Otherwise, keep the existing value.
        // For nullable fields like PhoneNumber and ManagerId, if the request provides null, it means to set the field to null.
        if (request.FirstName is not null) employee.FirstName = request.FirstName;
        if (request.LastName is not null) employee.LastName = request.LastName;
        if (request.Email is not null) employee.Email = request.Email;
        if (request.PhoneNumber is not null || (request.PhoneNumber == null && employee.PhoneNumber != null)) // Explicitly setting to null or updating
        {
            employee.PhoneNumber = request.PhoneNumber;
        }
        if (request.DateOfBirth.HasValue) employee.DateOfBirth = request.DateOfBirth.Value; // DOB updated first if present
        if (request.DateOfJoining.HasValue) employee.DateOfJoining = request.DateOfJoining.Value;
        if (request.DepartmentId.HasValue) employee.DepartmentId = request.DepartmentId.Value;
        if (request.JobTitleId.HasValue) employee.JobTitleId = request.JobTitleId.Value;
        if (request.ManagerId.HasValue || (request.ManagerId == null && employee.ManagerId != null)) // Explicitly setting to null or updating
        {
            employee.ManagerId = request.ManagerId;
        }

        // The IRepositoryWithEvents should automatically handle EntityUpdatedEvent
        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return request.Id;
    }
}
