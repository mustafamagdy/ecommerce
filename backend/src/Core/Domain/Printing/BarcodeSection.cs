using System.ComponentModel.DataAnnotations.Schema;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace FSH.WebApi.Domain.Printing;

public sealed class BarcodeSection : DocumentSection
{
  public BarcodeSection(int order, SectionAlignment alignment, SectionPosition position)
    : base(SectionType.Title, order, alignment, position, false)
  {
  }
}