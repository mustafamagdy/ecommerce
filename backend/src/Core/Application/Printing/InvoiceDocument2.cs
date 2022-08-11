// using FSH.WebApi.Application.Common.Pdf;
// using FSH.WebApi.Domain.Printing;
// using QuestPDF.Fluent;
// using QuestPDF.Helpers;
// using QuestPDF.Infrastructure;
// using Unit = QuestPDF.Infrastructure.Unit;
//
// namespace FSH.WebApi.Application.Printing
// {
//   public class InvoiceDocument2 : BasePdfDocument
//   {
//     private readonly ContinuesFixedSizeReceiptInvoice _template;
//
//     public InvoiceDocument2(ContinuesFixedSizeReceiptInvoice template)
//     {
//       _template = template;
//     }
//
//     protected override void SetupPage(PageDescriptor page)
//     {
//       base.SetupPage(page);
//       page.ContinuousSize(_template.Width, unit: Unit.Inch);
//     }
//
//     protected override void RenderHeader(IContainer container)
//     {
//       var headerComponents = _template.Sections
//         .Where(a => a.Type == SectionPosition.Header)
//         .OrderBy(a => a.Order)
//         .ToArray();
//
//       container.Column(col =>
//       {
//         foreach (var component in headerComponents)
//         {
//           switch (component.Type.Name)
//           {
//             case nameof(SectionType.Logo):
//               RenderLogo(col, (component as LogoSection)!.SetData(_logo));
//               break;
//             case nameof(SectionType.Title):
//               RenderText(col, component as TextSection);
//               break;
//             case nameof(SectionType.Barcode):
//               RenderBarcode(col, component as BarcodeSection);
//               break;
//
//             default:
//               break;
//           }
//         }
//
//         col.Item().SeparatorLine('=', 70);
//         col.Item().AlignMiddle().AlignCenter().ShowDebugArea().Text("فاتورة ضريبية مبسطة").FontSize(10);
//         col.Item().AlignMiddle().AlignCenter().ShowDebugArea().Text("Simplified Tax Invoice").FontSize(10);
//       });
//
//       // container.Row(row =>
//       // {
//       //   row.RelativeItem().Column(Column =>
//       //   {
//       //     Column
//       //       .Item().Text($"Order #{Model.OrderNumber}")
//       //       .FontSize(10).SemiBold().FontColor(Colors.Purple.Accent4);
//       //
//       //     Column.Item().Text(text =>
//       //     {
//       //       text.Span("Order date: ").SemiBold();
//       //       text.Span($"{Model.OrderDate: yyyy-MM-dd}");
//       //     });
//       //
//       //     Column.Item().Text(text =>
//       //     {
//       //       text.Span("Order time: ").SemiBold();
//       //       text.Span($"{Model.OrderDate:hh:mm:ss tt}");
//       //     });
//       //   });
//       //
//       //   // row.ConstantItem(100).Height(100).Image(_qrImage, ImageScaling.Resize);
//       // });
//     }
//
//     private void RenderBarcode(ColumnDescriptor col, BarcodeSection barcodeSection)
//     {
//       col
//         .Item()
//         .Height(38)
//         .ShowDebugArea()
//         .Background(Colors.White)
//         .AlignCenter()
//         .AlignMiddle()
//         .Text($"*{Model.OrderNumber}*")
//         .LineHeight(1)
//         .FontFamily("Libre Barcode 39") // use real font family name
//         .FontSize(36);
//     }
//
//     private void RenderText(ColumnDescriptor col, TextSection textSection)
//     {
//       col
//         .Item()
//         .ShowDebugArea()
//         .AlignCenter().AlignMiddle()
//         .Text(Model.OrderNumber)
//         .FontSize(10);
//     }
//
//     private void RenderLogo(ColumnDescriptor col, LogoSection logoSection)
//     {
//       col.Item().Height(30)
//         .AlignMiddle().AlignCenter().ShowDebugArea()
//         .Width(100).Image(logoSection.ImageData, ImageScaling.Resize);
//     }
//
//     protected override void Body(IContainer container)
//     {
//       container.PaddingVertical(5).Column(column =>
//       {
//         column.Spacing(5);
//
//         column.Item().Element(ComposeTable);
//
//         var totalPrice = Model.TotalAmount;
//         column.Item().PaddingRight(5).AlignRight().Text($"Grand total: {totalPrice:N2}").SemiBold();
//
//         // if (!string.IsNullOrWhiteSpace(Model.Comments))
//         //   column.Item().PaddingTop(25).Element(ComposeComments);
//       });
//     }
//
//     protected override void Footer(IContainer container)
//     {
//       container.Column(col =>
//       {
//         col.Item().AlignCenter().Width(70).Height(70).Image(_qrCode, ImageScaling.Resize);
//         col.Item().SeparatorLine();
//
//         col.Item().AlignCenter().Text(text =>
//         {
//           text.CurrentPageNumber();
//           text.Span(" / ");
//           text.TotalPages();
//         });
//         col.Item().SeparatorLine();
//       });
//     }
//
//     private void ComposeTable(IContainer container)
//     {
//       var headerStyle = TextStyle.Default.FontSize(8).SemiBold();
//
//       container.Table(table =>
//       {
//         table.ColumnsDefinition(columns =>
//         {
//           columns.ConstantColumn(25);
//           columns.ConstantColumn(40);
//           columns.RelativeColumn();
//         });
//
//         table.Header(header =>
//         {
//           header.Cell().AlignLeft().Text("Price").Style(headerStyle);
//           header.Cell().AlignLeft().Text("Qty").Style(headerStyle);
//           header.Cell().AlignRight().Text("Item").Style(headerStyle);
//         });
//
//         // string text = "كلام طويل وملوش اخر صدقني انا بقولك انه مش عارف اخره فين ولذلك هقولك كمان شوية";
//         foreach (var item in Model.OrderItems)
//         {
//           table.Cell().Element(CellStyle).AlignLeft().AlignTop().Text($"{item.Price:F0}");
//           table.Cell().Element(CellStyle).AlignLeft().AlignTop().Text(item.Qty);
//           table.Cell().Element(CellStyle).AlignRight().AlignTop().Text(item.ItemName);
//
//           static IContainer CellStyle(IContainer container) => container.PaddingHorizontal(2);
//
//           // .BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
//         }
//       });
//     }
//   }
//
//   // public class AddressComponent : IComponent
//   // {
//   //   private string Title { get; }
//   //   private Address Address { get; }
//   //
//   //   public AddressComponent(string title, Address address)
//   //   {
//   //     Title = title;
//   //     Address = address;
//   //   }
//   //
//   //   public void Compose(IContainer container)
//   //   {
//   //     container.ShowEntire().Column(column =>
//   //     {
//   //       column.Spacing(2);
//   //
//   //       column.Item().Text(Title).SemiBold();
//   //       column.Item().PaddingBottom(5).LineHorizontal(1);
//   //
//   //       column.Item().Text(Address.CompanyName);
//   //       column.Item().Text(Address.Street);
//   //       column.Item().Text($"{Address.City}, {Address.State}");
//   //       column.Item().Text(Address.Email);
//   //       column.Item().Text(Address.Phone);
//   //     });
//   //   }
//   // }
// }