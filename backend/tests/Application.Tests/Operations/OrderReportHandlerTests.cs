using FluentAssertions;
using FSH.WebApi.Application.Common.Exporters;
using FSH.WebApi.Application.Common.Persistence;
using FSH.WebApi.Application.Operation.Orders;
using FSH.WebApi.Application.Printing;
using FSH.WebApi.Domain.Operation;
using FSH.WebApi.Domain.Printing;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.Extensions.Localization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Ardalis.Specification;

namespace FSH.WebApi.Application.Tests.Operations;

public class OrderReportHandlerTests
{
    private readonly IReadRepository<Order> _orderRepository;
    private readonly IReadRepository<SimpleReceiptInvoice> _invoiceTemplateRepository;
    private readonly IReadRepository<OrdersSummaryReport> _summaryTemplateRepository;
    private readonly IPdfWriter _pdfWriter;
    private readonly IVatQrCodeGenerator _qrGenerator;
    private readonly IStringLocalizer<ExportOrderInvoiceRequestHandler> _invoiceLocalizer;
    private readonly IStringLocalizer<OrderSummaryReportRequestHandler> _summaryLocalizer;
    private readonly ISubscriptionTypeResolver _subscriptionTypeResolver;

    public OrderReportHandlerTests()
    {
        _orderRepository = Substitute.For<IReadRepository<Order>>();
        _invoiceTemplateRepository = Substitute.For<IReadRepository<SimpleReceiptInvoice>>();
        _summaryTemplateRepository = Substitute.For<IReadRepository<OrdersSummaryReport>>();
        _pdfWriter = Substitute.For<IPdfWriter>();
        _qrGenerator = Substitute.For<IVatQrCodeGenerator>();
        _invoiceLocalizer = Substitute.For<IStringLocalizer<ExportOrderInvoiceRequestHandler>>();
        _summaryLocalizer = Substitute.For<IStringLocalizer<OrderSummaryReportRequestHandler>>();
        _subscriptionTypeResolver = Substitute.For<ISubscriptionTypeResolver>();
        _subscriptionTypeResolver.Resolve().Returns(SubscriptionType.Standard);
    }

    private static Order CreateOrder(Guid id)
    {
        var customer = new Customer("Test", "123");
        var order = new Order(customer, "ORD001", DateTime.Today);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(order, id);
        return order;
    }

    [Fact]
    public async Task ExportOrderInvoice_Should_Return_Pdf_Stream()
    {
        var order = CreateOrder(Guid.NewGuid());
        _orderRepository.FirstOrDefaultAsync(Arg.Any<ExportOrderInvoiceWithBrandsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Order?>(order));
        _invoiceTemplateRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<SimpleReceiptInvoice>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SimpleReceiptInvoice?>(new SimpleReceiptInvoice()));
        var pdf = new MemoryStream();
        _pdfWriter.WriteToStream(Arg.Any<InvoiceDocument>()).Returns(pdf);

        var handler = new ExportOrderInvoiceRequestHandler(
            _orderRepository,
            _pdfWriter,
            _qrGenerator,
            _invoiceLocalizer,
            _invoiceTemplateRepository,
            _subscriptionTypeResolver);

        var request = new ExportOrderInvoiceRequest { OrderId = order.Id };
        var result = await handler.Handle(request, CancellationToken.None);

        result.PdfInvoice.Should().BeSameAs(pdf);
        result.OrderNumber.Should().Be(order.OrderNumber);
        _pdfWriter.Received(1).WriteToStream(Arg.Any<InvoiceDocument>());
    }

    [Fact]
    public async Task ExportOrderInvoice_Should_Throw_When_Not_Found()
    {
        _orderRepository.FirstOrDefaultAsync(Arg.Any<ExportOrderInvoiceWithBrandsSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Order?>(null));
        var handler = new ExportOrderInvoiceRequestHandler(
            _orderRepository,
            _pdfWriter,
            _qrGenerator,
            _invoiceLocalizer,
            _invoiceTemplateRepository,
            _subscriptionTypeResolver);
        var request = new ExportOrderInvoiceRequest { OrderId = Guid.NewGuid() };
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task OrderSummaryReport_Should_Return_Pdf_Stream()
    {
        var from = new DateTime(2023, 1, 1);
        var to = new DateTime(2023, 1, 31);
        var request = new OrderSummaryReportRequest { OrderDate = new Range<DateTime>(from, to) };

        var orders = new List<OrderSummaryDto>
        {
            new() { OrderNumber = "O1", OrderDate = from, TotalAmount = 100m, TotalPaid = 50m, TotalVat = 5m, NetAmount = 95m }
        };

        _orderRepository.ListAsync(Arg.Any<GetOrdersForSummaryReportSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(orders));
        _summaryTemplateRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<OrdersSummaryReport>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OrdersSummaryReport?>(new OrdersSummaryReport()));
        var pdf = new MemoryStream();
        _pdfWriter.WriteToStream(Arg.Any<InvoiceDocument>()).Returns(pdf);

        var handler = new OrderSummaryReportRequestHandler(
            _orderRepository,
            _pdfWriter,
            _summaryLocalizer,
            _summaryTemplateRepository,
            _subscriptionTypeResolver);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeSameAs(pdf);
        _pdfWriter.Received(1).WriteToStream(Arg.Any<InvoiceDocument>());
    }

    [Fact]
    public async Task OrderSummaryReport_Should_Handle_No_Orders()
    {
        var request = new OrderSummaryReportRequest { OrderDate = new Range<DateTime>(DateTime.Today, DateTime.Today) };
        var orders = new List<OrderSummaryDto>();
        _orderRepository.ListAsync(Arg.Any<GetOrdersForSummaryReportSpec>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(orders));
        _summaryTemplateRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<OrdersSummaryReport>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OrdersSummaryReport?>(new OrdersSummaryReport()));
        var pdf = new MemoryStream();
        _pdfWriter.WriteToStream(Arg.Any<InvoiceDocument>()).Returns(pdf);
        var handler = new OrderSummaryReportRequestHandler(
            _orderRepository,
            _pdfWriter,
            _summaryLocalizer,
            _summaryTemplateRepository,
            _subscriptionTypeResolver);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().BeSameAs(pdf);
        _pdfWriter.Received(1).WriteToStream(Arg.Any<InvoiceDocument>());
    }
}
