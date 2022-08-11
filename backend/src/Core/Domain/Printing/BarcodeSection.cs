using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace FSH.WebApi.Domain.Printing;

public sealed class BarcodeSection : IDocumentSection
{
  public BarcodeSection(int order, SectionAlignment alignment, SectionPosition position, string content)
  {
    Order = order;
    Alignment = alignment;
    Position = position;
    Content = content;
  }

  public SectionType Type => SectionType.Title;
  public SectionAlignment Alignment { get; }
  public SectionPosition Position { get; }
  public int Order { get; }
  public string Content { get; }

  public void RenderInColumn(ColumnDescriptor col)
  {
    col
      .Item()
      .Height(38)
      .Background(Colors.White)
      .AlignCenter()
      .AlignMiddle()
      .Text($"*{Content}*")
      .LineHeight(1)
      .FontFamily("Libre Barcode 39") // use real font family name
      .FontSize(36);
  }
}