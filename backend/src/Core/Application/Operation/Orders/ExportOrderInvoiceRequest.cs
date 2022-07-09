using FSH.WebApi.Application.Common.Exporters;
using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation.Orders;

public class ExportOrderInvoiceRequest : IRequest<(string, Stream)>
{
  public Guid? OrderId { get; set; }
  public string? OrderNumber { get; set; } = default;
}

public class ExportOrderInvoiceRequestValidator : CustomValidator<ExportOrderInvoiceRequest>
{
  public ExportOrderInvoiceRequestValidator()
  {
    RuleFor(a => a.OrderId).NotEmpty().When(a => string.IsNullOrEmpty(a.OrderNumber));
    RuleFor(a => a.OrderNumber).NotEmpty().When(a => a.OrderId == null);
  }
}

public class ExportOrderInvoiceWithBrandsSpec : Specification<Order, OrderExportDto>, ISingleResultSpecification
{
  public ExportOrderInvoiceWithBrandsSpec(ExportOrderInvoiceRequest request) =>
    Query
      .Include(a => a.Customer)
      .Include(a => a.OrderPayments)
      .ThenInclude(a => a.PaymentMethod)
      .Include(a => a.OrderItems)
      .ThenInclude(a => a.ServiceCatalog)
      .ThenInclude(a => a.Product)
      .Include(a => a.OrderItems)
      .ThenInclude(a => a.ServiceCatalog)
      .ThenInclude(a => a.Product)
      .Where(a => request.OrderId == null
                  || a.Id == request.OrderId
                  || request.OrderNumber == null
                  || a.OrderNumber == request.OrderNumber);
}

public class ExportOrderInvoiceRequestHandler : IRequestHandler<ExportOrderInvoiceRequest, (string OrderNumber, Stream PdfInvoice)>
{
  private readonly IReadRepository<Order> _repository;
  private readonly IPdfWriter _pdfWriter;
  private readonly IVatQrCodeGenerator _vatQrCodeGenerator;
  private readonly IStringLocalizer _t;

  public ExportOrderInvoiceRequestHandler(IReadRepository<Order> repository, IPdfWriter pdfWriter, IVatQrCodeGenerator vatQrCodeGenerator, IStringLocalizer<ExportOrderInvoiceRequestHandler> localizer)
  {
    _repository = repository;
    _pdfWriter = pdfWriter;
    _vatQrCodeGenerator = vatQrCodeGenerator;
    _t = localizer;
  }

  public async Task<(string OrderNumber, Stream PdfInvoice)> Handle(ExportOrderInvoiceRequest request, CancellationToken
    cancellationToken)
  {
    var spec = new ExportOrderInvoiceWithBrandsSpec(request);

    var order = await _repository.GetBySpecAsync((ISpecification<Order, OrderExportDto>)spec, cancellationToken);
    if (order == null)
    {
      throw new NotFoundException(_t["Order #{0} ({1}) not found", request.OrderNumber ?? string.Empty, request.OrderId ?? Guid.Empty]);
    }

    var invoice = new InvoiceDocument(order, _vatQrCodeGenerator);
    return (order.OrderNumber, _pdfWriter.WriteToStream(invoice));
  }
}