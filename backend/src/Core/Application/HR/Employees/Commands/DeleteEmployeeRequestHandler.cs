using FSH.WebApi.Application.Common.Exceptions; // For NotFoundException
using FSH.WebApi.Application.Common.Persistence; // For IRepositoryWithEvents, IApplicationUnitOfWork
using FSH.WebApi.Domain.Common.Events; // For EntityDeletedEvent
using FSH.WebApi.Domain.HR; // For Employee entity
using MediatR; // For IRequestHandler
using Microsoft.Extensions.Localization; // For IStringLocalizer

namespace FSH.WebApi.Application.HR.Employees.Commands;

public class DeleteEmployeeRequestHandler : IRequestHandler<DeleteEmployeeRequest, Guid>
{
    private readonly IRepositoryWithEvents<Employee> _employeeRepository;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public DeleteEmployeeRequestHandler(IRepositoryWithEvents<Employee> employeeRepository, IApplicationUnitOfWork uow, IStringLocalizer<DeleteEmployeeRequestHandler> localizer)
    {
        _employeeRepository = employeeRepository;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(DeleteEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);

        _ = employee ?? throw new NotFoundException(_t["Employee {0} Not Found.", request.Id]);

        // TODO: Add pre-delete validation if required by business logic.
        // For example, check if the employee is a manager to other active employees.
        // If so, either prevent deletion, reassign direct reports, or make them managerless.
        // Example check (would require IReadRepository<Employee> and a spec):
        // var directReportsSpec = new EmployeesByManagerIdSpec(request.Id);
        // bool hasDirectReports = await _employeeRepository.AnyAsync(directReportsSpec, cancellationToken);
        // if (hasDirectReports)
        // {
        //     throw new ConflictException(_t["Employee {0} is a manager to other employees and cannot be deleted directly.", request.Id]);
        // }

        // If ISoftDelete is implemented on Employee, this would be a soft delete.
        // The current IRepositoryWithEvents<T>.DeleteAsync typically handles this by calling entity.Delete()
        // which would set DeletedOn if T : ISoftDelete.
        employee.AddDomainEvent(EntityDeletedEvent.WithEntity(employee));

        await _employeeRepository.DeleteAsync(employee, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return request.Id;
    }
}
