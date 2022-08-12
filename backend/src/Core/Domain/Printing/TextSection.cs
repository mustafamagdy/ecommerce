namespace FSH.WebApi.Domain.Printing;

public abstract class TextSection : DocumentSection
{
  protected TextSection(int order, SectionAlignment alignment, SectionPosition position)
    : base(order, alignment, position, false)
  {
  }

  public int FontSize { get; set; } = 10;
  public override SectionType Type => SectionType.Title;
}