using FSH.WebApi.Application.Common.Models; // For PaginationResponse
using FSH.WebApi.Application.Common.Persistence; // For IReadRepository
using FSH.WebApi.Domain.HR; // For Employee
using MediatR;

namespace FSH.WebApi.Application.HR.Employees.Queries;

public class SearchEmployeesRequestHandler : IRequestHandler<SearchEmployeesRequest, PaginationResponse<EmployeeDto>>
{
    private readonly IReadRepository<Employee> _repository;

    public SearchEmployeesRequestHandler(IReadRepository<Employee> repository) => _repository = repository;

    public async Task<PaginationResponse<EmployeeDto>> Handle(SearchEmployeesRequest request, CancellationToken cancellationToken)
    {
        var spec = new EmployeesBySearchRequestSpec(request);

        // The PaginatedListAsync method should ideally handle:
        // 1. Fetching the data based on the spec (including filters and ordering).
        // 2. Projecting the data to EmployeeDto using the spec's Select clause.
        // 3. Counting the total number of records matching the filters (before pagination).
        // 4. Returning a PaginationResponse<EmployeeDto>.
        //
        // If _repository.PaginatedListAsync doesn't do all of this (e.g., if it doesn't use the spec's projection
        // or count), then manual steps for projection or counting might be needed.
        // However, the pattern from SearchBrandsRequestHandler suggests it should work.

        return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
    }
}
