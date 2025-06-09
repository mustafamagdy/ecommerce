using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Application.HR.Payroll.Specifications; // For SalaryStructureByEmployeeIdSpec
using MediatR;
using Microsoft.Extensions.Localization;
using System.Linq;

namespace FSH.WebApi.Application.HR.Payroll;

public class GetSalaryStructureRequestHandler : IRequestHandler<GetSalaryStructureRequest, SalaryStructureDto>
{
    private readonly IReadRepository<SalaryStructure> _salaryStructureRepo;
    private readonly IStringLocalizer _t;

    public GetSalaryStructureRequestHandler(
        IReadRepository<SalaryStructure> salaryStructureRepo,
        IStringLocalizer<GetSalaryStructureRequestHandler> localizer)
    {
        _salaryStructureRepo = salaryStructureRepo;
        _t = localizer;
    }

    public async Task<SalaryStructureDto> Handle(GetSalaryStructureRequest request, CancellationToken cancellationToken)
    {
        var spec = new SalaryStructureByEmployeeIdSpec(request.EmployeeId);
        var salaryStructure = await _salaryStructureRepo.FirstOrDefaultAsync(spec, cancellationToken);

        _ = salaryStructure ?? throw new NotFoundException(_t["Salary structure for employee {0} not found.", request.EmployeeId]);

        // Manual Mapping to DTO
        var dto = new SalaryStructureDto
        {
            Id = salaryStructure.Id,
            EmployeeId = salaryStructure.EmployeeId,
            EmployeeName = salaryStructure.Employee != null ? $"{salaryStructure.Employee.FirstName} {salaryStructure.Employee.LastName}" : null,
            BasicSalary = salaryStructure.BasicSalary,
            Earnings = salaryStructure.Earnings.Select(e => new SalaryComponentDto
            {
                Name = e.Name,
                Amount = e.Amount,
                IsPercentage = e.IsPercentage
            }).ToList(),
            Deductions = salaryStructure.Deductions.Select(d => new SalaryComponentDto
            {
                Name = d.Name,
                Amount = d.Amount,
                IsPercentage = d.IsPercentage
            }).ToList(),
            CalculatedTotalEarnings = salaryStructure.CalculateTotalEarnings(),
            CalculatedTotalDeductions = salaryStructure.CalculateTotalDeductions(),
            CalculatedNetSalary = salaryStructure.CalculateNetSalary(),
            CreatedOn = salaryStructure.CreatedOn,
            LastModifiedOn = salaryStructure.LastModifiedOn
        };

        return dto;
    }
}
