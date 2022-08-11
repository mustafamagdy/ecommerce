using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace FSH.WebApi.Domain.Printing;

public sealed class LogoSection : IDocumentSection
{
  public LogoSection(int order, SectionAlignment alignment, SectionPosition position, byte[] imageData)
  {
    Order = order;
    Alignment = alignment;
    Position = position;
    ImageData = imageData;
  }

  public SectionType Type => SectionType.Logo;
  public int Order { get; set; }
  public SectionAlignment Alignment { get; }
  public SectionPosition Position { get; }

  public byte[] ImageData { get; set; }

  public LogoSection SetData(byte[] logoData)
  {
    ImageData = logoData;
    return this;
  }

  public void RenderInColumn(ColumnDescriptor col)
  {
    col.Item().Height(30)
      .AlignMiddle().AlignCenter()
      .Width(100).Image(ImageData, ImageScaling.Resize);
  }
}