using FSH.WebApi.Application.Common.Exceptions; // For NotFoundException, ConflictException
using FSH.WebApi.Application.Common.Persistence; // For IRepositoryWithEvents, IApplicationUnitOfWork, IReadRepository
using FSH.WebApi.Domain.Common.Events; // For EntityDeletedEvent
using FSH.WebApi.Domain.HR; // For Employee entity
using FSH.WebApi.Application.HR.Employees.Specifications; // For EmployeesByManagerIdSpec
using MediatR; // For IRequestHandler
using Microsoft.Extensions.Localization; // For IStringLocalizer

namespace FSH.WebApi.Application.HR.Employees.Commands;

public class DeleteEmployeeRequestHandler : IRequestHandler<DeleteEmployeeRequest, Guid>
{
    // Using IRepository for _employeeRepository for Add/Update/Delete with events
    private readonly IRepositoryWithEvents<Employee> _employeeRepository;
    // Using IReadRepository for checks to avoid tracking if not needed for the check itself
    private readonly IReadRepository<Employee> _employeeReadRepository;
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public DeleteEmployeeRequestHandler(
        IRepositoryWithEvents<Employee> employeeRepository,
        IReadRepository<Employee> employeeReadRepository, // Injected IReadRepository
        IApplicationUnitOfWork uow,
        IStringLocalizer<DeleteEmployeeRequestHandler> localizer)
    {
        _employeeRepository = employeeRepository;
        _employeeReadRepository = employeeReadRepository; // Assigned
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(DeleteEmployeeRequest request, CancellationToken cancellationToken)
    {
        // Check if employee is a manager to others
        var directReportsSpec = new EmployeesByManagerIdSpec(request.Id);
        bool hasDirectReports = await _employeeReadRepository.AnyAsync(directReportsSpec, cancellationToken);
        if (hasDirectReports)
        {
            throw new ConflictException(_t["Employee {0} is a manager to other active employees and cannot be deleted directly. Please reassign their direct reports first.", request.Id]);
        }

        var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        _ = employee ?? throw new NotFoundException(_t["Employee {0} Not Found.", request.Id]);

        // If ISoftDelete is implemented on Employee, this would be a soft delete.
        // The current IRepositoryWithEvents<T>.DeleteAsync typically handles this by calling entity.Delete()
        // which would set DeletedOn if T : ISoftDelete.
        employee.AddDomainEvent(EntityDeletedEvent.WithEntity(employee));

        await _employeeRepository.DeleteAsync(employee, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return request.Id;
    }
}
