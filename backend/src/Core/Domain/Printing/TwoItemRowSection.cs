using System.ComponentModel.DataAnnotations.Schema;
using QuestPDF.Fluent;

namespace FSH.WebApi.Domain.Printing;

public sealed class TwoItemRowSection : DocumentSection
{
  public TwoItemRowSection(int order, SectionPosition position)
    : this(order, position, SectionAlignment.Center)
  {
  }

  public TwoItemRowSection(int order, SectionPosition position, SectionAlignment alignment)
    : base(SectionType.TwoPartTitle, order, alignment, position, false)
  {
  }
}