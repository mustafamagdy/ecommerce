// #define troubleshoot

using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Unit = QuestPDF.Infrastructure.Unit;

namespace FSH.WebApi.Application.Operation.Orders
{
  public abstract class BasePdfDocument : IDocument
  {
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
      container.Page(page =>
      {
        SetupPage(page);

        page.Header().Height(100)
#if troubleshoot
          .Background(Colors.Blue.Accent1)
#endif
          .Element(RenderHeader);

        page.Content().MinHeight(100)
#if troubleshoot
          .Background(Colors.Red.Accent1)
#endif
          .Element(Body);

        page.Footer().Height(150)
#if troubleshoot
          .Background(Colors.Yellow.Accent1)
#endif
          .Element(Footer);
      });
    }

    protected virtual void SetupPage(PageDescriptor page)
    {
      page.Margin(5);
      page.DefaultTextStyle(TextStyle.Default.FontFamily("Arial"));
    }

    protected abstract void RenderHeader(IContainer container);
    protected abstract void Body(IContainer container);
    protected abstract void Footer(IContainer container);
  }

  public class InvoiceDocument : BasePdfDocument
  {
    private readonly IInvoiceBarcodeGenerator _qrGenerator;
    public OrderExportDto Model { get; }
    private byte[] _qrCode;

    public InvoiceDocument(OrderExportDto model, IInvoiceBarcodeGenerator qrGenerator)
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
      // container.Row(row =>
      // {
      //   row.RelativeItem().Column(col =>
      //   {
      //     col.Item().BorderColor(Colors.Black).Border(1, Unit.Point).AlignCenter().Text("LOGO").FontSize(10);
      //   });
      // });
      _qrCode = _qrGenerator.GenerateQrCode(Model.Base64QrCode, 100, 100);
      container.Column(col =>
      {
        col.Item().Height(50)
          .AlignMiddle().AlignCenter().ShowTroubleshootBorders()
          .Width(100).Image(_qrCode, ImageScaling.Resize);

        col.Item().AlignCenter().ShowTroubleshootBorders().Text("LOGO 2").FontSize(10);
        // col.Item().AlignCenter().ShowTroubleshootBorders().Text("LOGO 3").FontSize(10);
        col.Item().SeparatorLine('=', 70);
        col.Item().AlignMiddle().AlignCenter().ShowTroubleshootBorders().Text("فاتورة ضريبية مبسطة").FontSize(10);
        col.Item().AlignMiddle().AlignCenter().ShowTroubleshootBorders().Text("Simplified Tax Invoice").FontSize(10);
      });

      // container.Row(row =>
      // {
      //   row.RelativeItem().Column(Column =>
      //   {
      //     Column
      //       .Item().Text($"Order #{Model.OrderNumber}")
      //       .FontSize(10).SemiBold().FontColor(Colors.Purple.Accent4);
      //
      //     Column.Item().Text(text =>
      //     {
      //       text.Span("Order date: ").SemiBold();
      //       text.Span($"{Model.OrderDate: yyyy-MM-dd}");
      //     });
      //
      //     Column.Item().Text(text =>
      //     {
      //       text.Span("Order time: ").SemiBold();
      //       text.Span($"{Model.OrderDate:hh:mm:ss tt}");
      //     });
      //   });
      //
      //   // row.ConstantItem(100).Height(100).Image(_qrImage, ImageScaling.Resize);
      // });
    }

    protected override void Body(IContainer container)
    {
      container.PaddingVertical(5).Column(column =>
      {
        column.Spacing(5);

        column.Item().Element(ComposeTable);

        var totalPrice = Model.TotalAmount;
        column.Item().PaddingRight(5).AlignRight().Text($"Grand total: {totalPrice:N2}", TextStyle.Default.SemiBold());

        // if (!string.IsNullOrWhiteSpace(Model.Comments))
        //   column.Item().PaddingTop(25).Element(ComposeComments);
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

    void ComposeTable(IContainer container)
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

        var text = "كلام طويل وملوش اخر صدقني انا بقولك انه مش عارف اخره فين ولذلك هقولك كمان شوية";
        foreach (var item in Model.OrderItems)
        {
          table.Cell().Element(CellStyle).AlignLeft().AlignTop().Text($"{item.Price:F0}");
          table.Cell().Element(CellStyle).AlignLeft().AlignTop().Text(item.Qty);
          table.Cell().Element(CellStyle).AlignRight().AlignTop().Text(item.ItemName);

          static IContainer CellStyle(IContainer container) => container.PaddingHorizontal(2);
          //.BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
        }
      });
    }
  }

  // public class AddressComponent : IComponent
  // {
  //   private string Title { get; }
  //   private Address Address { get; }
  //
  //   public AddressComponent(string title, Address address)
  //   {
  //     Title = title;
  //     Address = address;
  //   }
  //
  //   public void Compose(IContainer container)
  //   {
  //     container.ShowEntire().Column(column =>
  //     {
  //       column.Spacing(2);
  //
  //       column.Item().Text(Title).SemiBold();
  //       column.Item().PaddingBottom(5).LineHorizontal(1);
  //
  //       column.Item().Text(Address.CompanyName);
  //       column.Item().Text(Address.Street);
  //       column.Item().Text($"{Address.City}, {Address.State}");
  //       column.Item().Text(Address.Email);
  //       column.Item().Text(Address.Phone);
  //     });
  //   }
  // }


  public static class PdfDocumentEx
  {
    public static IContainer SeparatorLine(this IContainer element, char c = 'x', int charCount = 80)
    {
      var lineStr = new string(c, charCount);
      element.AlignCenter().AlignTop().Text(lineStr, TextStyle.Default.FontSize(5));
      return element;
    }

    public static IContainer ShowTroubleshootBorders(this IContainer element)
    {
#if troubleshoot
      element = element.BorderColor(Colors.Black).Border(1, Unit.Point);
#endif
      return element;
    }
  }
}