using MediatR;

namespace FSH.WebApi.Application.HR.Employees.Queries;

public class GetEmployeeRequest : IRequest<EmployeeDto>
{
    public Guid Id { get; set; }

    public GetEmployeeRequest(Guid id) => Id = id;
}
