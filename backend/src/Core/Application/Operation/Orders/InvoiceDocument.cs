#define troubleshoot

using System.Reflection;
using FSH.WebApi.Application.Common.Pdf;
using FSH.WebApi.Application.Printing;
using FSH.WebApi.Shared.Multitenancy;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Unit = QuestPDF.Infrastructure.Unit;

namespace FSH.WebApi.Application.Operation.Orders
{
  public class InvoiceDocument : BasePdfDocument
  {
    private readonly IVatQrCodeGenerator _qrGenerator;
    public OrderExportDto Model { get; }
    private byte[] _qrCode = null!;

    public InvoiceDocument(OrderExportDto model, IVatQrCodeGenerator qrGenerator) 
        : base(SubscriptionType.Standard)
    {
      _qrGenerator = qrGenerator;
      Model = model;
    }

    public InvoiceDocument(SubscriptionType subscriptionType, BoundTemplate boundTemplate) 
        : base(subscriptionType)
    {
      // Handle the case where we're creating from a template
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
      _qrCode = _qrGenerator?.GenerateQrCode(Model?.Base64QrCode, 100, 100);
      string? appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      byte[]? logo = appPath != null ? File.ReadAllBytes(appPath + "/Files/logos/tenant_logo.png") : null;

      container.Column(col =>
      {
        col.Item().Height(30)
          .AlignMiddle().AlignCenter()
          .Width(100).Image(logo, ImageScaling.Resize);

        col
          .Item()
          .Height(38)
          .Background(Colors.White)
          .AlignCenter()
          .AlignMiddle()
          .Text($"*{Model?.OrderNumber}*")
          .LineHeight(1)
          .FontFamily("Libre Barcode 39") // use real font family name
          .FontSize(36);

        col
          .Item()
          .AlignCenter().AlignMiddle()
          .Text(Model?.OrderNumber)
          .FontSize(10);

        col.Item().SeparatorLine('=', 70);
        col.Item().AlignMiddle().AlignCenter().Text("فاتورة ضريبية مبسطة").FontSize(10);
        col.Item().AlignMiddle().AlignCenter().Text("Simplified Tax Invoice").FontSize(10);
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

        var totalPrice = Model?.TotalAmount ?? 0;
        column.Item().PaddingRight(5).AlignRight().Text($"Grand total: {totalPrice:N2}").SemiBold();

        // if (!string.IsNullOrWhiteSpace(Model.Comments))
        //   column.Item().PaddingTop(25).Element(ComposeComments);
      });
    }

    protected override void Footer(IContainer container)
    {
      container.Column(col =>
      {
        if (_qrCode != null)
        {
          col.Item().AlignCenter().Width(70).Height(70).Image(_qrCode, ImageScaling.Resize);
        }
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

        // string text = "كلام طويل وملوش اخر صدقني انا بقولك انه مش عارف اخره فين ولذلك هقولك كمان شوية";
        if (Model?.OrderItems != null)
        {
          foreach (var item in Model.OrderItems)
          {
            table.Cell().Element(CellStyle).AlignLeft().AlignTop().Text($"{item.Price:F0}");
            table.Cell().Element(CellStyle).AlignLeft().AlignTop().Text(item.Qty);
            table.Cell().Element(CellStyle).AlignRight().AlignTop().Text(item.ItemName);

            static IContainer CellStyle(IContainer container) => container.PaddingHorizontal(2);

            // .BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
          }
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
}