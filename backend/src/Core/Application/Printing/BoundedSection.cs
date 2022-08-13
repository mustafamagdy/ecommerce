using FSH.WebApi.Domain.Printing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace FSH.WebApi.Application.Printing;

public abstract class BoundedSection
{
  public abstract SectionPosition Position { get; }
  public abstract SectionAlignment Alignment { get; }
  public abstract void RenderInColumn(ColumnDescriptor col);
}

public class BoundedTitleSection : BoundedSection
{
  public string Content { get; }
  private readonly TitleSection _decorated;

  public BoundedTitleSection(TitleSection decorated, string content)
  {
    Content = content;
    _decorated = decorated;
  }

  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  public override void RenderInColumn(ColumnDescriptor col)
  {
    col
      .Item()
      // .ShowDebugArea()
      .AlignCenter().AlignMiddle()
      .Text(Content)
      .FontSize(_decorated.FontSize);
  }
}

public class BoundedTwoItemRowSection : BoundedSection
{
  public string Item1 { get; }
  public string Item2 { get; }
  private readonly TwoItemRowSection _decorated;

  public BoundedTwoItemRowSection(TwoItemRowSection decorated, string item1, string item2)
  {
    Item1 = item1;
    Item2 = item2;
    _decorated = decorated;
  }

  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  public override void RenderInColumn(ColumnDescriptor col)
  {
    col.Item().Text(text =>
    {
      text.Span(Item1);
      text.Span(Item2);
    });
  }
}

public class BoundedBarcodeSection : BoundedSection
{
  public string Content { get; }
  private readonly BarcodeSection _decorated;

  public BoundedBarcodeSection(BarcodeSection decorated, string content)
  {
    Content = content;
    _decorated = decorated;
  }

  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  public override void RenderInColumn(ColumnDescriptor col)
  {
    col
      .Item()
      .Height(38)
      // .ShowDebugArea()
      .Background(Colors.White)
      .AlignCenter()
      .AlignMiddle()
      .Text($"*{Content}*")
      .LineHeight(1)
      .FontFamily("Libre Barcode 39") // use real font family name
      .FontSize(36);
  }
}

public class BoundedLogoSection : BoundedSection
{
  private readonly LogoSection _decorated;

  public BoundedLogoSection(LogoSection decorated)
  {
    _decorated = decorated;
  }


  public override SectionPosition Position => _decorated.Position;
  public override SectionAlignment Alignment => _decorated.Alignment;

  public override void RenderInColumn(ColumnDescriptor col)
  {
    // col.Item().Height(30)
    //   .AlignMiddle().AlignCenter()
    //   .Width(100).Image(ImageData, ImageScaling.Resize);
  }
}