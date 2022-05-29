﻿using FSH.WebApi.Application.Common.Exporters;
using FSH.WebApi.Application.Operation.Orders;
using FSH.WebApi.Domain.Operation;

namespace FSH.WebApi.Application.Operation;

public class ExportOrderInvoiceRequest : IRequest<(string, Stream)>
{
  //todo: add validation at least one of those prop need to be filled
  public Guid? OrderId { get; set; }
  public string? OrderNumber { get; set; }
}

public class ExportOrderInvoiceRequestHandler : IRequestHandler<ExportOrderInvoiceRequest, (string, Stream)>
{
  private readonly IReadRepository<Order> _repository;
  private readonly IPdfWriter _pdfWriter;

  public ExportOrderInvoiceRequestHandler(IReadRepository<Order> repository, IPdfWriter pdfWriter)
  {
    _repository = repository;
    _pdfWriter = pdfWriter;
  }

  public async Task<(string, Stream)> Handle(ExportOrderInvoiceRequest request, CancellationToken cancellationToken)
  {
    var spec = new ExportOrderInvoiceWithBrandsSpec(request);

    var order = await _repository.GetBySpecAsync((ISpecification<Order, OrderExportDto>)spec, cancellationToken);
    var invoice = new InvoiceDocument(order);
    return (order.OrderNumber, _pdfWriter.WriteToStream(invoice));
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
      .Where(a =>
        (request.OrderId == null || a.Id == request.OrderId)
        || (request.OrderNumber == null || a.OrderNumber == request.OrderNumber));
}