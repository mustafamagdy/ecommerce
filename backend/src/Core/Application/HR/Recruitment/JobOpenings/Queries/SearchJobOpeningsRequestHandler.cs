using FSH.WebApi.Application.Common.Models; // For PaginationResponse
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Domain.HR; // For JobOpening entity
using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Dtos; // For JobOpeningDto
using FSH.WebApi.Application.HR.Recruitment.JobOpenings.Specifications; // For JobOpeningsBySearchRequestSpec
using MediatR;

namespace FSH.WebApi.Application.HR.Recruitment.JobOpenings.Queries;

public class SearchJobOpeningsRequestHandler : IRequestHandler<SearchJobOpeningsRequest, PaginationResponse<JobOpeningDto>>
{
    private readonly IReadRepository<JobOpening> _repository;

    public SearchJobOpeningsRequestHandler(IReadRepository<JobOpening> repository) => _repository = repository;

    public async Task<PaginationResponse<JobOpeningDto>> Handle(SearchJobOpeningsRequest request, CancellationToken cancellationToken)
    {
        var spec = new JobOpeningsBySearchRequestSpec(request);
        return await _repository.PaginatedListAsync(spec, request.PageNumber, request.PageSize, cancellationToken);
    }
}
