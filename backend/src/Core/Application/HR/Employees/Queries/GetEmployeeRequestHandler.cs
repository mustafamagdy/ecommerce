using FSH.WebApi.Application.Common.Exceptions;
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For Employee
using MediatR;
using Microsoft.Extensions.Localization;

namespace FSH.WebApi.Application.HR.Employees.Queries;

public class GetEmployeeRequestHandler : IRequestHandler<GetEmployeeRequest, EmployeeDto>
{
    private readonly IReadRepository<Employee> _employeeRepository;
    private readonly IStringLocalizer<GetEmployeeRequestHandler> _t;

    public GetEmployeeRequestHandler(IReadRepository<Employee> employeeRepository, IStringLocalizer<GetEmployeeRequestHandler> localizer)
    {
        _employeeRepository = employeeRepository;
        _t = localizer;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeRequest request, CancellationToken cancellationToken)
    {
        var spec = new EmployeeByIdSpec(request.Id);
        var employeeDto = await _employeeRepository.FirstOrDefaultAsync(spec, cancellationToken);

        _ = employeeDto ?? throw new NotFoundException(_t["Employee {0} Not Found.", request.Id]);

        return employeeDto;
    }
}
