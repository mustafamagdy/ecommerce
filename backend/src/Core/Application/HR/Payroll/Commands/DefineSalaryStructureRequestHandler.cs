using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR;
using MediatR;
using Microsoft.Extensions.Localization;

using FSH.WebApi.Application.HR.Payroll.Specifications; // Added for SalaryStructureByEmployeeIdSpec

namespace FSH.WebApi.Application.HR.Payroll;

public class DefineSalaryStructureRequestHandler : IRequestHandler<DefineSalaryStructureRequest, Guid>
{
    private readonly IRepositoryWithEvents<SalaryStructure> _salaryStructureRepo;
    private readonly IReadRepository<Employee> _employeeRepo; // To validate EmployeeId
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public DefineSalaryStructureRequestHandler(
        IRepositoryWithEvents<SalaryStructure> salaryStructureRepo,
        IReadRepository<Employee> employeeRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<DefineSalaryStructureRequestHandler> localizer)
    {
        _salaryStructureRepo = salaryStructureRepo;
        _employeeRepo = employeeRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(DefineSalaryStructureRequest request, CancellationToken cancellationToken)
    {
        // Validate EmployeeId
        var employee = await _employeeRepo.GetByIdAsync(request.EmployeeId, cancellationToken);
        _ = employee ?? throw new NotFoundException(_t["Employee with ID {0} not found.", request.EmployeeId]);

        var spec = new SalaryStructureByEmployeeIdSpec(request.EmployeeId);
        var salaryStructure = await _salaryStructureRepo.FirstOrDefaultAsync(spec, cancellationToken);

        bool isUpdate = salaryStructure is not null;

        if (salaryStructure is null)
        {
            salaryStructure = new SalaryStructure { EmployeeId = request.EmployeeId };
        }

        salaryStructure.BasicSalary = request.BasicSalary;

        // Map DTOs to Domain Objects for Earnings
        salaryStructure.Earnings.Clear();
        foreach (var earningDto in request.Earnings)
        {
            salaryStructure.Earnings.Add(new SalaryComponent(earningDto.Name, earningDto.Amount, earningDto.IsPercentage));
        }

        // Map DTOs to Domain Objects for Deductions
        salaryStructure.Deductions.Clear();
        foreach (var deductionDto in request.Deductions)
        {
            salaryStructure.Deductions.Add(new SalaryComponent(deductionDto.Name, deductionDto.Amount, deductionDto.IsPercentage));
        }

        if (isUpdate)
        {
            salaryStructure.AddDomainEvent(EntityUpdatedEvent.WithEntity(salaryStructure));
            await _salaryStructureRepo.UpdateAsync(salaryStructure, cancellationToken);
        }
        else
        {
            salaryStructure.AddDomainEvent(EntityCreatedEvent.WithEntity(salaryStructure));
            await _salaryStructureRepo.AddAsync(salaryStructure, cancellationToken);
        }

        await _uow.CommitAsync(cancellationToken);

        return salaryStructure.Id;
    }
}
