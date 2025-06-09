using FSH.WebApi.Application.Common.Models; // For PaginationFilter, PaginationResponse
using MediatR;

namespace FSH.WebApi.Application.HR.Payroll;

public class GetPayslipsByEmployeeRequest : PaginationFilter, IRequest<PaginationResponse<PayslipDto>>
{
    public Guid EmployeeId { get; set; }

    // Constructor to easily set EmployeeId, PaginationFilter properties can be set by consumer
    public GetPayslipsByEmployeeRequest(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}
