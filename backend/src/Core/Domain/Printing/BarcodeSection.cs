namespace FSH.WebApi.Domain.Printing;

public sealed class BarcodeSection : DocumentSection
{
  private BarcodeSection()
    : this(-1, SectionAlignment.Center, SectionPosition.Header)
  {
  }

  public BarcodeSection(int order, SectionAlignment alignment, SectionPosition position)
    : base(order, alignment, position, false)
  {
  }
}