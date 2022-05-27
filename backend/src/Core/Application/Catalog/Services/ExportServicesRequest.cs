using FSH.WebApi.Application.Common.Exporters;

namespace FSH.WebApi.Application.Catalog.Services;

public class ExportServicesRequest : BaseFilter, IRequest<Stream>
{
  public Guid? ServiceCategoryId { get; set; }
  public decimal? MinimumRate { get; set; }
  public decimal? MaximumRate { get; set; }
}

public class ExportServicesRequestHandler : IRequestHandler<ExportServicesRequest, Stream>
{
  private readonly IReadRepository<Service> _repository;
  private readonly IExcelWriter _excelWriter;

  public ExportServicesRequestHandler(IReadRepository<Service> repository, IExcelWriter excelWriter)
  {
    _repository = repository;
    _excelWriter = excelWriter;
  }

  public async Task<Stream> Handle(ExportServicesRequest request, CancellationToken cancellationToken)
  {
    var spec = new ExportServicesWithServiceCategoriesSpecification(request);

    var list = await _repository.ListAsync(spec, cancellationToken);

    return _excelWriter.WriteToStream(list);
  }
}

public class ExportServicesWithServiceCategoriesSpecification : EntitiesByBaseFilterSpec<Service, ServiceExportDto>
{
  public ExportServicesWithServiceCategoriesSpecification(ExportServicesRequest request)
    : base(request) =>
    Query
      .Include(p => p.ServiceCategory)
      .Where(p => p.ServiceCategoryId.Equals(request.ServiceCategoryId!.Value), request.ServiceCategoryId.HasValue);
}