using FSH.WebApi.Shared.Extensions;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Application.Operation.Orders
{
  public class InvoiceDocument : IDocument
  {
    public OrderExportDto Model { get; }

    public InvoiceDocument(OrderExportDto model)
    {
      Model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
      container
        .Page(page =>
        {
          page.Margin(50);

          page.DefaultTextStyle(TextStyle.Default.FontFamily("Arial"));

          page.Header().Element(ComposeHeader);
          page.Content().Element(ComposeContent);

          page.Footer().AlignCenter().Text(text =>
          {
            text.CurrentPageNumber();
            text.Span(" / ");
            text.TotalPages();
          });
        });
    }

    void ComposeHeader(IContainer container)
    {
      container.Row(row =>
      {
        row.RelativeItem().Column(Column =>
        {
          Column
            .Item().Text($"Order #{Model.OrderNumber}")
            .FontSize(20).SemiBold().FontColor(Colors.Purple.Accent4);

          Column.Item().Text(text =>
          {
            text.Span("Order date: ").SemiBold();
            text.Span($"{Model.OrderDate}");
          });

          Column.Item().Text(text =>
          {
            text.Span("Order time: ").SemiBold();
            text.Span($"{Model.OrderTime}");
          });
        });

        // row.ConstantItem(100).Height(50).Placeholder();
      });
    }

    void ComposeContent(IContainer container)
    {
      container.PaddingVertical(40).Column(column =>
      {
        column.Spacing(20);


        column.Item().Element(ComposeTable);

        var totalPrice = Model.TotalAmount;
        column.Item().PaddingRight(5).AlignRight().Text($"Grand total: {totalPrice:N2}", TextStyle.Default.SemiBold());

        // if (!string.IsNullOrWhiteSpace(Model.Comments))
        //   column.Item().PaddingTop(25).Element(ComposeComments);
      });
    }

    void ComposeTable(IContainer container)
    {
      var headerStyle = TextStyle.Default.SemiBold();

      container.Table(table =>
      {
        table.ColumnsDefinition(columns =>
        {
          columns.ConstantColumn(25);
          columns.RelativeColumn(3);
          columns.RelativeColumn();
          columns.RelativeColumn();
          columns.RelativeColumn();
        });

        table.Header(header =>
        {
          header.Cell().Text("#");
          header.Cell().Text("Product").Style(headerStyle);
          header.Cell().AlignRight().Text("Unit price").Style(headerStyle);
          header.Cell().AlignRight().Text("Quantity").Style(headerStyle);
          header.Cell().AlignRight().Text("Total").Style(headerStyle);

          header.Cell().ColumnSpan(5).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
        });

        foreach (var item in Model.OrderItems)
        {
          table.Cell().Element(CellStyle).Text(Model.OrderItems.IndexOf(item) + 1);
          table.Cell().Element(CellStyle).Text(item.ItemName.ArabicWithFontGlyphsToPfd());
          table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price:F2}");
          table.Cell().Element(CellStyle).AlignRight().Text(item.Qty);
          table.Cell().Element(CellStyle).AlignRight().Text($"{(item.Price * item.Qty):N2}");

          static IContainer CellStyle(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
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