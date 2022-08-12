using System.ComponentModel.DataAnnotations.Schema;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Domain.Printing;

public sealed class LogoSection : DocumentSection
{
  public LogoSection(int order, SectionAlignment alignment, SectionPosition position)
    : base(order, alignment, position, false)
  {
  }

  public override SectionType Type => SectionType.Logo;
}