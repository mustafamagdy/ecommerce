namespace FSH.WebApi.Domain.Printing;

public abstract class TextSection : DocumentSection
{
  protected TextSection(SectionType type, int order, SectionAlignment alignment, SectionPosition position)
    : base(type, order, alignment, position, false)
  {
  }

  public int FontSize { get; set; } = 10;
}

public class TitleSection : TextSection
{
  protected TitleSection(int order, SectionAlignment alignment, SectionPosition position)
    : base(SectionType.Title, order, alignment, position)
  {
  }
}