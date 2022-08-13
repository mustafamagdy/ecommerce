#define troubleshoot

using System.Reflection;
using FSH.WebApi.Application.Common.Pdf;
using FSH.WebApi.Application.Printing;
using FSH.WebApi.Domain.Printing;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Unit = QuestPDF.Infrastructure.Unit;

namespace FSH.WebApi.Application.Operation.Orders
{
  public class DemoInvoiceDocument : BasePdfDocument
  {
    private readonly IVatQrCodeGenerator _qrGenerator;
    public OrderExportDto Model { get; }
    private byte[] _qrCode = null!;

    public DemoInvoiceDocument(OrderExportDto model, IVatQrCodeGenerator qrGenerator)
    {
      _qrGenerator = qrGenerator;
      Model = model;
    }

    protected override void SetupPage(PageDescriptor page)
    {
      base.SetupPage(page);
      page.ContinuousSize(3, unit: Unit.Inch);
    }

    protected override void RenderHeader(IContainer container)
    {
      _qrCode = _qrGenerator.GenerateQrCode(Model.Base64QrCode, 100, 100);
      string? appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      byte[] logo = File.ReadAllBytes(appPath + "/Files/logos/tenant_logo.png");

      container.Column(col =>
      {
        col.Item().Height(30)
          .AlignMiddle().AlignCenter().DebugArea()
          .Width(100).Image(logo, ImageScaling.Resize);

        col
          .Item()
          .Height(38)
          .DebugArea()
          .Background(Colors.White)
          .AlignCenter()
          .AlignMiddle()
          .Text($"*{Model.OrderNumber}*")
          .LineHeight(1)
          .FontFamily("Libre Barcode 39") // use real font family name
          .FontSize(36);

        col
          .Item()
          .DebugArea()
          .AlignCenter().AlignMiddle()
          .Text(Model.OrderNumber)
          .FontSize(10);

        col.Item().SeparatorLine('=', 70);
        col.Item().AlignMiddle().AlignCenter().DebugArea().Text("فاتورة ضريبية مبسطة").FontSize(10);
        col.Item().AlignMiddle().AlignCenter().DebugArea().Text("Simplified Tax Invoice").FontSize(10);
      });
    }

    protected override void Body(IContainer container)
    {
      container.PaddingVertical(5).Column(column =>
      {
        column.Spacing(5);

        column.Item().Element(ComposeTable);

        var totalPrice = Model.TotalAmount;
        column.Item().PaddingRight(5).AlignRight().Text($"Grand total: {totalPrice:N2}").SemiBold();
      });
    }

    protected override void Footer(IContainer container)
    {
      container.Column(col =>
      {
        col.Item().AlignCenter().Width(70).Height(70).Image(_qrCode, ImageScaling.Resize);
        col.Item().SeparatorLine();

        col.Item().AlignCenter().Text(text =>
        {
          text.CurrentPageNumber();
          text.Span(" / ");
          text.TotalPages();
        });
        col.Item().SeparatorLine();
      });
    }

    private void ComposeTable(IContainer container)
    {
      var headerStyle = TextStyle.Default.FontSize(8).SemiBold();

      container.Table(table =>
      {
        table.ColumnsDefinition(columns =>
        {
          columns.ConstantColumn(25);
          columns.ConstantColumn(40);
          columns.RelativeColumn();
        });

        table.Header(header =>
        {
          header.Cell().AlignLeft().Text("Price").Style(headerStyle);
          header.Cell().AlignLeft().Text("Qty").Style(headerStyle);
          header.Cell().AlignRight().Text("Item").Style(headerStyle);
        });

        foreach (var item in Model.OrderItems)
        {
          table.Cell().Element(CellStyle).AlignLeft().AlignTop().Text($"{item.Price:F0}");
          table.Cell().Element(CellStyle).AlignLeft().AlignTop().Text(item.Qty);
          table.Cell().Element(CellStyle).AlignRight().AlignTop().Text(item.ItemName);

          static IContainer CellStyle(IContainer container) => container.PaddingHorizontal(2);
        }
      });
    }
  }
}