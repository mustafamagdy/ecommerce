using FSH.WebApi.Application.Common.Exporters;
using FSH.WebApi.Application.Printing;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Printing;

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
  private readonly IReadRepository<SimpleReceiptInvoice> _templateInvoice;
  private readonly IPdfWriter _pdfWriter;
  private readonly IVatQrCodeGenerator _vatQrCodeGenerator;
  private readonly IStringLocalizer _t;

  public ExportOrderInvoiceRequestHandler(IReadRepository<Order> repository, IPdfWriter pdfWriter, IVatQrCodeGenerator vatQrCodeGenerator, IStringLocalizer<ExportOrderInvoiceRequestHandler> localizer, IReadRepository<SimpleReceiptInvoice> templateInvoice)
  {
    _repository = repository;
    _pdfWriter = pdfWriter;
    _vatQrCodeGenerator = vatQrCodeGenerator;
    _t = localizer;
    _templateInvoice = templateInvoice;
  }

  public async Task<(string OrderNumber, Stream PdfInvoice)> Handle(ExportOrderInvoiceRequest request, CancellationToken
    cancellationToken)
  {
    var spec = new ExportOrderInvoiceWithBrandsSpec(request);

    var order = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (order == null)
    {
      throw new NotFoundException(_t["Order #{0} ({1}) not found", request.OrderNumber ?? string.Empty, request.OrderId ?? Guid.Empty]);
    }

    order.Company = new CompanyDto
    {
      Address = new AddressDto
      {
        City = new City
        {
          Name = "test city name"
        }
      }
    };

    var invoiceTemplate = await _templateInvoice.FirstOrDefaultAsync(
      new SingleResultSpecification<SimpleReceiptInvoice>()
        .Query
        .Include(a => a.Sections.OrderBy(x => x.Order))
        .Where(a => a.Active)
        .Specification, cancellationToken);

    var boundTemplate = new BoundTemplate(invoiceTemplate);
    boundTemplate.BindTemplate(order);

    var invoice = new InvoiceDocument(boundTemplate);
    return (order.OrderNumber, _pdfWriter.WriteToStream(invoice));
  }
}