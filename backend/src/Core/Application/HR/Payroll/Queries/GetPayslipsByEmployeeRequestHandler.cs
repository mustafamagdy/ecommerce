using FSH.WebApi.Application.Common.Models; // For PaginationResponse
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For Payslip entity
using FSH.WebApi.Application.HR.Payroll.Specifications; // For PayslipsByEmployeeSpec
using MediatR;

namespace FSH.WebApi.Application.HR.Payroll;

public class GetPayslipsByEmployeeRequestHandler : IRequestHandler<GetPayslipsByEmployeeRequest, PaginationResponse<PayslipDto>>
{
    private readonly IReadRepository<Payslip> _payslipRepo;

    public GetPayslipsByEmployeeRequestHandler(IReadRepository<Payslip> payslipRepo)
    {
        _payslipRepo = payslipRepo;
    }

    public async Task<PaginationResponse<PayslipDto>> Handle(GetPayslipsByEmployeeRequest request, CancellationToken cancellationToken)
    {
        var spec = new PayslipsByEmployeeSpec(request);
        return await _payslipRepo.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
    }
}
