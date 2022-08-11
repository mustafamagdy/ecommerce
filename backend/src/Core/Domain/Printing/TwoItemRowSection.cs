using QuestPDF.Fluent;

namespace FSH.WebApi.Domain.Printing;

public sealed class TwoItemRowSection : IDocumentSection
{
  public TwoItemRowSection(int order, SectionPosition position, string item1Text, string item2Text)
  {
    Order = order;
    Position = position;
    Alignment = SectionAlignment.Center;
    Item1Text = item1Text;
    Item2Text = item2Text;
  }

  public TwoItemRowSection(int order, SectionPosition position, SectionAlignment alignment, string item1Text, string item2Text)
  {
    Order = order;
    Position = position;
    Alignment = alignment;
    Item1Text = item1Text;
    Item2Text = item2Text;
  }

  public SectionType Type => SectionType.Title;
  public SectionAlignment Alignment { get; }
  public int Order { get; }
  public string Item1Text { get; }
  public string Item2Text { get; }
  public SectionPosition Position { get; }

  public void RenderInColumn(ColumnDescriptor col)
  {
    col.Item().Text(Item1Text).SemiBold();
    col.Item().PaddingBottom(5).LineHorizontal(1);
    col.Item().Text(Item2Text);
  }
}