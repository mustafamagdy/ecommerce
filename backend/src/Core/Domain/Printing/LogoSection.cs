using System.ComponentModel.DataAnnotations.Schema;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Domain.Printing;

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