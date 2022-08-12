using System.ComponentModel.DataAnnotations.Schema;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace FSH.WebApi.Domain.Printing;

public sealed class BarcodeSection : DocumentSection
{
  public BarcodeSection(int order, SectionAlignment alignment, SectionPosition position)
    : base(order, alignment, position, false)
  {
  }

  public override SectionType Type => SectionType.Title;
}