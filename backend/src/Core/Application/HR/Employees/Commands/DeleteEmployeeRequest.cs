using MediatR;

namespace FSH.WebApi.Application.HR.Employees.Commands;

public class DeleteEmployeeRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteEmployeeRequest(Guid id) => Id = id;
}
