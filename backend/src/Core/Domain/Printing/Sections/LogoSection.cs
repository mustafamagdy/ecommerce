namespace FSH.WebApi.Domain.Printing.Sections;

public sealed class LogoSection : DocumentSection
{
  private LogoSection()
    : this(-1, SectionAlignment.Center, SectionPosition.Header)
  {
  }

  public LogoSection(int order, SectionAlignment alignment, SectionPosition position)
    : base(order, alignment, position, false)
  {
  }
}