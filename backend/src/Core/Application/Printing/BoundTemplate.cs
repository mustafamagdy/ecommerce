using FSH.WebApi.Domain.Printing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Application.Printing;

public class BoundTemplate
{
  private readonly PrintableDocument _decorated;

  public BoundTemplate(PrintableDocument decorated)
  {
    _decorated = decorated;
  }

  public void BindTemplate<T>(T dto)
    where T : class
  {
    if (dto == null) throw new ArgumentNullException(nameof(dto));

  }
}

public abstract class BoundSection
{
  public abstract void RenderInColumn(ColumnDescriptor col);
}

public class BoundLogoSection : BoundSection
{
  private readonly LogoSection _decorated;

  public BoundLogoSection(LogoSection decorated)
  {
    _decorated = decorated;
  }

  public override void RenderInColumn(ColumnDescriptor col)
  {
    // col.Item().Height(30)
    //   .AlignMiddle().AlignCenter()
    //   .Width(100).Image(ImageData, ImageScaling.Resize);
  }
}