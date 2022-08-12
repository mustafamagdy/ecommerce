using System.ComponentModel.DataAnnotations.Schema;
using QuestPDF.Fluent;

namespace FSH.WebApi.Domain.Printing;

public sealed class TwoItemRowSection : DocumentSection
{
  private TwoItemRowSection()
    : this(-1, SectionPosition.Header)
  {
  }

  public TwoItemRowSection(int order, SectionPosition position)
    : this(order, position, SectionAlignment.Center)
  {
  }

  public TwoItemRowSection(int order, SectionPosition position, SectionAlignment alignment)
    : base(order, alignment, position, false)
  {
  }
}