using QuestPDF.Fluent;

namespace FSH.WebApi.Domain.Printing;

public sealed class TextSection : IDocumentSection
{
  public TextSection(int order, SectionAlignment alignment, SectionPosition position, string content)
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
      .AlignCenter().AlignMiddle()
      .Text(Content)
      .FontSize(10);
  }
}