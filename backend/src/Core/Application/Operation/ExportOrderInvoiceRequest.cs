using FSH.WebApi.Application.Catalog.Products;
using FSH.WebApi.Application.Common.Exporters;

namespace FSH.WebApi.Application.Operation;

public class ExportOrderInvoiceRequest : BaseFilter, IRequest<Stream>
{
  public Guid? BrandId { get; set; }
  public decimal? MinimumRate { get; set; }
  public decimal? MaximumRate { get; set; }
}

public class ExportOrderInvoiceRequestHandler : IRequestHandler<ExportOrderInvoiceRequest, Stream>
{
  private readonly IReadRepository<Product> _repository;
  private readonly IExcelWriter _excelWriter;

  public ExportOrderInvoiceRequestHandler(IReadRepository<Product> repository, IExcelWriter excelWriter)
  {
    _repository = repository;
    _excelWriter = excelWriter;
  }

  public async Task<Stream> Handle(ExportOrderInvoiceRequest request, CancellationToken cancellationToken)
  {
    var spec = new ExportOrderInvoiceWithBrandsSpecification(request);

    var list = await _repository.ListAsync(spec, cancellationToken);

    return _excelWriter.WriteToStream(list);
  }
}

public class ExportOrderInvoiceWithBrandsSpecification : EntitiesByBaseFilterSpec<Product, ProductExportDto>
{
  public ExportOrderInvoiceWithBrandsSpecification(ExportOrderInvoiceRequest request)
    : base(request) =>
    Query
      .Include(p => p.Brand)
      .Where(p => p.BrandId.Equals(request.BrandId!.Value), request.BrandId.HasValue)
      .Where(p => p.Rate >= request.MinimumRate!.Value, request.MinimumRate.HasValue)
      .Where(p => p.Rate <= request.MaximumRate!.Value, request.MaximumRate.HasValue);
}