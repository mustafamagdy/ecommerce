using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.Common.Events;
using FSH.WebApi.Domain.HR;
using FSH.WebApi.Application.HR.Payroll.Specifications; // Added for SalaryStructureByEmployeeIdSpec
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Payroll;

// Spec for checking if a Payslip already exists for a given employee and pay period
public class PayslipByEmployeeAndPeriodSpec : Specification<Payslip>, ISingleResultSpecification
{
    public PayslipByEmployeeAndPeriodSpec(Guid employeeId, DateTime startDate, DateTime endDate) =>
        Query.Where(p => p.EmployeeId == employeeId && p.PayPeriodStartDate == startDate && p.PayPeriodEndDate == endDate);
}

public class GeneratePayslipRequestHandler : IRequestHandler<GeneratePayslipRequest, Guid>
{
    private readonly IRepositoryWithEvents<Payslip> _payslipRepo;
    private readonly IReadRepository<Employee> _employeeRepo;
    private readonly IReadRepository<SalaryStructure> _salaryStructureRepo; // Using IReadRepository for fetching
    private readonly IApplicationUnitOfWork _uow;
    private readonly IStringLocalizer _t;

    public GeneratePayslipRequestHandler(
        IRepositoryWithEvents<Payslip> payslipRepo,
        IReadRepository<Employee> employeeRepo,
        IReadRepository<SalaryStructure> salaryStructureRepo,
        IApplicationUnitOfWork uow,
        IStringLocalizer<GeneratePayslipRequestHandler> localizer)
    {
        _payslipRepo = payslipRepo;
        _employeeRepo = employeeRepo;
        _salaryStructureRepo = salaryStructureRepo;
        _uow = uow;
        _t = localizer;
    }

    public async Task<Guid> Handle(GeneratePayslipRequest request, CancellationToken cancellationToken)
    {
        // Validate EmployeeId
        var employee = await _employeeRepo.GetByIdAsync(request.EmployeeId, cancellationToken);
        _ = employee ?? throw new NotFoundException(_t["Employee with ID {0} not found.", request.EmployeeId]);

        // Fetch SalaryStructure
        var salaryStructureSpec = new SalaryStructureByEmployeeIdSpec(request.EmployeeId); // Using spec from Specifications folder
        var salaryStructure = await _salaryStructureRepo.FirstOrDefaultAsync(salaryStructureSpec, cancellationToken);
        _ = salaryStructure ?? throw new NotFoundException(_t["Salary structure for employee {0} not found. Please define it first.", request.EmployeeId]);

        // Check for existing payslip for the same period
        var existingPayslipSpec = new PayslipByEmployeeAndPeriodSpec(request.EmployeeId, request.PayPeriodStartDate, request.PayPeriodEndDate);
        var existingPayslip = await _payslipRepo.FirstOrDefaultAsync(existingPayslipSpec, cancellationToken);
        if (existingPayslip is not null)
        {
            throw new ConflictException(_t["A payslip for employee {0} for the period {1} to {2} already exists.",
                request.EmployeeId, request.PayPeriodStartDate.ToShortDateString(), request.PayPeriodEndDate.ToShortDateString()]);
        }

        // Calculations
        // Note: For financial calculations, always use decimal. Consistent rounding rules should be applied
        // if integrating with external financial systems or for display purposes to avoid minor discrepancies.

        int daysInMonth = DateTime.DaysInMonth(request.PayPeriodStartDate.Year, request.PayPeriodStartDate.Month);
        // Assuming PayPeriodStartDate and PayPeriodEndDate correctly define the actual days to be paid for.
        // This could be the full month, or a partial period if employee joined/left mid-month.
        int daysWorkedInPeriod = (request.PayPeriodEndDate - request.PayPeriodStartDate).Days + 1;

        decimal proRataFactor = 1.0m; // Default to 1 (full month)
        if (daysWorkedInPeriod < daysInMonth) // Basic pro-rata if period is less than the month's total days
        {
            proRataFactor = (decimal)daysWorkedInPeriod / daysInMonth;
        }
        // More complex pro-rata might consider actual join/leave dates from employee record if period is full month.

        decimal actualBasicSalaryForPeriod = salaryStructure.BasicSalary * proRataFactor;

        // Earnings & Deductions: Typically, percentage-based components are calculated on the *full* basic salary,
        // not the pro-rated one, unless specified otherwise by company policy. Fixed amounts are taken as is.
        // The SalaryComponent.CalculateActualValue method already uses the full BasicSalary from salaryStructure.
        decimal totalEarnings = salaryStructure.CalculateTotalEarnings();
        decimal totalDeductions = salaryStructure.CalculateTotalDeductions();

        decimal netSalary = actualBasicSalaryForPeriod + totalEarnings - totalDeductions;

        var payslip = new Payslip
        {
            EmployeeId = request.EmployeeId,
            PayPeriodStartDate = request.PayPeriodStartDate,
            PayPeriodEndDate = request.PayPeriodEndDate,
            BasicSalaryPaid = actualBasicSalaryForPeriod, // Pro-rated basic salary
            TotalEarnings = totalEarnings,
            TotalDeductions = totalDeductions,
            NetSalary = netSalary,
            GeneratedDate = DateTime.UtcNow,
            Status = PayslipStatus.Generated // Initial status after generation
        };

        // Snapshot components (amounts are based on full basic salary for percentages as per above comment)
        foreach (var earning in salaryStructure.Earnings)
        {
            payslip.Components.Add(new PayslipComponent(earning.Name, earning.CalculateActualValue(salaryStructure.BasicSalary), PayslipComponentType.Earning));
        }
        foreach (var deduction in salaryStructure.Deductions)
        {
            payslip.Components.Add(new PayslipComponent(deduction.Name, deduction.CalculateActualValue(salaryStructure.BasicSalary), PayslipComponentType.Deduction));
        }

        payslip.AddDomainEvent(EntityCreatedEvent.WithEntity(payslip));
        await _payslipRepo.AddAsync(payslip, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return payslip.Id;
    }
}
