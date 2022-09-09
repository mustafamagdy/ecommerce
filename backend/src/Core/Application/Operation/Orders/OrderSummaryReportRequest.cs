using FSH.WebApi.Application.Common.Exporters;
using FSH.WebApi.Application.Printing;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Shared.Multitenancy;

namespace FSH.WebApi.Application.Operation.Orders;

public class OrderSummaryReportRequest : IRequest<Stream>
{
  public Range<DateTime> OrderDate { get; set; }
}

public class OrderSummaryReportRequestValidator : CustomValidator<OrderSummaryReportRequest>
{
  public OrderSummaryReportRequestValidator()
  {
    // RuleFor(a => a.OrderId).NotEmpty().When(a => string.IsNullOrEmpty(a.OrderNumber));
    // RuleFor(a => a.OrderNumber).NotEmpty().When(a => a.OrderId == null);
  }
}

public class GetOrdersForSummaryReportSpec : Specification<Order, OrderSummaryDto>, ISingleResultSpecification
{
  public GetOrdersForSummaryReportSpec(OrderSummaryReportRequest request) =>
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
      .Where(a => (request.OrderDate.From == null || a.OrderDate >= request.OrderDate.From)
                  && (request.OrderDate.From == null || a.OrderDate <= request.OrderDate.To));
}

public class OrderSummaryReportRequestHandler : IRequestHandler<OrderSummaryReportRequest, Stream>
{
  private readonly IReadRepository<Order> _repository;
  private readonly IReadRepository<OrdersSummaryReport> _templateInvoice;
  private readonly IPdfWriter _pdfWriter;
  private readonly IStringLocalizer _t;
  private readonly ISubscriptionTypeResolver _subscriptionTypeResolver;

  public OrderSummaryReportRequestHandler(IReadRepository<Order> repository, IPdfWriter pdfWriter,
    IStringLocalizer<OrderSummaryReportRequestHandler> localizer, IReadRepository<OrdersSummaryReport> templateInvoice,
    ISubscriptionTypeResolver subscriptionTypeResolver)
  {
    _repository = repository;
    _pdfWriter = pdfWriter;
    _t = localizer;
    _templateInvoice = templateInvoice;
    _subscriptionTypeResolver = subscriptionTypeResolver;
  }

  public async Task<Stream> Handle(OrderSummaryReportRequest request, CancellationToken cancellationToken)
  {
    var spec = new GetOrdersForSummaryReportSpec(request);

    var orders = await _repository.ListAsync(spec, cancellationToken);
    var reportDto = new OrderSummaryReportDto()
    {
      Orders = orders,
      DateFrom = request.OrderDate.From?.ToShortDateString() ?? "_",
      DateTo = request.OrderDate.To?.ToShortDateString() ?? "_",
      TotalAmount = orders.Sum(a => a.TotalAmount),
      TotalPaid = orders.Sum(a => a.TotalPaid),
      TotalVat = orders.Sum(a => a.TotalVat),
      TotalRemaining = orders.Sum(a => a.NetAmount - a.TotalPaid)
    };

    var ordersSummaryReportTemplate = await _templateInvoice.FirstOrDefaultAsync(
      new SingleResultSpecification<OrdersSummaryReport>()
        .Query
        .Include(a => a.Sections.OrderBy(x => x.Order))
        .Where(a => a.Active)
        .Specification, cancellationToken);

    var boundTemplate = new BoundTemplate(ordersSummaryReportTemplate);
    boundTemplate.BindTemplate(reportDto);

    var subscriptionType = _subscriptionTypeResolver.Resolve();

    var pdf = new InvoiceDocument(subscriptionType, boundTemplate);
    return _pdfWriter.WriteToStream(pdf);
  }
}