namespace FSH.WebApi.Domain.Printing.Sections;

public abstract class TextSection : DocumentSection
{
  public int FontSize { get; set; } = 10;

  protected TextSection(int order, SectionAlignment alignment, SectionPosition position, bool showDebug)
    : base(order, alignment, position, showDebug)
  {
  }
}

public sealed class TitleSection : TextSection
{
  private TitleSection()
    : this(-1, SectionAlignment.Center, SectionPosition.Header)
  {
  }

  protected TitleSection(int order, SectionAlignment alignment, SectionPosition position)
    : base(order, alignment, position, false)
  {
  }
}